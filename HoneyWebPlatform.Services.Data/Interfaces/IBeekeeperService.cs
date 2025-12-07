namespace HoneyWebPlatform.Services.Data.Interfaces
{
    using Web.ViewModels.Beekeeper;
using HoneyWebPlatform.Data.Models;

    public interface IBeekeeperService
    {
        Task<bool> BeekeeperExistsByUserIdAsync(string userId);

        Task<bool> BeekeeperExistsByPhoneNumberAsync(string phoneNumber);

        Task<string> BeekeeperFullnameByHoneyIdAsync(string honeyId);

        Task<string> BeekeeperFullnameByPropolisIdAsync(string honeyId);

        Task Create(string userId, BecomeBeekeeperFormModel model);

        Task<string?> GetBeekeeperIdByUserIdAsync(string userId);

        Task<bool> HasHoneyWithIdAsync(string? userId, string honeyId);

        Task<bool> HasPropolisWithIdAsync(string? userId, string propolisId);

        Task<HoneyWebPlatform.Data.Models.Beekeeper?> GetBeekeeperProfileByIdAsync(string beekeeperId);

        Task<IEnumerable<BeekeeperCardViewModel>> GetAllBeekeepersAsync();
    }
}
