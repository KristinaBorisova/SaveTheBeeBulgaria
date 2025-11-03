namespace HoneyWebPlatform.Web.Controllers
{
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Infrastructure.Extensions;
using Services.Data.Interfaces;
using ViewModels.Beekeeper;
using ViewModels.Honey;

using static Common.NotificationMessagesConstants;

    [Authorize]
    public class BeekeeperController : Controller
    {
        private readonly IBeekeeperService beekeeperService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHoneyService honeyService;

        public BeekeeperController(IBeekeeperService beekeeperService, IWebHostEnvironment webHostEnvironment, IHoneyService honeyService)
        {
            this.beekeeperService = beekeeperService;
            this.webHostEnvironment = webHostEnvironment;
            this.honeyService = honeyService;
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Map()
        {
            try
            {
                // Using hardcoded test data for demonstration
                // TODO: Replace with database query once migration is applied on Railway
                var beekeepers = new List<BeekeeperCardViewModel>
                {
                    new BeekeeperCardViewModel
                    {
                        Id = "6d013af5-d0e9-4704-a6a9-877ae0000000",
                        FullName = "Ивайло Борисов",
                        Region = "Северозападен, Враца",
                        ShopLocation = "Пазар на Враца, Централен пазар",
                        Latitude = 43.2044,
                        Longitude = 23.5074,
                        HoneyCount = 5
                    }
                    // Commented out other beekeepers for now - only showing Ивайло Борисов
                    /*
                    new BeekeeperCardViewModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Мария Петрова",
                        Region = "Югозападен",
                        ShopLocation = "Централна градинка, София",
                        Latitude = 42.6979,
                        Longitude = 23.3219,
                        HoneyCount = 3
                    },
                    new BeekeeperCardViewModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Иван Димитров",
                        Region = "Южен централен",
                        ShopLocation = "Струма бул., Пловдив",
                        Latitude = 42.1354,
                        Longitude = 24.7453,
                        HoneyCount = 4
                    },
                    new BeekeeperCardViewModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Анна Иванова",
                        Region = "Североизточен",
                        ShopLocation = "Приморски, Варна",
                        Latitude = 43.2141,
                        Longitude = 27.9147,
                        HoneyCount = 6
                    },
                    new BeekeeperCardViewModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "Георги Станев",
                        Region = "Югоизточен",
                        ShopLocation = "Черно море бул., Бургас",
                        Latitude = 42.5048,
                        Longitude = 27.4626,
                        HoneyCount = 8
                    }
                    */
                };
                
                // Mock API key - replace with actual Google Maps API key from configuration
                var googleMapsApiKey = HttpContext.RequestServices
                    .GetService<IConfiguration>()?["GoogleMaps:ApiKey"] 
                    ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY")
                    ?? "";

                var viewModel = new BeekeepersMapViewModel
                {
                    Beekeepers = beekeepers.Select(b => new BeekeeperMapMarker
                    {
                        Id = b.Id,
                        FullName = b.FullName,
                        Region = b.Region,
                        ShopLocation = b.ShopLocation,
                        Latitude = b.Latitude ?? 42.7339, // Default to Bulgaria center if not set
                        Longitude = b.Longitude ?? 25.4858,
                        HoneyTypes = new List<string> { "Липов", "Акациев", "Билков" }, // Test honey types
                        ProfileUrl = Url.Action("Profile", "Beekeeper", new { id = b.Id }) ?? ""
                    }),
                    GoogleMapsApiKey = googleMapsApiKey
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
