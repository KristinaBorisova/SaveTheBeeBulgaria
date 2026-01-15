namespace HoneyWebPlatform.Web.Controllers
{
    using Griesoft.AspNetCore.ReCaptcha;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    using Hubs;
    using Data.Models;
    using ViewModels.User;
    using HoneyWebPlatform.Services.Data.Interfaces;
    using static Common.GeneralApplicationConstants;
    using static Common.NotificationMessagesConstants;
    using Microsoft.AspNetCore.Hosting;

    public class UserController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<UserController> _logger;

        private readonly IMemoryCache memoryCache;

        private readonly ISubscribedEmailService subscribedEmailService;
        private readonly ICartService cartService;
        private readonly IOrderService orderService;
        private readonly IDatabaseHealthService databaseHealthService;
        private readonly ILogger<UserController> logger;

        private readonly IHubContext<CartHub> hubContext;

        public UserController(SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager,
                              IMemoryCache memoryCache,
                              ISubscribedEmailService subscribedEmailService,
                              ICartService cartService,
                              IOrderService orderService,
                              IHubContext<CartHub> hubContext,
                              IWebHostEnvironment webHostEnvironment,
                              IDatabaseHealthService databaseHealthService,
                              ILogger<UserController> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this._logger = logger;

            this.memoryCache = memoryCache;

            this.subscribedEmailService = subscribedEmailService;
            this.cartService = cartService;
            this.orderService = orderService;
            this.databaseHealthService = databaseHealthService;
            this.logger = logger;

            this.hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("GET Register action called with returnUrl: {ReturnUrl}", returnUrl);

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                _logger.LogInformation("External authentication schemes signed out");

                RegisterFormModel model = new RegisterFormModel()
                {
                    ReturnUrl = returnUrl
                };

                _logger.LogInformation("RegisterFormModel created successfully");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET Register action");
                throw;
            }
        }

        [HttpPost]
        [ValidateRecaptcha(Action = nameof(Register),
            ValidationFailedAction = ValidationFailedAction.ContinueRequest)]
        public async Task<IActionResult> Register(RegisterFormModel model)
        {
            logger.LogInformation("Registration attempt started for email: {Email}", model.Email);
            
            try
            {
                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Registration failed - ModelState is invalid for email: {Email}", model.Email);
                    return View(model);
                }

                // Check database health before proceeding
                var canCreateUser = await databaseHealthService.CanCreateUserAsync();
                if (!canCreateUser)
                {
                    var dbStatus = await databaseHealthService.GetDatabaseStatusAsync();
                    logger.LogError("Database health check failed for registration attempt. Email: {Email}, Status: {Status}", model.Email, dbStatus);
                    ModelState.AddModelError(string.Empty, $"Базата данни не е достъпна: {dbStatus}. Моля, опитайте отново по-късно или се свържете с администратор.");
                    return View(model);
                }

                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("Registration failed - User already exists with email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Потребител с този имейл адрес вече съществува.");
                    return View(model);
                }

                ApplicationUser user = new ApplicationUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName
                    // Add more properties as needed
                };

                await userManager.SetEmailAsync(user, model.Email);
                await userManager.SetUserNameAsync(user, model.Email);

                // Profile picture upload removed for now
                // File upload handling code has been removed

                // Attempt to create user with comprehensive error handling
                IdentityResult result;
                try
                {
                    result = await userManager.CreateAsync(user, model.Password);
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
                {
                    ModelState.AddModelError(string.Empty, "Грешка в конфигурацията на системата. Моля, опитайте отново по-късно.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    // In a real application, you would use a proper logging framework
                    ModelState.AddModelError(string.Empty, "Възникна неочаквана грешка при създаване на профила. Моля, опитайте отново.");
                    return View(model);
                }

                if (!result.Succeeded)
                {
                    logger.LogWarning("User creation failed for email: {Email}. Errors: {Errors}", 
                        model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    
                    foreach (IdentityError error in result.Errors)
                    {
                        // Provide more user-friendly error messages
                        var friendlyMessage = GetFriendlyErrorMessage(error.Code);
                        ModelState.AddModelError(string.Empty, friendlyMessage);
                    }

                    return View(model);
                }

                logger.LogInformation("User successfully created for email: {Email}", model.Email);

                // Sign in the user
                try
                {
                    await signInManager.SignInAsync(user, false);
                    this.memoryCache.Remove(UsersCacheKey);

                    logger.LogInformation("User successfully signed in after registration. Email: {Email}", model.Email);
                    TempData[SuccessMessage] = "Успешно създадохте профил! Добре дошли!";
                    return Redirect(model.ReturnUrl ?? "/Home/Index");
                }
                catch (Exception ex)
                {
                    // User was created but sign-in failed
                    logger.LogError(ex, "User was created but sign-in failed for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Профилът беше създаден, но възникна грешка при влизането. Моля, влезте с вашите данни.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                logger.LogError(ex, "Critical error during registration for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Възникна критична грешка. Моля, опитайте отново или се свържете с администратор.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            LoginFormModel model = new LoginFormModel()
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result =
                await signInManager
                    .PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
            {
                TempData[ErrorMessage] =
                    "There was an error while logging you in! Please try again later or contact an administrator.";

                return View(model);
            }

            return Redirect(model.ReturnUrl ?? "/Home/Index");
        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }










        [HttpPost]
        public async Task<IActionResult> SubscribeNewsletter(string email)
        {
            try
            {
                if (IsValidEmail(email))
                {
                    // Check if the email is already a registered user
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        existingUser.IsSubscribed = true;
                        await userManager.UpdateAsync(existingUser);
                    }

                    // Add the email to the SubscribedEmails table
                    await subscribedEmailService.AddSubscribedEmailAsync(email);

                    TempData[SuccessMessage] = "Успешно се записахте за е-вестника!";
                    return Redirect(HttpContext.Request.Headers["Referer"].ToString());

                }
                else
                {
                    TempData[ErrorMessage] = "Неправилен формат.";
                    return Redirect(HttpContext.Request.Headers["Referer"].ToString());
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                TempData[ErrorMessage] = "Грешка при записването за е-вестника.";
                return Redirect(HttpContext.Request.Headers["Referer"].ToString());
            }
        }


        [HttpPost]
        public async Task<IActionResult> UnsubscribeNewsletter(string email)
        {
            try
            {
                if (IsValidEmail(email))
                {
                    // Check if the email is already a registered user
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        existingUser.IsSubscribed = false;
                        await userManager.UpdateAsync(existingUser);
                    }

                    // Remove the email from the SubscribedEmails table
                    await subscribedEmailService.RemoveSubscribedEmailAsync(email);

                    TempData[SuccessMessage] = "Успешно се отписахте от е-вестника!";
                    return Redirect(HttpContext.Request.Headers["Referer"].ToString());
                }
                else
                {
                    TempData[ErrorMessage] = "Неправилен формат.";
                    return Redirect(HttpContext.Request.Headers["Referer"].ToString());
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                TempData[ErrorMessage] = "Грешка при отписването от е-вестника.";
                return Redirect(HttpContext.Request.Headers["Referer"].ToString());
            }
        }



        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            // Get the current user's cart
            string userId = userManager.GetUserId(User);
            CartViewModel cart = await cartService.GetCartAsync(userId);

            if (cart == null)
            {
                // Log or debug the issue
                TempData[ErrorMessage] = "Невалидна поръчка. Количката е празна или невалидна.";
                return RedirectToAction("Cart");
            }

            // Get user information
            var user = await userManager.FindByIdAsync(userId);

            // Pass user information to the view
            var userInformation = new UserViewModel()
            {
                FullName = user.FirstName + " " + user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            // Combine the cart and user information in a single view model
            var cartViewModel = new CartViewModel
            {
                Honeys = cart.Honeys,
                Propolises = cart.Propolises,
                UserInformation = userInformation
            };

            return View(cartViewModel);
        }

        // Add actions for adding/removing items to/from the cart using the CartService
        // ...

        // Example action for adding a product to the cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid honeyId, Guid propolisId, int quantity)
        {
            string userId = userManager.GetUserId(User);
            bool result = await cartService.AddToCartAsync(userId, honeyId, propolisId, quantity);

            if (result)
            {
                TempData[SuccessMessage] = "Продуктът беше успешно добавен в количката!";
            }
            else
            {
                TempData[ErrorMessage] = "Неуспешно добавяне на продукта в количката.";
            }

            return RedirectToAction("Cart");
        }

        // Example action for removing a product from the cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            string userId = userManager.GetUserId(User);
            bool result = await cartService.RemoveFromCartAsync(userId, cartItemId);

            if (result)
            {
                TempData[SuccessMessage] = "Продуктът беше успешно изкаран от количката!!";
            }
            else
            {
                TempData[ErrorMessage] = "Неуспешно изкарване на продукта от количката.";
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            string userId = userManager.GetUserId(User);
            bool result = await cartService.ClearCartAsync(userId);

            if (result)
            {
                TempData[SuccessMessage] = "Количката беше успешно изчистена!";
            }
            else
            {
                TempData[ErrorMessage] = "Неуспешно изчистване на количката.";
            }

            return RedirectToAction("Cart");
        }


        [HttpPost]
        public async Task<IActionResult> PlaceOrder(UserViewModel model)
        {
            string userId = userManager.GetUserId(User);

            CartViewModel cart = await cartService.GetCartAsync(userId);

            if (cart == null || (!cart.Honeys.Any() && !cart.Propolises.Any()))
            {
                // Log or debug the issue
                TempData[ErrorMessage] = "Невалидна поръчка. Количката е празна или съдържа невалидни продукти.";
                return RedirectToAction("Cart");
            }

            // Validate user information
            if (model == null || string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Address))
            {
                TempData[ErrorMessage] = "Моля, попълнете всички задължителни полета.";
                return RedirectToAction("Cart");
            }

            // Set user information
            cart.UserInformation = model;
            cart.UserInformation.Id = Guid.Parse(userId);

            // Debug logging
            Console.WriteLine($"DEBUG: User Information - Name: {cart.UserInformation.FullName}, Email: {cart.UserInformation.Email}, Phone: {cart.UserInformation.PhoneNumber}, Address: {cart.UserInformation.Address}");

            // Validate that user information is properly set
            if (cart.UserInformation == null || string.IsNullOrEmpty(cart.UserInformation.Email))
            {
                TempData[ErrorMessage] = "Грешка при обработка на потребителските данни. Моля, опитайте отново.";
                return RedirectToAction("Cart");
            }

            Guid orderId;
            try
            {
                orderId = await orderService.CreateOrderAsync(cart);
                Console.WriteLine($"DEBUG: Order created successfully with ID: {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error creating order: {ex.Message}");
                TempData[ErrorMessage] = $"Грешка при създаване на поръчка: {ex.Message}";
                return RedirectToAction("Cart");
            }

            if (orderId == Guid.Empty)
            {
                TempData[ErrorMessage] = "Неуспешно създаване на поръчка. Моля, опитайте отново.";
            }
            else
            {
                TempData[SuccessMessage] = "Успешно създадена поръчка с номер " + orderId;

                await cartService.ClearCartAsync(userId);
            }

            return RedirectToAction("Orders");

        }


        public async Task<IActionResult> Orders()
        {
            string userId = userManager.GetUserId(User);

            // Retrieve the user's orders from the database using the OrderService
            List<OrderViewModel> orders = await orderService.GetUserOrdersAsync(userId);

            return View(orders);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(Guid productId, int quantity)
        {
            string userId = userManager.GetUserId(User);

            // Update the quantity in the cart
            var success = await cartService.UpdateCartItemQuantityAsync(userId, productId, quantity);

            if (success)
            {
                // Notify clients using SignalR
                await hubContext.Clients.All.SendAsync("CartItemQuantityUpdated", productId, quantity);

                // Return the updated total price
                var updatedCart = await cartService.GetCartAsync(userId);
                return Json(new { success = true, totalPrice = updatedCart.TotalPrice });
            }

            return Json(new { success = false });
        }





        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GetFriendlyErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                "DuplicateUserName" => "Потребител с това име вече съществува.",
                "DuplicateEmail" => "Потребител с този имейл адрес вече съществува.",
                "InvalidUserName" => "Невалидно потребителско име.",
                "InvalidEmail" => "Невалиден имейл адрес.",
                "InvalidPassword" => "Невалидна парола.",
                "PasswordTooShort" => "Паролата е твърде къса.",
                "PasswordRequiresNonAlphanumeric" => "Паролата трябва да съдържа поне един символ, който не е буква или цифра.",
                "PasswordRequiresDigit" => "Паролата трябва да съдържа поне една цифра.",
                "PasswordRequiresLower" => "Паролата трябва да съдържа поне една малка буква.",
                "PasswordRequiresUpper" => "Паролата трябва да съдържа поне една главна буква.",
                "UserAlreadyHasPassword" => "Потребителят вече има парола.",
                "UserLockoutNotEnabled" => "Заключването на потребители не е активирано.",
                "UserAlreadyInRole" => "Потребителят вече е в тази роля.",
                "UserNotInRole" => "Потребителят не е в тази роля.",
                "InvalidToken" => "Невалиден токен.",
                "RecoveryCodeRedemptionFailed" => "Неуспешно възстановяване на код.",
                "ConcurrencyFailure" => "Грешка при конкурентност. Операцията беше прекъсната.",
                "DefaultIdentityError" => "Възникна грешка при създаване на потребителя.",
                _ => $"Грешка: {errorCode}"
            };
        }

    }
}
