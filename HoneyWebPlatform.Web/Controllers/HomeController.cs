using System.Text.RegularExpressions;
using HoneyWebPlatform.Web.ViewModels.Honey;
using HoneyWebPlatform.Web.ViewModels.Propolis;

namespace HoneyWebPlatform.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using Services.Data.Interfaces;
    using HoneyWebPlatform.Data.Models;
    using HoneyWebPlatform.Data;

    using ViewModels.Home;
    using ViewModels.User;
    using static Common.GeneralApplicationConstants;
    using static Common.NotificationMessagesConstants;

    public class HomeController : Controller
    {
        private readonly IHoneyService honeyService;
        private readonly IPropolisService propolisService;
        private readonly IPostService postService;
        private readonly IEmailSender emailSender;
        private readonly IOrderEmailService orderEmailService;

        public HomeController(IHoneyService honeyService, IPropolisService propolisService, IPostService postService, IEmailSender emailSender, IOrderEmailService orderEmailService)
        {
            this.honeyService = honeyService;
            this.propolisService = propolisService;
            this.postService = postService;
            this.emailSender = emailSender;
            this.orderEmailService = orderEmailService;
        }

        public async Task<IActionResult> Index()
        {
            if (this.User.IsInRole(AdminRoleName))
            {
                return this.RedirectToAction("Index", "Home", new { Area = AdminAreaName });
            }

            IEnumerable<HoneyAllViewModel> honeyIndexViewModel =
                await honeyService.LastThreeHoneysAsync();

            // Removed propolis from homepage as requested
            // IEnumerable<PropolisAllViewModel> propolisIndexViewModel =
            //    await propolisService.LastThreePropolisеsAsync();

            IEnumerable<PostIndexViewModel> postIndexViewModel =
                await postService.LastThreePostsAsync();

            var viewModel = new IndexViewModel
            {
                Honeys = honeyIndexViewModel,
                Propolises = new List<PropolisAllViewModel>(), // Empty list instead of propolis data
                Posts = postIndexViewModel
            };


            return View(viewModel);
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Unsubscribe()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SendEmail(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Validation failed, return the same view with validation errors
                TempData[ErrorMessage] = "Имейлът не беше изпратен! Проверете въведените данни и опитайте отново.";
                return View("Contact", model);
            }

            try
            {
                // Construct the email message body
                var emailMessage = 
                    $"Name: {model.Name}\nEmail: {model.Email}\nPhone Number: {model.Number}\nSubject: {model.Subject}\nMessage: {model.Message}";

                // Use the EmailSender service to send the email
                await emailSender.SendEmailAsync("savethebeebulgaria@gmail.com", model.Subject, emailMessage, model.Number);

                TempData[SuccessMessage] = "Благодарим Ви за имейла!";

                // Optionally, redirect to a success page or return a success message
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                // Handle errors (e.g., log the error, display error message to user)
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrderFromHomepage(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[ErrorMessage] = "Моля, попълнете всички задължителни полета.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Create a new order directly from homepage form
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(), // Generate a temporary user ID for guest orders
                    PhoneNumber = model.PhoneNumber ?? "N/A",
                    CreatedOn = DateTime.Now,
                    Email = model.Email,
                    Address = model.Address ?? "N/A",
                    TotalPrice = 0, // Will be calculated when products are added
                    Status = OrderStatus.Обработван
                };

                // Add order to database
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                    dbContext.Orders.Add(order);
                    await dbContext.SaveChangesAsync();
                }

                // Send order confirmation email
                await orderEmailService.SendOrderConfirmationEmailAsync(model.Email, order, model.FullName);
                
                // Send admin notification
                await orderEmailService.SendAdminOrderNotificationAsync(order, model.FullName);

                TempData[SuccessMessage] = $"Успешно създадена поръчка с номер {order.Id}. Проверете имейла си за потвърждение!";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData[ErrorMessage] = $"Грешка при създаване на поръчка: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }







        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 400 || statusCode == 404)
            {
                return View("Error404");
            }

            if (statusCode == 401)
            {
                return View("Error401");
            }

            return View();
        }
    }
}