using HoneyWebPlatform.Services.Data.Interfaces;
using HoneyWebPlatform.Data.Models;
using Resend;

namespace HoneyWebPlatform.Services.Data
{
    public class ResendOrderEmailService : IOrderEmailService
    {
        private readonly IResend _resend;

        public ResendOrderEmailService(IResend resend)
        {
            _resend = resend;
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

                var message = new EmailMessage();
                message.From = "Save The Bee Bulgaria <noreply@savethebeebulgaria.com>";
                message.To.Add(customerEmail);
                message.Subject = subject;
                message.HtmlBody = emailBody;

                await _resend.EmailSendAsync(message);
                Console.WriteLine($"DEBUG: Order confirmation email sent successfully to {customerEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Failed to send order confirmation email: {ex.Message}");
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
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #F59F0A, #D97706); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .status-update {{ background: #e7f3ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #F59F0A; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍯 Save The Bee Bulgaria</h1>
            <h2>Обновление на статуса на поръчка</h2>
        </div>
        <div class='content'>
            <p>Здравейте <strong>{customerName}</strong>,</p>
            
            <div class='status-update'>
                <h3>📦 Статус на поръчката</h3>
                <p><strong>Номер на поръчка:</strong> {order.Id}</p>
                <p><strong>Нов статус:</strong> {order.Status}</p>
                <p><strong>Дата на обновление:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                <p><strong>Обща сума:</strong> {order.TotalPrice:F2} лв</p>
            </div>
            
            <p><strong>{statusMessage}</strong></p>
            
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

                var message = new EmailMessage();
                message.From = "Save The Bee Bulgaria <noreply@savethebeebulgaria.com>";
                message.To.Add(customerEmail);
                message.Subject = subject;
                message.HtmlBody = emailBody;

                await _resend.EmailSendAsync(message);
                Console.WriteLine($"DEBUG: Order status update email sent successfully to {customerEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Failed to send order status update email: {ex.Message}");
                throw;
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

                var message = new EmailMessage();
                message.From = "Save The Bee Bulgaria <noreply@savethebeebulgaria.com>";
                message.To.Add("savethebeebulgaria@gmail.com");
                message.Subject = subject;
                message.HtmlBody = emailBody;

                await _resend.EmailSendAsync(message);
                Console.WriteLine($"DEBUG: Admin order notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Failed to send admin order notification: {ex.Message}");
                throw;
            }
        }
    }
}
