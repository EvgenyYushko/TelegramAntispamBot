using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TelegramAntispamBot.BuisinessLogic.Services;
using TelegramAntispamBot.Controllers;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DataAccessLayer.Repositories;
using TelegramAntispamBot.DomainLayer.Repositories;
using TelegramAntispamBot.InjectSettings;
using TelegramAntispamBot.ServiceLayer.Services;

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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddControllers().AddNewtonsoftJson();
			services.AddScoped<IHandleMessageService, HandleMessageService>();
			services.AddScoped<IDeleteMessageService, DeleteMessageService>();
			services.AddScoped<IProfanityCheckerService, ProfanityCheckerService>();
			services.AddScoped<IProfanityCheckerRepository, ProfanityCheckerRepository>();
			services.AddScoped<UsersRepository>();
			services.AddSingleton<IUserInfoService, UserInfoService>();

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

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});

			if (local)
			{
				ApplicationDbContext dbContext;
				var scope = app.ApplicationServices.CreateScope();
				{
					dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					// Вызовите необходимые методы или инициализацию

					var testController = new BotController(new HandleMessageService
						(new DeleteMessageService()
						, new ProfanityCheckerService(new ProfanityCheckerRepository())
						, new UserInfoService(new UsersRepository(dbContext)))
						, _telegram);
					testController.RunLocalTest();
				}
			}

			Console.WriteLine($"Bot {Task.Run(async ()=> await _telegram.TelegramClient.GetMeAsync()).Result.Username} is running...");
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
					const string webhookUrl = "https://telegramantispambot.onrender.com/bot";
					await _telegram.TelegramClient.SetWebhookAsync(webhookUrl);
				}
			}
		}
	}
}
