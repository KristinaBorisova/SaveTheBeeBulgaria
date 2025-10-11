namespace HoneyWebPlatform.Web
{
    using System.Reflection;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    using HoneyWebPlatform.Services.Data.Models;
    using HoneyWebPlatform.Web.Areas.Hubs;
    using Hubs;
    using Data;
    using Data.Models;
    using Infrastructure.Extensions;
    using Infrastructure.ModelBinders;
    using Services.Data.Interfaces;
    using Services.Mapping;
    using ViewModels.Home;
    using static Common.GeneralApplicationConstants;

    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            // Handle Railway environment variables
            if (string.IsNullOrEmpty(connectionString))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    // Parse Railway's DATABASE_URL format: postgresql://user:password@host:port/database
                    var uri = new Uri(databaseUrl);
                    var userInfo = uri.UserInfo.Split(':');
                    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;";
                }
                else
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found and DATABASE_URL environment variable is not set.");
                }
            }

            builder.Services.AddDbContext<HoneyWebPlatformDbContext>(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // Use SQLite for local development
                    options.UseSqlite(connectionString);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
                else
                {
                    // Use PostgreSQL for production
                    options.UseNpgsql(connectionString);
                }
            });

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount =
                    builder.Configuration.GetValue<bool>("Identity:SignIn:RequireConfirmedAccount");
                options.Password.RequireLowercase =
                    builder.Configuration.GetValue<bool>("Identity:Password:RequireLowercase");
                options.Password.RequireUppercase =
                    builder.Configuration.GetValue<bool>("Identity:Password:RequireUppercase");
                options.Password.RequireNonAlphanumeric =
                    builder.Configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric");
                options.Password.RequiredLength =
                    builder.Configuration.GetValue<int>("Identity:Password:RequiredLength");
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<HoneyWebPlatformDbContext>();

            // Add email settings configuration here
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddApplicationServices(typeof(IHoneyService));
            // Print the list of added services
            //PrintAddedServicesForEntities(builder.Services);


            builder.Services.AddRecaptchaService();

            builder.Services.AddMemoryCache();
            builder.Services.AddResponseCaching();

            builder.Services.AddSignalR();

            builder.Services.ConfigureApplicationCookie(cfg =>
            {
                cfg.LoginPath = "/User/Login";
                cfg.AccessDeniedPath = "/Home/Error/401";
            });

            builder.Services
                .AddControllersWithViews()
                .AddMvcOptions(options =>
                {
                    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
                    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
                });

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add health checks
            builder.Services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "postgresql");

            // Add localization services - COMMENTED OUT FOR SIMPLICITY
            // builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            // builder.Services.Configure<RequestLocalizationOptions>(options =>
            // {
            //     var supportedCultures = new[] { "en-US", "bg-BG" };
            //     options.SetDefaultCulture("bg-BG")
            //         .AddSupportedCultures(supportedCultures)
            //         .AddSupportedUICultures(supportedCultures);
            // });

            // External authentication providers - only add if properly configured
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

            var authBuilder = builder.Services.AddAuthentication();

            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret) && 
                googleClientId != "YOUR_GOOGLE_CLIENT_ID")
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }

            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret) && 
                facebookAppId != "YOUR_FACEBOOK_APP_ID")
            {
                authBuilder.AddFacebook(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                });
            }


            WebApplication app = builder.Build();

            // Ensure database is created and migrated
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                try
                {
                    // Use Migrate() instead of EnsureCreated() for production
                    if (app.Environment.IsProduction())
                    {
                        context.Database.Migrate();
                    }
                    else
                    {
                        context.Database.EnsureCreated();
                    }
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while creating/migrating the database.");
                    // Don't rethrow in production to prevent container restart loops
                    if (!app.Environment.IsProduction())
                    {
                        throw;
                    }
                }
            }

            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error/500");
                app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");

                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Add localization middleware - COMMENTED OUT FOR SIMPLICITY
            // app.UseRequestLocalization();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add health check endpoint
            app.MapHealthChecks("/health");

            app.EnableOnlineUsersCheck();

            if (app.Environment.IsDevelopment())
            {
                app.SeedAdministrator(DevelopmentAdminEmail);
            }

            app.UseEndpoints(config =>
            {
                config.MapControllerRoute(
                    name: "areas",
                    pattern: "/{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                config.MapControllerRoute(
                    name: "ProtectingUrlRoute",
                    pattern: "/{controller}/{action}/{id}/{information}",
                    defaults: new { Controller = "Category", Action = "Details" });

                config.MapDefaultControllerRoute();
                config.MapRazorPages();
                config.MapHub<ChatHub>("/chatHub");
                config.MapHub<CartHub>("/cartHub");
                //for admin order panel
                config.MapHub<OrderHub>("/orderHub");
            });


            app.Run();
        }

        /// Checking if all the services are added for each entity
        private static void PrintAddedServicesForEntities(IServiceCollection services)
        {
            var entityNames = new[] { "Beekeeper", "Honey", "Post", "Flavour", "Category", "Propolis", "SubscribedEmail", "User" /* Add other entity names here */ };

            Console.WriteLine("Added Services for Entities:");
            foreach (var entityName in entityNames)
            {
                var matchingServices = services
                    .Where(service => service.ServiceType.Name.Contains(entityName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingServices.Any())
                {
                    Console.WriteLine($"- {entityName} Services:");
                    foreach (var service in matchingServices)
                    {
                        Console.WriteLine($"  - {service.ServiceType.Name} | Implementation: {service.ImplementationType?.Name ?? "N/A"}");
                    }
                    Console.WriteLine();
                }
            }
        }

    }

}