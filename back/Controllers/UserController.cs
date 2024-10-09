using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models.User;

namespace Quizer.Controllers
{
	[Route("[controller]")]
	public class UserController(
		UserManager<ApplicationUser> userManager,
		IUserStore<ApplicationUser> userStore,
		SignInManager<ApplicationUser> signInManager,
		ILogger<UserController> logger,
		IPasswordHasher<ApplicationUser> passwordHasher
		) : Controller
	{


		[Authorize(Policy = "AdminRights")]
		public IActionResult Index()
		{
			IEnumerable<ApplicationUser> users = userManager.Users.ToList();

			return View(users); 
		}

		[HttpGet("Register")]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromForm] UserRegistrationInput userRegistration)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser user = new() { 
					ControlQuestion1 = userRegistration.ControlQuestion1,
                    ControlQuestion2 = userRegistration.ControlQuestion2,
                    ControlQuestion3 = userRegistration.ControlQuestion3,
                };
				await userStore.SetUserNameAsync(user, userRegistration.Username.Trim(), CancellationToken.None);
				var hashingResult = user.SetAnswers(passwordHasher, userRegistration.ControlAnswer1, userRegistration.ControlAnswer2, userRegistration.ControlAnswer3);
				if (hashingResult.IsFailed)
				{
                    logger.LogWarning("Hashing failure when registring a new user: " + hashingResult.Reasons.First());
                }

				var creationResult = await userManager.CreateAsync(user, userRegistration.Password.Trim());
				if (creationResult.Succeeded)
				{
					logger.LogInformation("User created a new account with password.");

					await signInManager.SignInAsync(user, isPersistent: false);
				}

				return RedirectToAction(nameof(Login));
			}

			return StatusCode(500);
		}

		[HttpGet("Login")]
		public async Task<IActionResult> Login(string? returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");
			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			UserLogin userLogin = new()
			{
				ReturnUrl = returnUrl,
			};

			return View(userLogin);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login(UserLogin userLogin)
		{
            userLogin.ReturnUrl ??= Url.Content("~/");

			if (ModelState.IsValid)
			{
				var result = await signInManager.PasswordSignInAsync(userLogin.Input.Username, userLogin.Input.Password, userLogin.Input.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					logger.LogInformation("User logged in.");
					return LocalRedirect(userLogin.ReturnUrl);
				}
				if (result.RequiresTwoFactor)
				{
					logger.LogWarning(message: $"TwoFactor Authentication is required for the user {userLogin.Input.Username}.");
				}
				if (result.IsLockedOut)
				{
					logger.LogWarning("User account locked out.");
					return RedirectToPage("./Lockout");
				}
				else
				{
                    UserLogin errorUserLogin = new()
                    {
                        ReturnUrl = userLogin.ReturnUrl,
						ErrorMessage = "Неверный логин или пароль."
                    };
                    return View(errorUserLogin);
				}
			}

			return View();
		}
	}
}
