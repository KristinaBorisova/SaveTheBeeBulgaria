using System.Text.RegularExpressions;
using HoneyWebPlatform.Web.ViewModels.Honey;
using HoneyWebPlatform.Web.ViewModels.Propolis;

namespace HoneyWebPlatform.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> GetBeekeepersByHoneyType(int categoryId)
        {
            try
            {
                var beekeepers = await GetBeekeepersByCategoryAsync(categoryId);
                return Json(beekeepers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
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
                
                if (selectedHoneyType == null)
                {
                    TempData[ErrorMessage] = "Избраният вид мед не е валиден.";
                    return RedirectToAction("Index", "Home");
                }

                var honeyPrice = selectedHoneyType.Price;

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
                    
                    // Get actual honey product from database using category ID
                    var actualHoney = await dbContext.Honeys
                        .Where(h => h.IsActive && h.CategoryId == selectedHoneyType.Id)
                        .FirstOrDefaultAsync();
                    
                    var productId = actualHoney?.Id ?? Guid.NewGuid(); // Fallback to random GUID if not found
                    
                    dbContext.Orders.Add(order);
                    await dbContext.SaveChangesAsync();

                    // Add order items
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ProductName = selectedHoneyType.Name,
                        Quantity = model.Quantity,
                        Price = honeyPrice,
                        OrderId = order.Id
                    };

                    dbContext.OrderItems.Add(orderItem);
                    await dbContext.SaveChangesAsync();
                }

                // Send order confirmation email (with error handling)
                try
                {
                    await orderEmailService.SendOrderConfirmationEmailAsync(model.Email, order, model.FullName);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the order
                    Console.WriteLine($"Failed to send order confirmation email: {emailEx.Message}");
                }
                
                // Send admin notification (with error handling)
                try
                {
                    await orderEmailService.SendAdminOrderNotificationAsync(order, model.FullName);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the order
                    Console.WriteLine($"Failed to send admin notification email: {emailEx.Message}");
                }

                TempData[SuccessMessage] = $"Успешно създадена поръчка с номер {order.Id}. Проверете имейла си за потвърждение!";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                Console.WriteLine($"Order creation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
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
            // Filter to only include the specified honey types (using Bulgarian names from database)
            var allowedHoneyTypes = new[] { "Липов", "Слънчогледов", "Билков" };
            var categories = await categoryService.AllCategoriesAsync();
            
            return categories
                .Where(c => allowedHoneyTypes.Contains(c.Name))
                .Select(c => new HoneyTypeViewModel
                {
                    Id = c.Id,
                    Name = c.Name, // Use the Bulgarian name directly from database
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

        private async Task<IEnumerable<BeekeeperViewModel>> GetBeekeepersByCategoryAsync(int categoryId)
        {
            // Get beekeepers who have honey products in the specified category
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                
                var beekeepers = await dbContext.Beekeepers
                    .Where(b => b.OwnedHoney.Any(h => h.CategoryId == categoryId && h.IsActive))
                    .Select(b => new BeekeeperViewModel
                    {
                        Id = b.Id,
                        FullName = b.User.FirstName + " " + b.User.LastName,
                        Location = "България", // You might want to add location to beekeeper model
                        PhoneNumber = b.PhoneNumber
                    })
                    .ToListAsync();

                // If no beekeepers found in database, return sample data
                if (!beekeepers.Any())
                {
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
                        }
                    };
                }

                return beekeepers ?? new List<BeekeeperViewModel>();
            }
        }

        private decimal GetHoneyPriceByCategory(string categoryName)
        {
            return categoryName switch
            {
                "Липов" => 15.50m,
                "Билков" => 18.00m,
                "Слънчогледов" => 12.00m,
                "Акациев" => 16.00m,
                "Горски" => 20.00m,
                _ => 15.00m
            };
        }


        private string GetHoneyDescriptionByCategory(string categoryName)
        {
            return categoryName switch
            {
                "Липов" => "Нежен липов мед с цветен аромат",
                "Билков" => "Билков мед от различни лечебни билки",
                "Слънчогледов" => "Слънчогледов мед с богат вкус",
                "Акациев" => "Светъл акациев мед с мека сладост",
                "Горски" => "Тъмен горски мед с богат вкус",
                _ => "Висококачествен български мед"
            };
        }
    }
}