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
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #F59F0A, #D97706); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .order-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .order-item {{ border-bottom: 1px solid #eee; padding: 10px 0; }}
        .total {{ font-weight: bold; font-size: 1.2em; color: #F59F0A; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍯 Save The Bee Bulgaria</h1>
            <h2>Потвърждение за поръчка</h2>
        </div>
        <div class='content'>
            <p>Здравейте <strong>{customerName}</strong>,</p>
            
            <p>Благодарим Ви за поръчката! Вашето заявяване е получено и ще бъде обработено възможно най-скоро.</p>
            
            <div class='order-details'>
                <h3>📋 Детайли за поръчката:</h3>
                <div class='order-item'>
                    <strong>Номер на поръчка:</strong> {order.Id}
                </div>
                <div class='order-item'>
                    <strong>Дата:</strong> {order.CreatedOn:dd.MM.yyyy HH:mm}
                </div>
                <div class='order-item'>
                    <strong>Статус:</strong> {order.Status}
                </div>
                <div class='order-item'>
                    <strong>Телефон:</strong> {order.PhoneNumber}
                </div>
                <div class='order-item'>
                    <strong>Адрес:</strong> {order.Address}
                </div>
                <div class='order-item total'>
                    <strong>Обща сума:</strong> {order.TotalPrice:F2} лв
                </div>
            </div>
            
            <p>Нашият екип ще се свърже с Вас скоро за потвърждение на детайлите и доставката.</p>
            
            <p>Ако имате въпроси, не се колебайте да се свържете с нас.</p>
            
            <div class='footer'>
                <p>С най-добри пожелания,<br><strong>Екипът на Save The Bee Bulgaria</strong></p>
                <hr>
                <p><strong>Save The Bee Bulgaria</strong><br>
                📧 Email: savethebeebulgaria@gmail.com<br>
                📞 Телефон: +359888000000</p>
            </div>
        </div>
    </div>
</body>
</html>";

                await _emailSender.SendEmailAsync(customerEmail, subject, emailBody, order.PhoneNumber);
                Console.WriteLine($"DEBUG: OrderEmailService - Order confirmation email sent successfully to: {customerEmail}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the order creation
                Console.WriteLine($"Failed to send order confirmation email: {ex.Message}");
                Console.WriteLine($"Email error stack trace: {ex.StackTrace}");
                // Don't throw - let the order creation succeed even if email fails
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
                Console.WriteLine($"Email error stack trace: {ex.StackTrace}");
                // Don't throw - let the order update succeed even if email fails
            }
        }

        public async Task SendAdminOrderNotificationAsync(Order order, string customerName)
        {
            try
            {
                var subject = $"Нова поръчка #{order.Id} - Save The Bee Bulgaria Admin";
                
                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #dc2626, #b91c1c); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .order-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .order-item {{ border-bottom: 1px solid #eee; padding: 10px 0; }}
        .urgent {{ background: #fef2f2; border: 1px solid #fecaca; padding: 15px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚨 АДМИН УВЕДОМЛЕНИЕ</h1>
            <h2>Нова поръчка е получена!</h2>
        </div>
        <div class='content'>
            <div class='urgent'>
                <h3>⚠️ Нова поръчка изисква внимание!</h3>
                <p>Моля, обработете поръчката възможно най-скоро.</p>
            </div>
            
            <div class='order-details'>
                <h3>📋 Детайли за поръчката:</h3>
                <div class='order-item'>
                    <strong>Номер на поръчка:</strong> {order.Id}
                </div>
                <div class='order-item'>
                    <strong>Клиент:</strong> {customerName}
                </div>
                <div class='order-item'>
                    <strong>Имейл:</strong> {order.Email}
                </div>
                <div class='order-item'>
                    <strong>Телефон:</strong> {order.PhoneNumber}
                </div>
                <div class='order-item'>
                    <strong>Адрес:</strong> {order.Address}
                </div>
                <div class='order-item'>
                    <strong>Обща сума:</strong> {order.TotalPrice:F2} лв
                </div>
                <div class='order-item'>
                    <strong>Дата:</strong> {order.CreatedOn:dd.MM.yyyy HH:mm}
                </div>
                <div class='order-item'>
                    <strong>Статус:</strong> {order.Status}
                </div>
            </div>
            
            <p><strong>Админ панел:</strong> <a href='https://savethebeebulgaria-production.up.railway.app/Admin/Orders/All'>/Admin/Orders/All</a></p>
            
            <div class='footer'>
                <p>Това е автоматично уведомление от системата на Save The Bee Bulgaria.</p>
            </div>
        </div>
    </div>
</body>
</html>";

                await _emailSender.SendEmailAsync("savethebeebulgaria@gmail.com", subject, emailBody, order.PhoneNumber);
                Console.WriteLine($"DEBUG: OrderEmailService - Admin notification email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send admin order notification: {ex.Message}");
                Console.WriteLine($"Email error stack trace: {ex.StackTrace}");
                // Don't throw - let the order creation succeed even if email fails
            }
        }
    }
}
