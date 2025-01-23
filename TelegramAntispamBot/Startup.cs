using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using TelegramAntispamBot.Authentication.Handlers;
using TelegramAntispamBot.BackgroundServices;
using TelegramAntispamBot.BuisinessLogic.Services;
using TelegramAntispamBot.BuisinessLogic.Services.Auth;
using TelegramAntispamBot.Controllers;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DataAccessLayer.Repositories;
using TelegramAntispamBot.DomainLayer.Models;
using TelegramAntispamBot.DomainLayer.Models.Auth;
using TelegramAntispamBot.DomainLayer.Repositories;
using TelegramAntispamBot.Enumerations;
using TelegramAntispamBot.InjectSettings;
using TelegramAntispamBot.ServiceLayer.Authorization;
using TelegramAntispamBot.ServiceLayer.Services;
using AuthorizationOptions = TelegramAntispamBot.DomainLayer.Models.Auth.AuthorizationOptions;


namespace TelegramAntispamBot
{
	public class Startup
	{
		public const string TELEGRAM_ANTISPAM_BOT_KEY = "TELEGRAM_ANTISPAM_BOT_KEY";
		private TelegramInject _telegram;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureSettings(IServiceCollection services)
		{
			services.Configure<AppOptions>(Configuration.GetSection(nameof(AppOptions)));
			services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));
			services.Configure<AuthorizationOptions>(Configuration.GetSection(nameof(AuthorizationOptions)));
		}

		public void ConfigureServices(IServiceCollection services)
		{
			ConfigureSettings(services);

			AddAppAuthentication(services, Configuration.GetSection("JwtOptions").Get<JwtOptions>());
			ApdAppAuthorization(services);

			services.AddRazorPages();
			services.AddControllers().AddNewtonsoftJson();
			services.AddScoped<IHandleMessageService, HandleMessageService>();
			services.AddScoped<IDeleteMessageService, DeleteMessageService>();
			services.AddScoped<IProfanityCheckerService, ProfanityCheckerService>();
			services.AddScoped<IProfanityCheckerRepository, ProfanityCheckerRepository>();
			services.AddScoped<UsersRepository>();
			services.AddSingleton<IUserInfoService, UserInfoService>();
			services.AddHostedService<HealthCheckBackgroundService>();

			services.AddScoped<IUsersAccountRepository, UsersAccountRepository>();
			services.AddScoped<IUsersService, UserService>();
			services.AddScoped<IPermissionService, PermissionService>();
			services.AddScoped<IJwtProvider, JwtProvider>();
			services.AddScoped<IPasswordHasher, PasswordHasher>();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

			var botToken = Configuration.GetValue<string>(TELEGRAM_ANTISPAM_BOT_KEY) ?? Environment.GetEnvironmentVariable(TELEGRAM_ANTISPAM_BOT_KEY);
			_telegram = new TelegramInject
			{
				TelegramClient = new TelegramBotClient(botToken ?? throw new Exception("TelegrammToken is not be null"))
			};

			services.AddSingleton(_telegram);

			var connectionString = Environment.GetEnvironmentVariable("DB_URL_POSTGRESQL");
			if (string.IsNullOrEmpty(connectionString))
			{
				connectionString = Configuration.GetConnectionString("DefaultConnection");
			}
			services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			bool local;
			if (env.IsDevelopment())
			{
				local = true;
				app.UseDeveloperExceptionPage();
			}
			else
			{
				local = false;
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
				var dbContext = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();
				dbContext.Database.Migrate();
			}

			Task.Run(async () => await ConfigureWebhookAsync(local));

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});

			if (local)
			{
				var scope = app.ApplicationServices.CreateScope();
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

					var testController = new BotController(new HandleMessageService
						(new DeleteMessageService()
						, new ProfanityCheckerService(new ProfanityCheckerRepository())
						, new UserInfoService(new UsersRepository(dbContext)))
						, _telegram);
					testController.RunLocalTest();
				}
			}

			Console.WriteLine($"Bot {Task.Run(async () => await _telegram.TelegramClient.GetMeAsync()).Result.Username} is running...");
		}

		public async Task ConfigureWebhookAsync(bool local)
		{
			if (local)
			{
				await _telegram.TelegramClient.DeleteWebhookAsync();
			}
			else
			{
				var wh = await _telegram.TelegramClient.GetWebhookInfoAsync();
				if (wh.IpAddress is null)
				{
					var urlSite = Configuration?["AppOptions:Domain"];
					var webhookUrl = $"{urlSite}/bot";
					await _telegram.TelegramClient.SetWebhookAsync(webhookUrl);
				}
			}
		}

		private static void AddAppAuthentication(IServiceCollection services, JwtOptions jwtOptions)
		{
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
				{
					options.TokenValidationParameters = new()
					{
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
					};

					options.Events = new JwtBearerEvents
					{
						OnMessageReceived = context =>
						{
							context.Token = context.Request.Cookies["token"];
							return Task.CompletedTask;
						}
					};
				});
		}

		private static void ApdAppAuthorization(IServiceCollection service)
		{
			service.AddAuthorization(options =>
			{
				options.AddPolicy("Admin", policy => policy.AddRequirements(new PermissionRequirement(Permission.Delete)));
				options.AddPolicy("User", policy => policy.AddRequirements(new PermissionRequirement(Permission.Read)));
				options.AddPolicy("Tutor", policy => policy.AddRequirements(new PermissionRequirement(Permission.Update)));
			});
		}
	}
}
