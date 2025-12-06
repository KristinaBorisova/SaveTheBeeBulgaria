namespace HoneyWebPlatform.Web
{
    using System.Reflection;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

using HoneyWebPlatform.Services.Data.Models;
using HoneyWebPlatform.Web.Areas.Hubs;
using Hubs;
using Data;
using Data.Models;
using Infrastructure.Extensions;
using Infrastructure.ModelBinders;
using Services.Data.Interfaces;
using Services.Data;
using Services.Mapping;
using ViewModels.Home;
using static Common.GeneralApplicationConstants;

    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            // Debug logging for connection string
            Console.WriteLine($"Initial connection string: {(string.IsNullOrEmpty(connectionString) ? "NULL" : "SET")}");
            
            // Handle Railway environment variables
            if (string.IsNullOrEmpty(connectionString))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                Console.WriteLine($"DATABASE_URL: {(string.IsNullOrEmpty(databaseUrl) ? "NULL" : "SET")}");
                
                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    try
                    {
                        // Parse Railway's DATABASE_URL format: postgresql://user:password@host:port/database
                        var uri = new Uri(databaseUrl);
                        var userInfo = uri.UserInfo.Split(':');
                        
                        // Handle port parsing more safely
                        var port = uri.Port > 0 ? uri.Port : 5432; // Default PostgreSQL port
                        
                        connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;";
                        
                        Console.WriteLine($"Parsed connection string successfully for host: {uri.Host}, port: {port}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
                        Console.WriteLine($"DATABASE_URL: {databaseUrl}");
                        
                        // Try alternative Railway environment variables
                        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
                        var pgPort = Environment.GetEnvironmentVariable("PGPORT");
                        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
                        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
                        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
                        
                        if (!string.IsNullOrEmpty(pgHost))
                        {
                            var port = !string.IsNullOrEmpty(pgPort) ? int.Parse(pgPort) : 5432;
                            connectionString = $"Host={pgHost};Port={port};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;";
                            Console.WriteLine($"Using alternative Railway environment variables");
                        }
                        else
                        {
                            // Fallback to default connection string
                            connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                        }
                    }
                }
                else
                {
                    // Try alternative Railway environment variables
                    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
                    var pgPort = Environment.GetEnvironmentVariable("PGPORT");
                    var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
                    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
                    var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
                    
                    if (!string.IsNullOrEmpty(pgHost))
                    {
                        var port = !string.IsNullOrEmpty(pgPort) ? int.Parse(pgPort) : 5432;
                        connectionString = $"Host={pgHost};Port={port};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;";
                        Console.WriteLine($"Using Railway environment variables: PGHOST={pgHost}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Connection string 'DefaultConnection' not found and DATABASE_URL environment variable is not set.");
                    }
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
                    // Suppress pending model changes warning in production (database may have extra columns from reverted migrations)
                    options.ConfigureWarnings(warnings => 
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
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

            // Configure Data Protection for Railway deployment
            // Use basic configuration that works with .NET 6.0
            builder.Services.AddDataProtection();

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
                    // Temporarily disabled AutoValidateAntiforgeryTokenAttribute to test if it's causing reloading issues
                    // options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
                });

            // Configure email services
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            
            // Configure HttpClient for Resend and register ResendEmailProvider
            builder.Services.AddHttpClient<ResendEmailProvider>();
            builder.Services.AddTransient<IOrderEmailService, ResendEmailProvider>();

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

            // Configure URL binding for Railway (must listen on PORT env var)
            // In .NET 9.0, ASP.NET Core automatically reads PORT, but we'll set it explicitly for Railway
            var webPort = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(webPort))
            {
                // Configure the web host to listen on Railway's PORT
                // Railway expects the app to listen on 0.0.0.0 (all interfaces) on the PORT env var
                builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");
                Console.WriteLine($"[STARTUP] Configured to listen on http://0.0.0.0:{webPort}");
            }
            else
            {
                // Also check ASPNETCORE_URLS as fallback
                var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
                if (!string.IsNullOrEmpty(urls))
                {
                    builder.WebHost.UseUrls(urls);
                    Console.WriteLine($"[STARTUP] Configured to listen on URLs from ASPNETCORE_URLS: {urls}");
                }
                else
                {
                    // Default fallback - listen on port 80
                    builder.WebHost.UseUrls("http://0.0.0.0:80");
                    Console.WriteLine("[STARTUP] PORT and ASPNETCORE_URLS not set. Using default: http://0.0.0.0:80");
                }
            }

            WebApplication app = builder.Build();

        // Ensure upload directories exist for persistent storage
        var uploadPaths = new[]
        {
            Path.Combine(app.Environment.WebRootPath, "uploads", "HoneyPictures"),
            Path.Combine(app.Environment.WebRootPath, "uploads", "PropolisPictures"),
            Path.Combine(app.Environment.WebRootPath, "uploads", "PostPictures"),
            Path.Combine(app.Environment.WebRootPath, "uploads", "HivePictures"),
            Path.Combine(app.Environment.WebRootPath, "uploads", "UsersProfilePictures")
        };

            foreach (var path in uploadPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"Created upload directory: {path}");
                }
            }

            // Ensure database is created and migrated (run asynchronously to not block startup)
            // This allows the app to start listening immediately while migration happens in background
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2)); // Give app time to start listening
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        
                        logger.LogInformation("Starting database migration...");
                        Console.WriteLine("Starting database migration...");
                        
                        // Use Migrate() instead of EnsureCreated() for production
                        if (app.Environment.IsProduction())
                        {
                            // Check if database exists and can connect
                            if (context.Database.CanConnect())
                            {
                                // Try to apply migrations, but don't fail if there are pending model changes
                                // (this can happen if database has extra columns from reverted migrations)
                                try
                                {
                                    context.Database.Migrate();
                                    logger.LogInformation("Database migration completed successfully.");
                                    Console.WriteLine("Database migration completed successfully.");
                                }
                                catch (InvalidOperationException ex) when (
                                    ex.Message.Contains("pending changes") || 
                                    ex.Message.Contains("PendingModelChangesWarning") ||
                                    ex.InnerException?.Message?.Contains("pending changes") == true)
                                {
                                    logger.LogWarning("Database has pending model changes (likely from reverted migrations). Skipping migration. Database is accessible.");
                                    Console.WriteLine($"Warning: Database has pending model changes but is accessible. Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                // Database doesn't exist, create it
                                context.Database.Migrate();
                                logger.LogInformation("Database created and migrated successfully.");
                                Console.WriteLine("Database created and migrated successfully.");
                            }
                        }
                        else
                        {
                            context.Database.EnsureCreated();
                            logger.LogInformation("Database ensured created.");
                            Console.WriteLine("Database ensured created.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Use a separate scope for logging the error
                    using (var scope = app.Services.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while creating/migrating the database.");
                        
                        // Log connection string details for debugging (without sensitive data)
                        logger.LogError("Connection string details: {ConnectionString}", 
                            connectionString?.Replace("Password=", "Password=***").Replace("Pwd=", "Pwd=***"));
                    }
                    
                    Console.WriteLine($"Database migration error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Don't throw - allow app to continue running even if migration fails
                }
            });

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

            // Only use HTTPS redirection if not on Railway (Railway handles HTTPS at proxy level)
            // Check if we're on Railway by checking for PORT environment variable
            var isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT"));
            if (!isRailway)
            {
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            // Add localization middleware - COMMENTED OUT FOR SIMPLICITY
            // app.UseRequestLocalization();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add health check endpoint (includes database check)
            app.MapHealthChecks("/health");
            
            // Add simple ping endpoint that doesn't require database - for Railway readiness checks
            app.MapGet("/ping", () => Results.Ok(new { 
                status = "ok", 
                timestamp = DateTime.UtcNow,
                message = "Application is running and ready to accept requests"
            }));
            
            // Add a simple readiness endpoint for Railway
            app.MapGet("/ready", () => Results.Ok(new { 
                status = "ready", 
                timestamp = DateTime.UtcNow,
                port = Environment.GetEnvironmentVariable("PORT") ?? "not set"
            }));

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

            // Log startup information
            try
            {
                var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
                var httpPort = Environment.GetEnvironmentVariable("PORT") ?? "Not set";
                var urls = builder.Configuration["ASPNETCORE_URLS"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "Not configured";
                startupLogger.LogInformation("Application starting. Environment: {Environment}, PORT: {Port}, URLs: {Urls}", 
                    app.Environment.EnvironmentName, httpPort, urls);
                Console.WriteLine($"Application is ready. Environment: {app.Environment.EnvironmentName}");
                Console.WriteLine($"PORT environment variable: {httpPort}");
                Console.WriteLine($"ASPNETCORE_URLS: {urls}");
                if (httpPort != "Not set")
                {
                    Console.WriteLine($"Application will listen on: http://0.0.0.0:{httpPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during startup logging: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            // Log that we're about to start listening
            var finalPort = Environment.GetEnvironmentVariable("PORT") ?? "80";
            Console.WriteLine($"[STARTUP] ========================================");
            Console.WriteLine($"[STARTUP] Application is starting and will listen on port {finalPort}");
            Console.WriteLine($"[STARTUP] Environment: {app.Environment.EnvironmentName}");
            Console.WriteLine($"[STARTUP] ========================================");
            
            // Wrap app.Run in try-catch to catch any unhandled exceptions
            try
            {
                Console.WriteLine("[STARTUP] Starting Kestrel server...");
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] Application crashed: {ex.Message}");
                Console.WriteLine($"[FATAL ERROR] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[FATAL ERROR] Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw to ensure Railway restarts the container
            }
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