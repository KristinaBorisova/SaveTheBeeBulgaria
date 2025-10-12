using HoneyWebPlatform.Data.Models;

namespace HoneyWebPlatform.Services.Data.Interfaces
{
    public interface IOrderEmailService
    {
        Task SendOrderConfirmationEmailAsync(string customerEmail, Order order, string customerName);
        Task SendOrderStatusUpdateEmailAsync(string customerEmail, Order order, string customerName);
        Task SendAdminOrderNotificationAsync(Order order, string customerName);
    }
}
