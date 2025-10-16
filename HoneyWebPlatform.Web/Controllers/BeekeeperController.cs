namespace HoneyWebPlatform.Web.Controllers
{
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                    Story = beekeeper.Story,
                    Region = beekeeper.Region,
                    NumberOfHives = beekeeper.NumberOfHives,
                    ExperienceYears = beekeeper.ExperienceYears,
                    Specialties = beekeeper.Specialties,
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
