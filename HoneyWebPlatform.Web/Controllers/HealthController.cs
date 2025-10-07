using Microsoft.AspNetCore.Mvc;
using HoneyWebPlatform.Services.Data.Interfaces;
using static Common.NotificationMessagesConstants;

namespace HoneyWebPlatform.Web.Controllers
{
    public class HealthController : Controller
    {
        private readonly IDatabaseHealthService databaseHealthService;

        public HealthController(IDatabaseHealthService databaseHealthService)
        {
            this.databaseHealthService = databaseHealthService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DatabaseStatus()
        {
            try
            {
                var status = await databaseHealthService.GetDatabaseStatusAsync();
                var canCreateUser = await databaseHealthService.CanCreateUserAsync();
                var isConnected = await databaseHealthService.IsDatabaseConnectedAsync();

                var healthInfo = new
                {
                    IsConnected = isConnected,
                    CanCreateUser = canCreateUser,
                    Status = status,
                    Timestamp = DateTime.UtcNow
                };

                return Json(healthInfo);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsConnected = false,
                    CanCreateUser = false,
                    Status = $"Error: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var isConnected = await databaseHealthService.IsDatabaseConnectedAsync();
                var canCreateUser = await databaseHealthService.CanCreateUserAsync();
                var status = await databaseHealthService.GetDatabaseStatusAsync();

                if (isConnected && canCreateUser)
                {
                    TempData[SuccessMessage] = "Базата данни е достъпна и готова за работа.";
                }
                else
                {
                    TempData[ErrorMessage] = $"Проблем с базата данни: {status}";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData[ErrorMessage] = $"Грешка при проверка на базата данни: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
