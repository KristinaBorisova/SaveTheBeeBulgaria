namespace HoneyWebPlatform.Services.Data
{
    using Microsoft.EntityFrameworkCore;

    using HoneyWebPlatform.Data;
    using HoneyWebPlatform.Data.Models;
    using Interfaces;
    using Web.ViewModels.Beekeeper;

    public class BeekeeperService : IBeekeeperService
    {
        private readonly HoneyWebPlatformDbContext dbContext;

        public BeekeeperService(HoneyWebPlatformDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> BeekeeperExistsByUserIdAsync(string userId)
        {
            bool result = await dbContext
                .Beekeepers
                .AnyAsync(a => a.UserId.ToString() == userId);

            return result;
        }

        public async Task<string> BeekeeperFullnameByHoneyIdAsync(string honeyId)
        {
            var honey = await dbContext
                .Honeys
                .Include(h => h.Beekeeper)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(h => h.Id.ToString() == honeyId);

            if (honey?.Beekeeper?.User == null)
            {
                return "Неизвестен пчелар";
            }

            var firstName = honey.Beekeeper.User.FirstName ?? "";
            var lastName = honey.Beekeeper.User.LastName ?? "";
            
            return $"{firstName} {lastName}".Trim();
        }
        public async Task<string> BeekeeperFullnameByPropolisIdAsync(string propolisId)
        {
            var propolis = await dbContext
                .Propolises
                .Include(p => p.Beekeeper)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(h => h.Id.ToString() == propolisId);

            if (propolis?.Beekeeper?.User == null)
            {
                return "Неизвестен пчелар";
            }

            var firstName = propolis.Beekeeper.User.FirstName ?? "";
            var lastName = propolis.Beekeeper.User.LastName ?? "";
            
            return $"{firstName} {lastName}".Trim();
        }

        public async Task<bool> BeekeeperExistsByPhoneNumberAsync(string phoneNumber)
        {
            bool result = await dbContext
                .Beekeepers
                .AnyAsync(a => a.PhoneNumber == phoneNumber);

            return result;
        }

        public async Task Create(string userId, BecomeBeekeeperFormModel model)
        {
            Beekeeper newBeekeeper = new Beekeeper()
            {
                PhoneNumber = model.PhoneNumber,
                UserId = Guid.Parse(userId),
                HiveFarmPicturePaths = model.HivePicturePath
            };

            await dbContext.Beekeepers.AddAsync(newBeekeeper);

            await dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetBeekeeperIdByUserIdAsync(string userId)
        {
            Beekeeper? beekeeper = await dbContext
                .Beekeepers
                .FirstOrDefaultAsync(a => a.UserId.ToString() == userId);
            if (beekeeper == null)
            {
                return null;
            }

            return beekeeper.Id.ToString();
        }

        public async Task<bool> HasHoneyWithIdAsync(string? userId, string honeyId)
        {
            Beekeeper? beekeeper = await dbContext
                .Beekeepers
                .Include(a => a.OwnedHoney)
                .FirstOrDefaultAsync(a => a.UserId.ToString() == userId);
            if (beekeeper == null)
            {
                return false;
            }

            honeyId = honeyId.ToLower();
            return beekeeper.OwnedHoney.Any(h => h.Id.ToString() == honeyId);
        }
        public async Task<bool> HasPropolisWithIdAsync(string? userId, string propolisId)
        {
            Beekeeper? beekeeper = await dbContext
                .Beekeepers
                .Include(a => a.OwnedPropolis)
                .FirstOrDefaultAsync(a => a.UserId.ToString() == userId);
            if (beekeeper == null)
            {
                return false;
            }

            propolisId = propolisId.ToLower();
            return beekeeper.OwnedPropolis.Any(h => h.Id.ToString() == propolisId);
        }

        public async Task<HoneyWebPlatform.Data.Models.Beekeeper?> GetBeekeeperProfileByIdAsync(string beekeeperId)
        {
            return await dbContext
                .Beekeepers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id.ToString() == beekeeperId);
        }
    }
}
