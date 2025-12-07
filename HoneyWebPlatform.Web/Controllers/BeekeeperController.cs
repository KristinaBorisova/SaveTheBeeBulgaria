namespace HoneyWebPlatform.Web.Controllers
{
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Infrastructure.Extensions;
using Services.Data.Interfaces;
using ViewModels.Beekeeper;
using ViewModels.Honey;

using static Common.NotificationMessagesConstants;

    public class BeekeeperController : Controller
    {
        private readonly IBeekeeperService beekeeperService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHoneyService honeyService;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;

        public BeekeeperController(
            IBeekeeperService beekeeperService, 
            IWebHostEnvironment webHostEnvironment, 
            IHoneyService honeyService,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            this.beekeeperService = beekeeperService;
            this.webHostEnvironment = webHostEnvironment;
            this.honeyService = honeyService;
            this.emailSender = emailSender;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Become()
        {
            string? userId = User.GetId();
            bool isBeekeeper = await beekeeperService.BeekeeperExistsByUserIdAsync(userId!);
            if (isBeekeeper)
            {
                TempData[ErrorMessage] = "You are already a Beekeeper!";

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Become(BecomeBeekeeperFormModel model)
        {
            string? userId = User.GetId();
            bool isBeekeeper = await beekeeperService.BeekeeperExistsByUserIdAsync(userId!);
            if (isBeekeeper)
            {
                TempData[ErrorMessage] = "You are already a Beekeeper!";
                return RedirectToAction("Index", "Home");
            }

            bool isPhoneNumberTaken =
                await beekeeperService.BeekeeperExistsByPhoneNumberAsync(model.PhoneNumber);
            if (isPhoneNumberTaken)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber),
                    "A Beekeeper with the provided phone number already exists!");
            }

            try
            {
                // Picture saving logic
                if (model.HivePicture != null && model.HivePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "HivePictures");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.HivePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.HivePicture.CopyToAsync(fileStream);
                    }

                    model.HivePicturePath = "/uploads/HivePictures/" + uniqueFileName;
                }

                await beekeeperService.Create(userId!, model);
            }
            catch (Exception ex)
            {
                TempData[ErrorMessage] =
                    "Unexpected error occurred while registering you as a Beekeeper! Please try again later or contact administrator.";

                return RedirectToAction("Index", "Home");
            }


            return RedirectToAction("All", "Honey");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            try
            {
                var beekeepers = await beekeeperService.GetAllBeekeepersAsync();
                
                var viewModel = new AllBeekeepersViewModel
                {
                    Beekeepers = beekeepers
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData[ErrorMessage] = "Възникна грешка при зареждането на пчеларите!";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecommendBeekeeper(RecommendBeekeeperFormModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return to map with errors
                var googleMapsApiKey = HttpContext.RequestServices
                    .GetService<IConfiguration>()?["GoogleMaps:ApiKey"] 
                    ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY")
                    ?? "";

                var allBeekeepers = await beekeeperService.GetAllBeekeepersAsync();
                var mapMarkers = new List<BeekeeperMapMarker>();
                
                foreach (var beekeeper in allBeekeepers)
                {
                    var profileUrl = Url.Action("Profile", "Beekeeper", new { id = beekeeper.Id }) ?? "";
                    if (string.IsNullOrEmpty(profileUrl))
                    {
                        profileUrl = $"/Beekeeper/Profile/{beekeeper.Id}";
                    }

                    var beekeeperHoneys = await honeyService.AllByBeekeeperIdAsync(beekeeper.Id);
                    var honeyTypes = beekeeperHoneys
                        .Select(h => h.Title)
                        .Distinct()
                        .Take(5)
                        .ToList();

                    mapMarkers.Add(new BeekeeperMapMarker
                    {
                        Id = beekeeper.Id,
                        FullName = beekeeper.FullName,
                        Region = beekeeper.Location ?? "България", // Use Location from BeekeeperCardViewModel
                        ShopLocation = beekeeper.Location ?? "България",
                        Latitude = 42.7339, // Default coordinates for Bulgaria center
                        Longitude = 25.4858,
                        HoneyTypes = honeyTypes.Any() ? honeyTypes : new List<string> { "Различни видове мед" },
                        ProfileUrl = profileUrl
                    });
                }

                var viewModel = new BeekeepersMapViewModel
                {
                    Beekeepers = mapMarkers,
                    GoogleMapsApiKey = googleMapsApiKey,
                    RecommendationForm = model
                };

                return View("Map", viewModel);
            }

            try
            {
                // Get admin email from configuration or use default
                var adminEmail = configuration["EmailSettings:AdminEmail"] 
                    ?? configuration["EmailSettings:SmtpUsername"] 
                    ?? "savethebeebulgaria@gmail.com";

                // Log for verification
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Sending email to: {adminEmail}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Beekeeper: {model.BeekeeperName}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Contact: {model.ContactNumber}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Region: {model.Region}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Colonies: {model.NumberOfColonies ?? "Not specified"}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Recommender: {model.RecommenderName} ({model.RecommenderEmail})");

                // Create email body
                var emailSubject = "Нова препоръка за пчелар - Save The Bee Bulgaria";
                var coloniesInfo = !string.IsNullOrEmpty(model.NumberOfColonies) 
                    ? $"<div class='info-box'><div class='info-label'>Брой пчелни семейства:</div><div class='info-value'>{model.NumberOfColonies}</div></div>"
                    : "";
                
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
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #F59F0A; }}
        .info-label {{ font-weight: bold; color: #F59F0A; margin-bottom: 5px; }}
        .info-value {{ margin-bottom: 15px; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Нова препоръка за пчелар</h2>
        </div>
        <div class='content'>
            <div class='info-box'>
                <div class='info-label'>Име на пчелар:</div>
                <div class='info-value'>{model.BeekeeperName}</div>
            </div>
            <div class='info-box'>
                <div class='info-label'>Номер за връзка:</div>
                <div class='info-value'>{model.ContactNumber}</div>
            </div>
            <div class='info-box'>
                <div class='info-label'>Регион:</div>
                <div class='info-value'>{model.Region}</div>
            </div>
            {coloniesInfo}
            <div class='info-box'>
                <div class='info-label'>Защо се препоръчва:</div>
                <div class='info-value'>{model.RecommendationReason}</div>
            </div>
            <div class='info-box'>
                <div class='info-label'>Препоръчан от:</div>
                <div class='info-value'>{model.RecommenderName} ({model.RecommenderEmail})</div>
            </div>
            <div class='footer'>
                <p>Това съобщение е изпратено автоматично от формата за препоръки на Save The Bee Bulgaria.</p>
            </div>
        </div>
    </div>
</body>
</html>";

                // Send email
                await emailSender.SendEmailAsync(adminEmail, emailSubject, emailBody, "");
                
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] ✓ Email sent successfully to {adminEmail}");

                TempData[SuccessMessage] = "Благодарим ви! Вашата препоръка е изпратена успешно. Ще се свържем с вас скоро.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] ✗ Error sending email: {ex.Message}");
                Console.WriteLine($"[BEEKEEPER RECOMMENDATION] Stack trace: {ex.StackTrace}");
                TempData[ErrorMessage] = "Възникна грешка при изпращането на препоръката. Моля, опитайте отново по-късно.";
            }

            return RedirectToAction("Map", new { success = true });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Map(bool? success = false)
        {
            try
            {
                // Show success notification if redirected from successful submission
                if (success == true && TempData.ContainsKey(SuccessMessage))
                {
                    ViewBag.ShowSuccessNotification = true;
                }
                // Get all beekeepers from database (works in both SQLite local and PostgreSQL production)
                var allBeekeepers = await beekeeperService.GetAllBeekeepersAsync();
                
                // Get Google Maps API key from configuration
                var googleMapsApiKey = HttpContext.RequestServices
                    .GetService<IConfiguration>()?["GoogleMaps:ApiKey"] 
                    ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY")
                    ?? "";

                // Hardcoded test data for local development when database is empty
                var hardcodedBeekeepers = new List<BeekeeperMapMarker>
                {
                    new BeekeeperMapMarker
                    {
                        Id = "6d013af5-d0e9-4704-a6a9-877ae0000000", // Placeholder - will be replaced if found in DB
                        FullName = "Ивайло Борисов",
                        Region = "Северозападен, Враца",
                        ShopLocation = "Пазар на Враца, Централен пазар",
                        Latitude = 43.2044,
                        Longitude = 23.5074,
                        HoneyTypes = new List<string> { "Различни видове мед" }
                    }
                };

                // Build map markers from beekeepers
                var mapMarkers = new List<BeekeeperMapMarker>();
                
                if (allBeekeepers.Any())
                {
                    // Use database beekeepers
                    foreach (var beekeeper in allBeekeepers)
                    {
                        string beekeeperId = beekeeper.Id;
                        string profileUrl = string.Empty;
                        
                        // Generate profile URL
                        var beekeeperExists = await beekeeperService.GetBeekeeperProfileByIdAsync(beekeeperId) != null;
                        
                        if (beekeeperExists)
                        {
                            profileUrl = Url.Action("Profile", "Beekeeper", new { id = beekeeperId }) ?? "";
                            if (string.IsNullOrEmpty(profileUrl))
                            {
                                profileUrl = $"/Beekeeper/Profile/{beekeeperId}";
                            }
                        }
                        
                        // Get honey types for this beekeeper
                        var honeyTypes = new List<string>();
                        if (beekeeperExists)
                        {
                            try
                            {
                                var beekeeperHoneys = await honeyService.AllByBeekeeperIdAsync(beekeeperId);
                                honeyTypes = beekeeperHoneys
                                    .Select(h => h.Title)
                                    .Distinct()
                                    .Take(5)
                                    .ToList();
                            }
                            catch
                            {
                                // If honey lookup fails, use default
                                honeyTypes = new List<string> { "Различни видове мед" };
                            }
                        }
                        
                        if (!honeyTypes.Any())
                        {
                            honeyTypes = new List<string> { "Различни видове мед" };
                        }
                        
                        mapMarkers.Add(new BeekeeperMapMarker
                        {
                            Id = beekeeperId,
                            FullName = beekeeper.FullName,
                            Region = beekeeper.Location ?? "България", // Use Location from BeekeeperCardViewModel
                            ShopLocation = beekeeper.Location ?? "България",
                            Latitude = 42.7339, // Default coordinates for Bulgaria center
                            Longitude = 25.4858,
                            HoneyTypes = honeyTypes,
                            ProfileUrl = profileUrl
                        });
                    }
                }
                else
                {
                    // Use hardcoded data for testing
                    foreach (var hardcodedBeekeeper in hardcodedBeekeepers)
                    {
                        string beekeeperId = hardcodedBeekeeper.Id;
                        string profileUrl = string.Empty;
                        
                        // Try to find the beekeeper in database by name
                        var actualBeekeeperId = await beekeeperService.GetBeekeeperIdByFullNameAsync(hardcodedBeekeeper.FullName);
                        if (!string.IsNullOrEmpty(actualBeekeeperId))
                        {
                            beekeeperId = actualBeekeeperId;
                            
                            // Generate profile URL if beekeeper exists
                            var beekeeperExists = await beekeeperService.GetBeekeeperProfileByIdAsync(beekeeperId) != null;
                            if (beekeeperExists)
                            {
                                profileUrl = Url.Action("Profile", "Beekeeper", new { id = beekeeperId }) ?? "";
                                if (string.IsNullOrEmpty(profileUrl))
                                {
                                    profileUrl = $"/Beekeeper/Profile/{beekeeperId}";
                                }
                                
                                // Get honey types from database
                                try
                                {
                                    var beekeeperHoneys = await honeyService.AllByBeekeeperIdAsync(beekeeperId);
                                    hardcodedBeekeeper.HoneyTypes = beekeeperHoneys
                                        .Select(h => h.Title)
                                        .Distinct()
                                        .Take(5)
                                        .ToList();
                                }
                                catch
                                {
                                    // Keep default honey types
                                }
                            }
                        }
                        
                        hardcodedBeekeeper.Id = beekeeperId;
                        hardcodedBeekeeper.ProfileUrl = profileUrl;
                        mapMarkers.Add(hardcodedBeekeeper);
                    }
                }

                var viewModel = new BeekeepersMapViewModel
                {
                    Beekeepers = mapMarkers,
                    GoogleMapsApiKey = googleMapsApiKey,
                    RecommendationForm = new RecommendBeekeeperFormModel()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData[ErrorMessage] = "Възникна грешка при зареждането на картата!";
                return RedirectToAction("All", "Beekeeper");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Profile(string id)
        {
            try
            {
                var beekeeper = await beekeeperService.GetBeekeeperProfileByIdAsync(id);
                if (beekeeper == null)
                {
                    TempData[ErrorMessage] = "Пчеларът не е намерен!";
                    return RedirectToAction("All", "Honey");
                }

                var ownedHoneys = await honeyService.AllByBeekeeperIdAsync(id);
                
                var viewModel = new BeekeeperProfileViewModel
                {
                    Id = beekeeper.Id.ToString(),
                    FullName = beekeeper.User.FirstName + " " + beekeeper.User.LastName,
                    Email = beekeeper.User.Email!,
                    PhoneNumber = beekeeper.PhoneNumber,
                    Story = "Информацията ще бъде добавена скоро", // Default value until database migration
                    Region = "България", // Default value until database migration
                    NumberOfHives = 0, // Default value until database migration
                    ExperienceYears = "Опит", // Default value until database migration
                    Specialties = "Различни видове мед", // Default value until database migration
                    HiveFarmPicturePaths = beekeeper.HiveFarmPicturePaths,
                    OwnedHoneys = ownedHoneys,
                    TotalHoneys = ownedHoneys.Count(),
                    JoinedDate = DateTime.Now.AddMonths(-6) // Default to 6 months ago since ApplicationUser doesn't have CreatedOn property
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData[ErrorMessage] = "Възникна грешка при зареждането на профила!";
                return RedirectToAction("All", "Honey");
            }
        }
    }
}
