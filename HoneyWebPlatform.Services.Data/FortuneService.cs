namespace HoneyWebPlatform.Services.Data
{
    using HoneyWebPlatform.Data;
    using HoneyWebPlatform.Data.Models;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class FortuneService : IFortuneService
    {
        private readonly HoneyWebPlatformDbContext dbContext;

        public FortuneService(HoneyWebPlatformDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> CanAccessFortuneTodayAsync(string ipAddress)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                
                var access = await this.dbContext.FortuneAccesses
                    .FirstOrDefaultAsync(fa => fa.IpAddress == ipAddress);

                if (access == null)
                {
                    return true; // No previous access, allow
                }

                // Check if last access was today
                var lastAccessDate = access.LastAccessDate.Date;
                return lastAccessDate < today; // Allow if last access was before today
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Table doesn't exist yet, allow access
                return true;
            }
            catch (Exception)
            {
                // Any other error, allow access to not break the feature
                return true;
            }
        }

        public async Task RecordFortuneAccessAsync(string ipAddress)
        {
            try
            {
                var today = DateTime.UtcNow;
                
                var access = await this.dbContext.FortuneAccesses
                    .FirstOrDefaultAsync(fa => fa.IpAddress == ipAddress);

                if (access == null)
                {
                    // Create new access record
                    access = new FortuneAccess
                    {
                        Id = Guid.NewGuid(),
                        IpAddress = ipAddress,
                        LastAccessDate = today,
                        CreatedOn = today
                    };
                    
                    await this.dbContext.FortuneAccesses.AddAsync(access);
                }
                else
                {
                    // Update existing record
                    access.LastAccessDate = today;
                    this.dbContext.FortuneAccesses.Update(access);
                }

                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Silently fail if table doesn't exist - feature will still work but won't track
                // User can create the table manually when ready
            }
        }
    }
}

