using HoneyWebPlatform.Services.Data.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using HoneyWebPlatform.Services.Data.Models;
using HoneyWebPlatform.Data.Models;

namespace HoneyWebPlatform.Services.Data
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IEmailSender _emailSender;

        public OrderEmailService(IOptions<EmailSettings> emailSettings, IEmailSender emailSender)
        {
            _emailSettings = emailSettings.Value;
            _emailSender = emailSender;
        }

        public async Task SendOrderConfirmationEmailAsync(string customerEmail, Order order, string customerName)
        {
            try
            {
                var subject = "Потвърждение за поръчка - Save The Bee Bulgaria";
                
                var emailBody = $@"
Здравейте {customerName},

Благодарим Ви за поръчката!

Детайли за поръчката:
- Номер на поръчка: {order.Id}
- Дата: {order.CreatedOn:dd.MM.yyyy HH:mm}
- Статус: {order.Status}
- Телефон: {order.PhoneNumber}
- Адрес: {order.Address}
- Обща сума: {order.TotalPrice:F2} лв

Нашият екип ще се свърже с Вас скоро за потвърждение на детайлите и доставката.

С най-добри пожелания,
Екипът на Save The Bee Bulgaria

---
Save The Bee Bulgaria
Email: savethebeebulgaria@gmail.com
Телефон: +359888000000
";

                await _emailSender.SendEmailAsync(customerEmail, subject, emailBody, order.PhoneNumber);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the order creation
                Console.WriteLine($"Failed to send order confirmation email: {ex.Message}");
                throw;
            }
        }

        public async Task SendOrderStatusUpdateEmailAsync(string customerEmail, Order order, string customerName)
        {
            try
            {
                var subject = $"Обновление на статуса на поръчка {order.Id} - Save The Bee Bulgaria";
                
                var statusMessage = order.Status switch
                {
                    OrderStatus.Обработван => "Вашата поръчка е получена и се обработва.",
                    OrderStatus.Приготвен => "Вашата поръчка е приготвена и готова за изпращане.",
                    OrderStatus.Изпратен => "Вашата поръчка е изпратена и е на път към Вас.",
                    OrderStatus.Приключен => "Вашата поръчка е успешно доставена. Благодарим Ви!",
                    _ => "Статусът на вашата поръчка е обновен."
                };

                var emailBody = $@"
Здравейте {customerName},

Статусът на вашата поръчка е обновен.

Детайли за поръчката:
- Номер на поръчка: {order.Id}
- Нов статус: {order.Status}
- Дата на обновление: {DateTime.Now:dd.MM.yyyy HH:mm}
- Обща сума: {order.TotalPrice:F2} лв

{statusMessage}

Ако имате въпроси, не се колебайте да се свържете с нас.

С най-добри пожелания,
Екипът на Save The Bee Bulgaria

---
Save The Bee Bulgaria
Email: savethebeebulgaria@gmail.com
Телефон: +359888000000
";

                await _emailSender.SendEmailAsync(customerEmail, subject, emailBody, order.PhoneNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send order status update email: {ex.Message}");
                throw;
            }
        }

        public async Task SendAdminOrderNotificationAsync(Order order, string customerName)
        {
            try
            {
                var subject = $"Нова поръчка #{order.Id} - Save The Bee Bulgaria Admin";
                
                var emailBody = $@"
Нова поръчка е получена!

Детайли за поръчката:
- Номер на поръчка: {order.Id}
- Клиент: {customerName}
- Имейл: {order.Email}
- Телефон: {order.PhoneNumber}
- Адрес: {order.Address}
- Обща сума: {order.TotalPrice:F2} лв
- Дата: {order.CreatedOn:dd.MM.yyyy HH:mm}
- Статус: {order.Status}

Моля, обработете поръчката възможно най-скоро.

Админ панел: /Admin/Orders/All
";

                await _emailSender.SendEmailAsync("savethebeebulgaria@gmail.com", subject, emailBody, order.PhoneNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send admin order notification: {ex.Message}");
                throw;
            }
        }
    }
}
