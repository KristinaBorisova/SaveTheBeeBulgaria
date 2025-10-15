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
        // private readonly IPropolisService propolisService; // Commented out - propolis functionality disabled
        private readonly IPostService postService;
        private readonly IEmailSender emailSender;
        private readonly IOrderEmailService orderEmailService;
        private readonly ICategoryService categoryService;
        private readonly IBeekeeperService beekeeperService;

        public HomeController(IHoneyService honeyService, IPropolisService propolisService, IPostService postService, IEmailSender emailSender, IOrderEmailService orderEmailService, ICategoryService categoryService, IBeekeeperService beekeeperService)
        {
            this.honeyService = honeyService;
            // this.propolisService = propolisService; // Commented out - propolis functionality disabled
            this.postService = postService;
            this.emailSender = emailSender;
            this.orderEmailService = orderEmailService;
            this.categoryService = categoryService;
            this.beekeeperService = beekeeperService;
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

            // Populate order form data
            ViewBag.OrderFormData = new OrderFormViewModel
            {
                AvailableHoneyTypes = await GetHoneyTypesAsync(),
                AvailableBeekeepers = await GetBeekeepersAsync()
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

        [HttpGet]
        public async Task<IActionResult> GetOrderFormData()
        {
            var orderFormData = new OrderFormViewModel
            {
                AvailableHoneyTypes = await GetHoneyTypesAsync(),
                AvailableBeekeepers = await GetBeekeepersAsync()
            };

            return Json(orderFormData);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrderFromHomepage(OrderFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[ErrorMessage] = "Моля, попълнете всички задължителни полета.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get honey type details
                var honeyTypes = await GetHoneyTypesAsync();
                var selectedHoneyType = honeyTypes.FirstOrDefault(h => h.Id == model.HoneyTypeId);
                var honeyPrice = selectedHoneyType?.Price ?? 0m;

                // Calculate total price
                var totalPrice = honeyPrice * model.Quantity;

                // Create a new order directly from homepage form
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(), // Generate a temporary user ID for guest orders
                    PhoneNumber = model.PhoneNumber ?? "N/A",
                    CreatedOn = DateTime.Now,
                    Email = model.Email,
                    Address = model.Address ?? "N/A",
                    TotalPrice = totalPrice,
                    Status = OrderStatus.Обработван
                };

                // Add order to database
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                    dbContext.Orders.Add(order);
                    await dbContext.SaveChangesAsync();

                    // Add order items
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(), // This would be the actual honey product ID
                        ProductName = selectedHoneyType?.Name ?? "Мед",
                        Quantity = model.Quantity,
                        Price = honeyPrice,
                        OrderId = order.Id
                    };

                    dbContext.OrderItems.Add(orderItem);
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

        private async Task<IEnumerable<HoneyTypeViewModel>> GetHoneyTypesAsync()
        {
            // Filter to only include the specified honey types
            var allowedHoneyTypes = new[] { "Linden", "Bio", "Sunflower" };
            var categories = await categoryService.AllCategoriesAsync();
            
            return categories
                .Where(c => allowedHoneyTypes.Contains(c.Name))
                .Select(c => new HoneyTypeViewModel
                {
                    Id = c.Id,
                    Name = GetBulgarianHoneyName(c.Name),
                    Price = GetHoneyPriceByCategory(c.Name),
                    Description = GetHoneyDescriptionByCategory(c.Name)
                });
        }

        private async Task<IEnumerable<BeekeeperViewModel>> GetBeekeepersAsync()
        {
            // For now, return sample beekeepers. In a real implementation, 
            // you would query the database for active beekeepers
            return new List<BeekeeperViewModel>
            {
                new BeekeeperViewModel
                {
                    Id = Guid.NewGuid(),
                    FullName = "Ивайло Борисов",
                    Location = "Враца",
                    PhoneNumber = "+359886612263"
                },
                new BeekeeperViewModel
                {
                    Id = Guid.NewGuid(),
                    FullName = "Мария Петрова",
                    Location = "Пловдив",
                    PhoneNumber = "+359888234567"
                },
                new BeekeeperViewModel
                {
                    Id = Guid.NewGuid(),
                    FullName = "Иван Стоянов",
                    Location = "Варна",
                    PhoneNumber = "+359888345678"
                }
            };
        }

        private decimal GetHoneyPriceByCategory(string categoryName)
        {
            return categoryName switch
            {
                "Linden" => 15.50m,
                "Bio" => 18.00m,
                "Sunflower" => 12.00m,
                _ => 15.00m
            };
        }

        private string GetBulgarianHoneyName(string categoryName)
        {
            return categoryName switch
            {
                "Linden" => "Липов мед",
                "Bio" => "Билков мед",
                "Sunflower" => "Слънчогледов мед",
                _ => categoryName
            };
        }

        private string GetHoneyDescriptionByCategory(string categoryName)
        {
            return categoryName switch
            {
                "Linden" => "Нежен липов мед с цветен аромат",
                "Bio" => "Билков мед от различни лечебни билки",
                "Sunflower" => "Слънчогледов мед с богат вкус",
                _ => "Висококачествен български мед"
            };
        }
    }
}