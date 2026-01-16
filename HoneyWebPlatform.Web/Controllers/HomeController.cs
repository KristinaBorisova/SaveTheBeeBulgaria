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
    using Services.Data;

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
        private readonly IServiceProvider serviceProvider;
        private readonly IFortuneService fortuneService;

        public HomeController(IHoneyService honeyService, IPostService postService, IEmailSender emailSender, IOrderEmailService orderEmailService, ICategoryService categoryService, IBeekeeperService beekeeperService, IServiceProvider serviceProvider, IFortuneService fortuneService)
        {
            this.honeyService = honeyService;
            // this.propolisService = propolisService; // Commented out - propolis functionality disabled
            this.postService = postService;
            this.emailSender = emailSender;
            this.orderEmailService = orderEmailService;
            this.categoryService = categoryService;
            this.beekeeperService = beekeeperService;
            this.serviceProvider = serviceProvider;
            this.fortuneService = fortuneService;
        }

        public async Task<IActionResult> Index()
        {
            // Debug: Log when Index is called
            Console.WriteLine($"DEBUG: Index action called at {DateTime.Now}");
            
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

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> HoneyFortune()
        {
            // Get client IP address
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Handle IPv4-mapped IPv6 addresses
            if (ipAddress != null && ipAddress.StartsWith("::ffff:"))
            {
                ipAddress = ipAddress.Substring(7);
            }
            
            // Fallback if IP is null
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "unknown";
            }

            // Check if user can access fortune today
            bool canAccess = await this.fortuneService.CanAccessFortuneTodayAsync(ipAddress);
            
            ViewBag.CanAccessFortune = canAccess;
            
            string? fortuneText = null;
            
            if (canAccess)
            {
                // Check if there's already a fortune for today
                fortuneText = await this.fortuneService.GetTodayFortuneAsync(ipAddress);
                
                // If no fortune exists for today, generate a new one
                if (string.IsNullOrEmpty(fortuneText))
                {
                    fortuneText = GetRandomFortune();
                    // Store the new fortune
                    await this.fortuneService.RecordFortuneAccessAsync(ipAddress, fortuneText);
                }
            }
            else
            {
                // User already accessed today, get their stored fortune
                fortuneText = await this.fortuneService.GetTodayFortuneAsync(ipAddress);
            }
            
            // Pass the fortune to the view
            ViewBag.FortuneText = fortuneText;

            return View();
        }

        private string GetRandomFortune()
        {
            var fortunes = new List<string>
            {
                "Късмет! Днес ще намериш нещо сладко, което не е мед.",
                "Любов! Твоето сърце ще се усмихне.",
                "Пари! Нещо неочаквано ще попълни портфейла ти.",
                "Успех! Твоите усилия ще дадат плодове по-скоро, отколкото очакваш.",
                "Енергията ти ще бъде заразна, НО в добрия смисъл. :) ",
                "Ново приятелство!",
                "Приключение! Днес ще се случи нещо за което да разказваш и след 10 години.",
                "Спокойствие! Вътрешният ти глас ще ти каже точно какво да правиш.",
                "Вдъхновение! Идея, която чакаше, ще дойде днес.",
                "Щастие! Днес ще усетиш, че си точно там, където трябва да си.",
                "Смях! Нещо забавно ще се случи.",
                "Успех! Всичко което те притеснява ще се разреши по най-добрия начин.",
                "Енергия! Днес ще имаш сили за всичко, което искаш.",
                "Грация! Ще се справиш с всяка ситуация.",
                "Мъдрост! Решението, което търсиш, вече е в теб.",
                "Хармония! Днес всичко ще се подреди по най-добрия начин.",
                "Нещо, което обичаш, ще ти донесе радост.",
                "Благодарност! Ще забележиш нещо малко, но хубаво.",
                "Смелост! Това е твоят знак да направиш стъпка, която дълго отлагаш.",
                "Творчество! Идеята, която чакаше, ще дойде днес.",
                "Баланс! Днес ще намериш време за всичко важно.",
                "Любов. Какво повече му трябва на човек?",
                "Свързаност! Някой близък ще ти направи деня по-специален.",
                "Изобилие! Днес ще получиш повече, отколкото очакваш.",
                "Лекота! Всичко, което ти тежи, ще стане по-леко.",
                "Ще забележиш нещо прекрасно в ежедневието си.",
                "Сила! Всяко предизвикателство ще ти покаже колко си способен.",
                "Пари, пари и още пари!",
                "Възможности! Врата, която мислеше, че е затворена - ще се отвори.",
                "Радост! Нещо малко ще направи денят ти по-специален.",
                "Любопитство! Ще научиш нещо ново, което ще те зарадва.",
                "Доброто което правиш ще се върне при теб.",
                "Търпение! Всичко, което чакаш, ще дойде в правилния момент.",
                "Вяра! Ще повярваш в себе си по-силно от вчера.",
                "Ти можеш! Ще се справиш с всяка ситуация.",
                "Вдъхновение! Нещо ще те мотивира да направиш следващата стъпка.",
                "Забава! Днес ще се усмиеш поне веднъж от сърце.",
                "Свобода! Ще се почувстваш по-освободен от всякога.",
                "Любов към живота! Днес ще усетиш колко е хубав всъщност.",
                "Успех! Твоите мечти са по-близо, отколкото мислиш.",
                "Благополучие! Всичко в живота ти ще тече по-плавно.",
                "Радост! Малко нещо ще направи денят ти по-специален.",
                "Късмет!",
                "Енергия! Ще имаш сили за всичко, което искаш да направиш.",
                "Хармония! Днес всичко ще се подреди по най-добрия начин.",
                "Мъдрост! Решението, което търсиш, вече е в теб.",
                "Днес ще усетиш, че си точно там, където трябва да бъдеш.",
                "Надежда! Всичко, което те притеснява, ще се разреши по най-добрия начин.",
                "Любов! Твоето сърце ще се усмихне днес.",
                "Пари! Нещо неочаквано ще попълни портфейла ти."
            };

            var random = new Random();
            return fortunes[random.Next(fortunes.Count)];
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Test()
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderFromHomepage(OrderFormViewModel model)
        {
            try
            {
                // Debug: Log when PlaceOrderFromHomepage is called
                Console.WriteLine($"DEBUG: PlaceOrderFromHomepage called at {DateTime.Now}");
                Console.WriteLine($"DEBUG: ModelState.IsValid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    // Collect all validation errors for display
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData[ErrorMessage] = $"Моля, коригирайте следните грешки: {string.Join(", ", errors)}";
                    return RedirectToAction("Index", "Home");
                }
                
                // Get honey type details
                var honeyTypes = await GetHoneyTypesAsync();
                var honeyTypesList = honeyTypes.ToList();
                
                if (!honeyTypesList.Any())
                {
                    TempData[ErrorMessage] = "Няма налични видове мед. Моля, свържете се с администратор.";
                    return RedirectToAction("Index", "Home");
                }
                
                var selectedHoneyType = honeyTypesList.FirstOrDefault(h => h.Id == model.HoneyTypeId);
                
                if (selectedHoneyType == null)
                {
                    TempData[ErrorMessage] = $"Избраният вид мед (ID: {model.HoneyTypeId}) не е валиден. Моля, изберете от списъка.";
                    return RedirectToAction("Index", "Home");
                }

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
                        // If no users exist, we'll need to handle this differently
                        TempData[ErrorMessage] = "Системата не е настроена правилно. Моля, свържете се с администратор.";
                        return RedirectToAction("Index", "Home");
                    }
                    
                    // Create a new order directly from homepage form
                    var order = new Order
                    {
                        // Let Entity Framework handle ID generation
                        UserId = guestUserId, // Use valid user ID for guest orders
                        PhoneNumber = model.PhoneNumber ?? "N/A",
                        CreatedOn = DateTime.UtcNow, // Use UTC time for PostgreSQL
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
                        dbContext.Orders.Add(order);
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception dbEx)
                    {
                        
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
                    }
                    catch (Exception dbEx)
                    {
                        throw;
                    }
                    
                    // Set success message first (before email sending)
                    TempData[SuccessMessage] = $"Успешно създадена поръчка с номер {order.Id}. Проверете имейла си за потвърждение!";

        // Send emails in background (non-blocking)
        _ = Task.Run(async () =>
        {
            // Create a new scope for background email sending
            using (var scope = serviceProvider.CreateScope())
            {
                var backgroundOrderEmailService = scope.ServiceProvider.GetRequiredService<IOrderEmailService>();
                
                try
                {
                    await backgroundOrderEmailService.SendOrderConfirmationEmailAsync(model.Email, order, model.FullName);
                }
                catch (Exception emailEx)
                {
                    // Email sending failed, but order was created successfully
                }
                
                try
                {
                    await backgroundOrderEmailService.SendAdminOrderNotificationAsync(order, model.FullName);
                }
                catch (Exception emailEx)
                {
                    // Email sending failed, but order was created successfully
                }
            }
        });

                    return RedirectToAction("Index", "Home");
                }
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