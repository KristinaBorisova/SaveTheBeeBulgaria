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

        private readonly IHubContext<CartHub> hubContext;

        public UserController(SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager,
                              IMemoryCache memoryCache,
                              ISubscribedEmailService subscribedEmailService,
                              ICartService cartService,
                              IOrderService orderService,
                              IHubContext<CartHub> hubContext,
                              IWebHostEnvironment webHostEnvironment,
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
            try
            {
                _logger.LogInformation("Registration attempt started for email: {Email}", model.Email);

                // Validate model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Registration failed - ModelState is invalid for email: {Email}. Errors: {Errors}", 
                        model.Email, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return View(model);
                }

                _logger.LogInformation("Model validation passed for email: {Email}", model.Email);

                // Additional input validation and sanitization
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password) || 
                    string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
                {
                    _logger.LogWarning("Registration failed - Required fields are empty for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Всички полета са задължителни.");
                    return View(model);
                }

                // Sanitize inputs
                model.Email = model.Email.Trim().ToLowerInvariant();
                model.FirstName = model.FirstName.Trim();
                model.LastName = model.LastName.Trim();

                _logger.LogInformation("Input sanitization completed for email: {Email}", model.Email);

                // Password strength validation
                if (model.Password.Length < 6)
                {
                    _logger.LogWarning("Registration failed - Password too short for email: {Email}", model.Email);
                    ModelState.AddModelError(nameof(model.Password), "Паролата трябва да е поне 6 символа.");
                    return View(model);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    _logger.LogWarning("Registration failed - Password confirmation mismatch for email: {Email}", model.Email);
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Паролите не съвпадат.");
                    return View(model);
                }

                _logger.LogInformation("Password validation passed for email: {Email}", model.Email);

                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed - User already exists with email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Потребител с този имейл вече съществува.");
                    return View(model);
                }

                _logger.LogInformation("User does not exist, proceeding with creation for email: {Email}", model.Email);

                // Create new user
                ApplicationUser user = new ApplicationUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                _logger.LogInformation("Created ApplicationUser object for: {Email}, FirstName: {FirstName}, LastName: {LastName}", 
                    model.Email, model.FirstName, model.LastName);

                // Set email and username
                await userManager.SetEmailAsync(user, model.Email);
                await userManager.SetUserNameAsync(user, model.Email);

                _logger.LogInformation("Set email and username for user: {Email}", model.Email);

                // Create user with password
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed for email: {Email}. Errors: {Errors}", 
                        model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }

                _logger.LogInformation("User created successfully for email: {Email}, UserId: {UserId}", 
                    model.Email, user.Id);

                // Verify user was created by attempting to retrieve it
                var createdUser = await userManager.FindByEmailAsync(model.Email);
                if (createdUser == null)
                {
                    _logger.LogError("User creation verification failed - User not found after creation for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Грешка при създаване на потребителя. Моля опитайте отново.");
                    return View(model);
                }

                _logger.LogInformation("User creation verified successfully for email: {Email}, UserId: {UserId}", 
                    model.Email, createdUser.Id);

                // Sign in the user
                var signInResult = await signInManager.SignInAsync(user, false);
                if (!signInResult.Succeeded)
                {
                    _logger.LogError("User sign-in failed for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Грешка при влизане в системата. Моля опитайте отново.");
                    return View(model);
                }

                _logger.LogInformation("User signed in successfully for email: {Email}", model.Email);

                // Clear cache
                this.memoryCache.Remove(UsersCacheKey);
                _logger.LogInformation("Cache cleared for users");

                _logger.LogInformation("Registration completed successfully for email: {Email}", model.Email);
                return Redirect(model.ReturnUrl ?? "/Home/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Възникна неочаквана грешка по време на регистрацията. Моля опитайте отново.");
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

    }
}
