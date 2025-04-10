using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BuisinessLogic.Handlers;
using BuisinessLogic.Services.Authorization;
using BuisinessLogic.Services.Facades;
using BuisinessLogic.Services.Parsers;
using BuisinessLogic.Services.Telegram;
using DataAccessLayer;
using DataAccessLayer.Repositories;
using DomainLayer.Models.Authorization;
using DomainLayer.Repositories;
using GoogleServices.Drive;
using GoogleServices.Gemini;
using GoogleServices.Interfaces;
using Infrastructure.Common;
using Infrastructure.Enumerations;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using Infrastructure.Models.AI;
using MailSenderService.BuisinessLogic.Services;
using MailSenderService.ServiceLayer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ML_SpamClassifier;
using ML_SpamClassifier.Interfaces;
using Quartz;
using QuartzHostedService;
using ServiceLayer.Services.AI;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using TelegramAntispamBot.BackgroundServices;
using TelegramAntispamBot.Controllers;
using TelegramAntispamBot.Filters;
using TelegramAntispamBot.Jobs;
using TelegramAntispamBot.Jobs.Base;
using static Infrastructure.Constants.TelegramConstatns;
using static Infrastructure.Helpers.Logger;
using AuthorizationOptions = DomainLayer.Models.Authorization.AuthorizationOptions;
using static TelegramAntispamBot.Jobs.Helpers.JobHelper;

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
			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(10);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			services.Configure<AppOptions>(Configuration.GetSection(nameof(AppOptions)));
			services.Configure<MailOptions>(Configuration.GetSection(nameof(MailOptions)));
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

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.MinimumSameSitePolicy = SameSiteMode.None;
				options.Secure = CookieSecurePolicy.Always;
			});
		}

		public void ConfigureServices(IServiceCollection services)
		{
			ConfigureSettings(services);
			ConfigureAuthorization(services);

			var env = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
			var environmentName = env.EnvironmentName;

			services.AddRazorPages();
			services.AddControllers().AddNewtonsoftJson();
			services.AddHttpContextAccessor();
			services.AddScoped<IHandleMessageService, HandleMessageService>();
			services.AddScoped<IDeleteMessageService, DeleteMessageService>();
			services.AddScoped<IProfanityCheckerService, ProfanityCheckerService>();
			services.AddScoped<IProfanityCheckerRepository, ProfanityCheckerRepository>();
			if (env.IsProduction())
			{
				services.AddSingleton<ITelegramUserRepository, TelegramUserRepository>();
				services.AddSingleton<ITelegramUserService, TelegramUserService>();
				services.AddSingleton<IMLService, MLService>();
				services.AddSingleton<ExternalAuthManager>();
				services.AddSingleton<ISpamDetector, SpamDetector>();
				services.AddSingleton<MLFacade>();
			}
			else
			{
				services.AddScoped<ITelegramUserRepository, TelegramUserRepository>();
				services.AddScoped<ITelegramUserService, TelegramUserService>();
				services.AddScoped<IMLService, MLService>();
				services.AddScoped<ExternalAuthManager>();
				services.AddScoped<ISpamDetector, SpamDetector>();
				services.AddScoped<MLFacade>();
			}
			services.AddSingleton<IMailService, MailService>();
			services.AddSingleton<INewsParserServiceAI, NewsParserServiceAI>();
			services.AddSingleton<IValidationErrorServiceAI, ValidationErrorServiceAI>();
			services.AddHostedService<HealthCheckBackgroundService>();

			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IPermissionService, PermissionService>();
			services.AddScoped<IJwtProvider, JwtProvider>();
			services.AddScoped<IPasswordHasher, PasswordHasher>();
			services.AddScoped<ILogRepository, LogRepository>();

			services.AddQuartz(q =>
			{
				q.SchedulerId = "MainScheduler";
				q.UseMicrosoftDependencyInjectionJobFactory();

				var timeZone = TimeZoneHelper.GetTimeZoneInfo();

				foreach (var job in JobSettings)
				{
					var jobKey = new JobKey($"{job.Key}Job");
					q.AddJob(job.Type, jobKey, j => j.StoreDurably());

					if (!job.Castum)
					{
						q.AddTrigger(t => t
							.WithIdentity($"{job.Key}Trigger")
							.ForJob(jobKey)
							.WithCronSchedule($"{job.Time}", x => x.InTimeZone(timeZone))
						);
					}
				}
			});

			// Запуск Quartz как фоновой службы
			services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

			// Получаем IScheduler для проверки расписаний
			services.AddTransient(provider =>
			{
				var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
				return schedulerFactory.GetScheduler().GetAwaiter().GetResult();
			});
			services.AddTransient<ScheduleInspectorService>();

			var serviceAccountJson = Configuration.GetValue<string>(GOOGLE_DRIVE_SERVICE_ACCOUNT_JSON) ?? Environment.GetEnvironmentVariable(GOOGLE_DRIVE_SERVICE_ACCOUNT_JSON);
			services.AddSingleton<IGoogleDriveUploader>(provider =>
			{
				return new GoogleDriveUploader(serviceAccountJson);
			});

			var geminiApiKey = Configuration.GetValue<string>(GEMINI_API_KEY) ?? Environment.GetEnvironmentVariable(GEMINI_API_KEY);
			services.AddSingleton<IGenerativeLanguageModel>(provider =>
			{
				return new GenerativeLanguageModel(geminiApiKey);
			});

			services.AddSingleton<NbrbCurrencyParser>();
			services.AddSingleton<HabrParser>();
			services.AddSingleton<OnlinerParser>();
			services.AddSingleton<AllNewsParser>();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

			var botToken = Configuration.GetValue<string>(TELEGRAM_ANTISPAM_BOT_KEY) ?? Environment.GetEnvironmentVariable(TELEGRAM_ANTISPAM_BOT_KEY);
			_telegram = new TelegramInject
			{
				TelegramClient = new TelegramBotClient(botToken ?? throw new Exception("TelegrammToken is not be null"))
			};

			services.AddSingleton(_telegram);

			var mailOption = Configuration.GetSection("MailOptions").Get<MailOptions>();
			mailOption.SenderPassword = Configuration.GetValue<string>(SENDER_PASSWORD) ?? Environment.GetEnvironmentVariable(SENDER_PASSWORD);

			services.AddSingleton(mailOption);

			AddFilters(services);

			var connectionString = Environment.GetEnvironmentVariable("DB_URL_POSTGRESQL");
			if (string.IsNullOrEmpty(connectionString))
			{
				connectionString = Configuration.GetConnectionString("DefaultConnection");
			}
			services.AddDbContext<ApplicationDbContext>(options =>
			options.UseNpgsql(connectionString, options => options.MigrationsAssembly("DataAccessLayer")));
			services.AddIdentity<UserEntity, RoleEntity>(options =>
			{
				options.User.RequireUniqueEmail = true;

				options.SignIn.RequireConfirmedAccount = false;
				options.SignIn.RequireConfirmedPhoneNumber = false;
				options.SignIn.RequireConfirmedEmail = false;

				// временно отключим требования к паролю
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 6;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;

				// отключим требования к логину
				options.User.AllowedUserNameCharacters = null;

			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			services.Configure<IdentityOptions>(options =>
			{
				//options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Время блокировки
				//options.Lockout.MaxFailedAccessAttempts = 5; // Количество попыток
				options.Lockout.AllowedForNewUsers = false; // Применять ли блокировку для новых пользователей
			});
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
				//app.UseExceptionHandler("/Error");
				app.UseDeveloperExceptionPage();

				app.UseHsts();
				var dbContext = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();
				dbContext.Database.Migrate();
			}

			// Важно: включаем логику распознавания X-Forwarded-Proto
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			Task.Run(async () => await ConfigureWebhookAsync(local));

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRequestLocalization();
			app.UseCookiePolicy();
			app.UseRouting();
			app.UseSession();
			app.UseAuthentication();
			app.UseAuthorization();
			//app.UseMvc();  
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});

			Log($"Using timezone: {TimeZoneHelper.GetTimeZoneInfo().DisplayName}");
			Log($"Current local time: {TimeZoneHelper.DateTimeNow}");

			using (var scope = app.ApplicationServices.CreateScope())
			{
				var inspector = scope.ServiceProvider.GetRequiredService<ScheduleInspectorService>();
				Task.Run(async () => await inspector.InitializeAsync()).Wait();
				Task.Run(async () => await inspector.PrintScheduleInfo()).Wait();

				if (!local)
				{
					var ml = scope.ServiceProvider.GetRequiredService<MLFacade>();
					Task.Run(async () => await ml.LoadModel()).Wait();
				}
			}

			if (local)
			{
				var scope = app.ApplicationServices.CreateScope();
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
					var googleDriveUploader = scope.ServiceProvider.GetRequiredService<IGoogleDriveUploader>();
					var generativeLanguageModel = scope.ServiceProvider.GetRequiredService<IGenerativeLanguageModel>();
					var mlService = scope.ServiceProvider.GetRequiredService<IMLService>();
					var telegreamUserService = new TelegramUserService(new TelegramUserRepository(dbContext));
					var ml = scope.ServiceProvider.GetRequiredService<MLFacade>();
					//Task.Run(async () => await ml.LoadModel()).Wait();

					//var aiService = scope.ServiceProvider.GetRequiredService<INewsParserServiceAI>();
					//var h = new HabrParser(aiService);
					//var res = Task.Run(async ()=> await h.ParseLatestPostAsync(new ParseParams { ChatTitle = "Здоровое питание" })).Result;

					//var parser = new AllNewsParser(aiService);
					//var s = Task.Run(async ()=> await parser.ParseAllNewsRss(new ParseParams 
					//{ 
					//	ChatTitle = "Политика"
					//	, lastMessages = new System.Collections.Generic.List<string>()
					//	{
					//		"Трамп и его ации",
					//		"Кто призедент нашей страны??"
					//	} 
					//})).Result;

					var testController = new BotController
						(
							new HandleMessageService
							(
								new DeleteMessageService()
								, new ProfanityCheckerService(new ProfanityCheckerRepository())
								, telegreamUserService
								, userService
								, new SpamDetector(generativeLanguageModel, mlService, env)
								, mlService
								, ml
							)
							, _telegram
						);
					testController.RunLocalTest();
				}
			}

			Log($"Bot {Task.Run(async () => await _telegram.TelegramClient.GetMeAsync()).Result.Username} is running...");
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

		private void AddAppAuthentication(IServiceCollection services, JwtOptions jwtOptions)
		{
			services.AddAuthentication(options =>
				{
					// Схема по умолчанию для аутентификации из cookies
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					// Схема для вызова аутентификации (challenge) – Google
					//options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
				})
				.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
				{
					options.Cookie.SameSite = SameSiteMode.None;
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				}) // Регистрация cookie-аутентификации
				.AddGoogle(options =>
				{
					options.ClientId = Configuration.GetValue<string>(GOOGLE_CLIENT_ID) ?? Environment.GetEnvironmentVariable(GOOGLE_CLIENT_ID);
					options.ClientSecret = Configuration.GetValue<string>(GOOGLE_CLIENT_SECRET) ?? Environment.GetEnvironmentVariable(GOOGLE_CLIENT_SECRET);
					options.CallbackPath = new PathString("/signin-google");
				})
				.AddVkontakte(options =>
				{
					options.ClientId = Configuration.GetValue<string>(VK_CLIENT_ID) ?? Environment.GetEnvironmentVariable(VK_CLIENT_ID);
					options.ClientSecret = Configuration.GetValue<string>(VK_CLIENT_SECRET) ?? Environment.GetEnvironmentVariable(VK_CLIENT_SECRET);
					// Scope
					options.Scope.Clear();
					options.Scope.Add("email");

					// Fields
					options.Fields.Clear();
					options.Fields.Add("photo_200,email,first_name,last_name");

					// Маппинг claims
					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
					options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
					options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
					options.ClaimActions.MapJsonKey("urn:vkontakte:photo", "photo_200");

					options.CallbackPath = new PathString("/signin-vkontakte");
				})
				.AddGitHub(options => // Добавьте это
				{
					options.ClientId = Configuration.GetValue<string>(GITHUB_CLIENT_ID) ?? Environment.GetEnvironmentVariable(GITHUB_CLIENT_ID);
					options.ClientSecret = Configuration.GetValue<string>(GITHUB_CLIENT_SECRET) ?? Environment.GetEnvironmentVariable(GITHUB_CLIENT_SECRET);
					options.CallbackPath = new PathString("/signin-github");

					// Scope для доступа к email (если нужно)
					options.Scope.Add("user:email");

					// Маппинг claims (опционально)
					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
					options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

					//// Если нужны дополнительные поля:
					//options.Events.OnCreatingTicket = async context =>
					//{
					//	if (context.AccessToken != null)
					//	{
					//		var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
					//		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
					//		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					//		var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
					//		response.EnsureSuccessStatusCode();

					//		var emails = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
					//		foreach (var email in emails.RootElement.EnumerateArray())
					//		{
					//			if (email.GetProperty("primary").GetBoolean())
					//			{
					//				context.Identity?.AddClaim(new Claim(ClaimTypes.UserName, email.GetProperty("email").GetString()));
					//				break;
					//			}
					//		}
					//	}
					//};
				})
				.AddMailRu(options =>
				{
					options.ClientId = Configuration.GetValue<string>(MAILRU_CLIENT_ID) ?? Environment.GetEnvironmentVariable(MAILRU_CLIENT_ID);
					options.ClientSecret = Configuration.GetValue<string>(MAILRU_CLIENT_SECRET) ?? Environment.GetEnvironmentVariable(MAILRU_CLIENT_SECRET);
					options.CallbackPath = new PathString("/signin-mailru");

					// Настройка маппинга claims
					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
					options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
					options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
					options.ClaimActions.MapJsonKey("urn:mailru:image", "image");
				})
				.AddMicrosoftAccount(options =>
				{
					options.ClientId = Configuration.GetValue<string>(MICROSOFT_CLIENT_ID) ?? Environment.GetEnvironmentVariable(MICROSOFT_CLIENT_ID);
					options.ClientSecret = Configuration.GetValue<string>(MICROSOFT_CLIENT_SECRET) ?? Environment.GetEnvironmentVariable(MICROSOFT_CLIENT_SECRET);

					// Расширенные настройки
					options.CallbackPath = new PathString("/signin-microsoft");
					options.AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint + "?prompt=select_account";
					options.Scope.Add("openid");
					options.Scope.Add("profile");
					options.Scope.Add("email");
					options.ClaimActions.MapJsonKey(ClaimTypes.Email, "mail");
					options.SaveTokens = true;

				})
				//.AddTelegramAuth(opt =>
				//{
				//	opt.CallbackPath = new PathString("/signin-github");

				//})
				.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
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
				options.AddPolicy(nameof(Role.Admin), policy => policy.AddRequirements(new PermissionRequirement(Permission.Delete)));
				options.AddPolicy(nameof(Role.User), policy => policy.AddRequirements(new PermissionRequirement(Permission.Read)));
				options.AddPolicy(nameof(Role.Tutor), policy => policy.AddRequirements(new PermissionRequirement(Permission.Update)));
			});
		}
	}
}
