namespace HoneyWebPlatform.Services.Data.Interfaces
{
    public interface IFortuneService
    {
        Task<bool> CanAccessFortuneTodayAsync(string ipAddress);
        
        Task RecordFortuneAccessAsync(string ipAddress, string fortuneText);
        
        Task<string?> GetTodayFortuneAsync(string ipAddress);
    }
}

