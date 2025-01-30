using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BuisinessLogic.Handlers;
using BuisinessLogic.Services.Authorization;
using BuisinessLogic.Services.Telegram;
using DataAccessLayer;
using DataAccessLayer.Repositories;
using DomainLayer.Models.Authorization;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using TelegramAntispamBot.BackgroundServices;
using TelegramAntispamBot.Controllers;
using TelegramAntispamBot.Filters;
using static Infrastructure.Constants.TelegramConstatns;
using AuthorizationOptions = DomainLayer.Models.Authorization.AuthorizationOptions;

namespace TelegramAntispamBot
{
	public class Startup
	{
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

			services.AddLocalization(options => options.ResourcesPath = "Resources");
			services.AddControllersWithViews()
				.AddViewLocalization();// добавляем локализацию представлений;

			services.Configure<RequestLocalizationOptions>(options =>
			{
				var supportedCultures = new[]
				{
					new CultureInfo("en"),
					new CultureInfo("ru"),
				};

				options.DefaultRequestCulture = new RequestCulture("en");
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
			});
		}

		public void ConfigureServices(IServiceCollection services)
		{
			ConfigureSettings(services);
			ConfigureAuthorization(services);

			services.AddRazorPages();
			services.AddControllers().AddNewtonsoftJson();
			services.AddHttpContextAccessor();
			services.AddScoped<IHandleMessageService, HandleMessageService>();
			services.AddScoped<IDeleteMessageService, DeleteMessageService>();
			services.AddScoped<IProfanityCheckerService, ProfanityCheckerService>();
			services.AddScoped<IProfanityCheckerRepository, ProfanityCheckerRepository>();
			services.AddSingleton<ITelegramUserRepository, TelegramUserRepository>();
			services.AddSingleton<ITelegramUserService, TelegramUserService>();
			services.AddHostedService<HealthCheckBackgroundService>();

			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IPermissionService, PermissionService>();
			services.AddScoped<IJwtProvider, JwtProvider>();
			services.AddScoped<IPasswordHasher, PasswordHasher>();
			services.AddScoped<ILogRepository, LogRepository>();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

			var botToken = Configuration.GetValue<string>(TELEGRAM_ANTISPAM_BOT_KEY) ?? Environment.GetEnvironmentVariable(TELEGRAM_ANTISPAM_BOT_KEY);
			_telegram = new TelegramInject
			{
				TelegramClient = new TelegramBotClient(botToken ?? throw new Exception("TelegrammToken is not be null"))
			};

			services.AddSingleton(_telegram);

			AddFilters(services);

			var connectionString = Environment.GetEnvironmentVariable("DB_URL_POSTGRESQL");
			if (string.IsNullOrEmpty(connectionString))
			{
				connectionString = Configuration.GetConnectionString("DefaultConnection");
			}

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseNpgsql(connectionString, options => options.MigrationsAssembly("DataAccessLayer")));
		}

		private static void AddFilters(IServiceCollection services)
		{
			services.AddScoped<LogPageFilter>();
		}

		private void ConfigureAuthorization(IServiceCollection services)
		{
			AddAppAuthentication(services, Configuration.GetSection("JwtOptions").Get<JwtOptions>());
			ApdAppAuthorization(services);
		}

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
				app.UseHsts();
				var dbContext = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();
				dbContext.Database.Migrate();
			}

			Task.Run(async () => await ConfigureWebhookAsync(local));

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRequestLocalization();
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
						, new TelegramUserService(new TelegramUserRepository(dbContext)))
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
					var urlSite = Configuration?[$"{nameof(AppOptions)}:Domain"];
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
