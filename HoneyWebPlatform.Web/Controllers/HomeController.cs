using System.Text.RegularExpressions;
using System.Linq;
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

        public HomeController(IHoneyService honeyService, IPostService postService, IEmailSender emailSender, IOrderEmailService orderEmailService, ICategoryService categoryService, IBeekeeperService beekeeperService)
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

            // Get Ивайло Борисов's beekeeper ID
            string? ivayloBorisovId = await GetIvayloBorisovBeekeeperIdAsync();

            var viewModel = new IndexViewModel
            {
                Honeys = honeyIndexViewModel,
                Propolises = new List<PropolisAllViewModel>(), // Empty list instead of propolis data
                Posts = postIndexViewModel,
                IvayloBorisovBeekeeperId = ivayloBorisovId
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
            try
            {
                // Debug: Log that the method was called
                Console.WriteLine($"DEBUG: PlaceOrderFromHomepage called with model: {model?.FullName ?? "NULL"}");
                
                // Debug: Log model state
                Console.WriteLine($"DEBUG: ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"DEBUG: Validation Error: {error.ErrorMessage}");
                    }
                    
                    // Collect all validation errors for display
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData[ErrorMessage] = $"Моля, коригирайте следните грешки: {string.Join(", ", errors)}";
                    return RedirectToAction("Index", "Home");
                }
                // Debug: Log form data
                Console.WriteLine($"DEBUG: Form Data - FullName: {model.FullName}, Email: {model.Email}, Phone: {model.PhoneNumber}, Address: {model.Address}");
                Console.WriteLine($"DEBUG: Form Data - HoneyTypeId: {model.HoneyTypeId}, BeekeeperId: {model.BeekeeperId}, Quantity: {model.Quantity}");
                
                // Get honey type details
                var honeyTypes = await GetHoneyTypesAsync();
                var honeyTypesList = honeyTypes.ToList();
                
                Console.WriteLine($"DEBUG: Available honey types count: {honeyTypesList.Count}");
                
                if (!honeyTypesList.Any())
                {
                    Console.WriteLine("DEBUG: No honey types available, redirecting with error");
                    TempData[ErrorMessage] = "Няма налични видове мед. Моля, свържете се с администратор.";
                    return RedirectToAction("Index", "Home");
                }
                
                // Check if we have the required data for order creation
                Console.WriteLine("DEBUG: Checking required data for order creation");
                
                var selectedHoneyType = honeyTypesList.FirstOrDefault(h => h.Id == model.HoneyTypeId);
                
                if (selectedHoneyType == null)
                {
                    Console.WriteLine($"DEBUG: Selected honey type not found. HoneyTypeId: {model.HoneyTypeId}");
                    Console.WriteLine($"DEBUG: Available honey types: {string.Join(", ", honeyTypesList.Select(h => $"{h.Id}:{h.Name}"))}");
                    TempData[ErrorMessage] = $"Избраният вид мед (ID: {model.HoneyTypeId}) не е валиден. Моля, изберете от списъка.";
                    return RedirectToAction("Index", "Home");
                }
                
                Console.WriteLine($"DEBUG: Selected honey type: {selectedHoneyType.Name} (ID: {selectedHoneyType.Id})");

                var honeyPrice = selectedHoneyType.Price;

                // Calculate total price
                var totalPrice = honeyPrice * model.Quantity;

                // Add order to database
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                    
                    // Get the first available user ID (or create a guest user)
                    var guestUserId = await dbContext.Users
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
                    
                    if (guestUserId == Guid.Empty)
                    {
                        Console.WriteLine("DEBUG: No users found in database, creating guest order with random ID");
                        // If no users exist, we'll need to handle this differently
                        TempData[ErrorMessage] = "Системата не е настроена правилно. Моля, свържете се с администратор.";
                        return RedirectToAction("Index", "Home");
                    }
                    
                    Console.WriteLine($"DEBUG: Using guest user ID: {guestUserId}");
                    
                    // Create a new order directly from homepage form
                    var order = new Order
                    {
                        // Let Entity Framework handle ID generation
                        UserId = guestUserId, // Use valid user ID for guest orders
                        PhoneNumber = model.PhoneNumber ?? "N/A",
                        CreatedOn = DateTime.Now,
                        Email = model.Email ?? "N/A",
                        Address = model.Address ?? "N/A",
                        TotalPrice = totalPrice,
                        Status = OrderStatus.Обработван
                    };
                    
                    // Get actual honey product from database using category ID
                    var actualHoney = await dbContext.Honeys
                        .Where(h => h.IsActive && h.CategoryId == selectedHoneyType.Id)
                        .FirstOrDefaultAsync();
                    
                    var productId = actualHoney?.Id ?? Guid.NewGuid(); // Fallback to random GUID if not found
                    
                    try
                    {
                        Console.WriteLine($"DEBUG: Attempting to save order with ID: {order.Id}");
                        Console.WriteLine($"DEBUG: Order details - Email: '{order.Email}', Address: '{order.Address}', Phone: '{order.PhoneNumber}', TotalPrice: {order.TotalPrice}");
                        
                        dbContext.Orders.Add(order);
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"DEBUG: Order saved successfully with ID: {order.Id}");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"DEBUG: Error saving order: {dbEx.Message}");
                        Console.WriteLine($"DEBUG: Inner Exception: {dbEx.InnerException?.Message}");
                        Console.WriteLine($"DEBUG: Stack Trace: {dbEx.StackTrace}");
                        Console.WriteLine($"DEBUG: Order data - Email: '{order.Email}', Address: '{order.Address}', Phone: '{order.PhoneNumber}'");
                        
                        // Try to get more specific error information
                        if (dbEx.InnerException != null)
                        {
                            Console.WriteLine($"DEBUG: Inner exception details: {dbEx.InnerException.Message}");
                        }
                        
                        TempData[ErrorMessage] = $"Грешка при създаване на поръчка: {dbEx.InnerException?.Message ?? dbEx.Message}";
                        return RedirectToAction("Index", "Home");
                    }

                    // Add order items
                    var orderItem = new OrderItem
                    {
                        // Let Entity Framework handle ID generation
                        ProductId = productId,
                        ProductName = selectedHoneyType.Name ?? "Unknown Product",
                        Quantity = model.Quantity,
                        Price = honeyPrice,
                        OrderId = order.Id
                    };

                    try
                    {
                        dbContext.OrderItems.Add(orderItem);
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"DEBUG: OrderItem saved successfully with ID: {orderItem.Id}");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"DEBUG: Error saving order item: {dbEx.Message}");
                        Console.WriteLine($"DEBUG: OrderItem data - ProductName: '{orderItem.ProductName}', ProductId: '{orderItem.ProductId}'");
                        throw;
                    }
                }

                // Send order confirmation email (with error handling)
                try
                {
                    Console.WriteLine($"DEBUG: Attempting to send order confirmation email to {model.Email}");
                    await orderEmailService.SendOrderConfirmationEmailAsync(model.Email, order, model.FullName);
                    Console.WriteLine("DEBUG: Order confirmation email sent successfully");
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the order
                    Console.WriteLine($"DEBUG: Failed to send order confirmation email: {emailEx.Message}");
                    Console.WriteLine($"DEBUG: Email error stack trace: {emailEx.StackTrace}");
                }
                
                // Send admin notification (with error handling)
                try
                {
                    Console.WriteLine("DEBUG: Attempting to send admin notification email");
                    await orderEmailService.SendAdminOrderNotificationAsync(order, model.FullName);
                    Console.WriteLine("DEBUG: Admin notification email sent successfully");
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the order
                    Console.WriteLine($"DEBUG: Failed to send admin notification email: {emailEx.Message}");
                    Console.WriteLine($"DEBUG: Admin email error stack trace: {emailEx.StackTrace}");
                }

                TempData[SuccessMessage] = $"Успешно създадена поръчка с номер {order.Id}. Проверете имейла си за потвърждение!";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                Console.WriteLine($"Order creation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Set a simple error message and redirect
                TempData[ErrorMessage] = $"Грешка при създаване на поръчка: {ex.Message}";
                
                // Always try to redirect to home page
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

                // If no beekeepers found in database, try to get Ивайло Борисов specifically
                if (!beekeepers.Any())
                {
                    var ivayloBorisov = await GetIvayloBorisovBeekeeperAsync(dbContext);
                    if (ivayloBorisov != null)
                    {
                        return new List<BeekeeperViewModel> { ivayloBorisov };
                    }
                    
                    // Fallback to sample data if Ивайло Борисов not found
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

        private async Task<BeekeeperViewModel?> GetIvayloBorisovBeekeeperAsync(HoneyWebPlatformDbContext dbContext)
        {
            var beekeeper = await dbContext.Beekeepers
                .Where(b => b.User.FirstName == "Ивайло" && b.User.LastName == "Борисов")
                .Select(b => new BeekeeperViewModel
                {
                    Id = b.Id,
                    FullName = b.User.FirstName + " " + b.User.LastName,
                    Location = "Враца",
                    PhoneNumber = b.PhoneNumber
                })
                .FirstOrDefaultAsync();

            return beekeeper;
        }

        private async Task<string?> GetIvayloBorisovBeekeeperIdAsync()
        {
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HoneyWebPlatformDbContext>();
                
                var beekeeperId = await dbContext.Beekeepers
                    .Where(b => b.User.FirstName == "Ивайло" && b.User.LastName == "Борисов")
                    .Select(b => b.Id.ToString())
                    .FirstOrDefaultAsync();

                return beekeeperId;
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