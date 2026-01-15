namespace HoneyWebPlatform.Services.Data.Interfaces
{
    public interface IDatabaseHealthService
    {
        Task<bool> IsDatabaseConnectedAsync();
        Task<string> GetDatabaseStatusAsync();
        Task<bool> CanCreateUserAsync();
    }
}
