using Microsoft.EntityFrameworkCore;
using HoneyWebPlatform.Data;
using HoneyWebPlatform.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace HoneyWebPlatform.Services.Data
{
    public class DatabaseHealthService : IDatabaseHealthService
    {
        private readonly HoneyWebPlatformDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public DatabaseHealthService(HoneyWebPlatformDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<bool> IsDatabaseConnectedAsync()
        {
            try
            {
                // Try to execute a simple query to check database connectivity
                await context.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetDatabaseStatusAsync()
        {
            try
            {
                // Check if database can be reached
                var canConnect = await IsDatabaseConnectedAsync();
                if (!canConnect)
                {
                    return "Database connection failed";
                }

                // Check if migrations are up to date
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    return $"Database has {pendingMigrations.Count()} pending migrations";
                }

                // Check if we can query the Users table (most critical for registration)
                var userCount = await context.Users.CountAsync();
                return $"Database connected successfully. Users table accessible with {userCount} users.";
            }
            catch (Exception ex)
            {
                return $"Database error: {ex.Message}";
            }
        }

        public async Task<bool> CanCreateUserAsync()
        {
            try
            {
                // Check basic database connectivity
                if (!await IsDatabaseConnectedAsync())
                {
                    return false;
                }

                // Check if we can access the Users table
                await context.Users.FirstOrDefaultAsync();
                
                // Check if UserManager is properly configured
                var testUser = new ApplicationUser
                {
                    UserName = "test@test.com",
                    Email = "test@test.com"
                };

                // Try to validate user creation without actually creating
                var validationResult = await userManager.ValidateAsync(testUser);
                return validationResult.Succeeded;
            }
            catch
            {
                return false;
            }
        }
    }
}
