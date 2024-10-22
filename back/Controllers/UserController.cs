using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models.User;
using FluentResults;

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
            ViewData["layout"] = "";

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
				var hashingResult = user.SetAnswers(passwordHasher, userRegistration.ControlAnswer1.Trim().ToLower(), userRegistration.ControlAnswer2.Trim().ToLower(), userRegistration.ControlAnswer3.Trim().ToLower());
				if (hashingResult.IsFailed)
				{
                    logger.LogWarning("Hashing failure when registring a new user: " + hashingResult.Reasons.First());
                }

				var creationResult = await userManager.CreateAsync(user, userRegistration.Password.Trim());
				if (creationResult.Succeeded)
				{
					logger.LogInformation("User created a new account with password.");

					await signInManager.SignInAsync(user, isPersistent: false);
                    await userManager.AddToRoleAsync(user, "Member");
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

			ViewData["layout"] = "";

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

		[HttpGet("ForgotPassword")]
		public IActionResult ForgotPassword()
		{
			ViewData["returnUrl"] = Url.Content("~/");
            return View();
		}

		[HttpGet("ResetPassword")]
		public async Task<IActionResult> ResetPassword(string userName, string? returnUrl)
		{
            ApplicationUser? user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                UserLogin errorUserLogin = new()
                {
                    ReturnUrl = returnUrl,
                    ErrorMessage = "Пользователь с таким именем не найден."
                };
                return View(errorUserLogin);
            }

			UserResetPassword userResetPassword = new UserResetPassword()
			{
				Username = userName,
				ReturnUrl = returnUrl,
				ControlQuestion1 = user.ControlQuestion1!,
				ControlQuestion2 = user.ControlQuestion2!,
				ControlQuestion3 = user.ControlQuestion3!
			};

			return View(userResetPassword);
		}

		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(UserResetPassword userResetPassword) 
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

            userResetPassword.ReturnUrl ??= Url.Content("~/");

            ApplicationUser? user = await userManager.FindByNameAsync(userResetPassword.Username);
			if (user == null) {
                UserLogin errorUserLogin = new()
                {
                    ReturnUrl = userResetPassword.ReturnUrl,
                    ErrorMessage = "Пользователь с таким именем не найден."
                };
                return View(errorUserLogin);
            }

			Result<bool> result = user.CheckAnswers(passwordHasher, 
				userResetPassword.Input.ControlAnswer1.Trim().ToLower(), 
				userResetPassword.Input.ControlAnswer2.Trim().ToLower(),
				userResetPassword.Input.ControlAnswer3.Trim().ToLower());

			if (result.IsFailed)
			{
				return StatusCode(500);
			}

			if (result.Value)
			{
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetResult = await userManager.ResetPasswordAsync(user, token, userResetPassword.Input.NewPassword.Trim());
				if (!resetResult.Succeeded)
				{
					return StatusCode(500);
				}
				return Ok();
			}
			else
			{
				UserLogin errorUserLogin = new()
				{
					ReturnUrl = userResetPassword.ReturnUrl,
					ErrorMessage = "Вы ответили неверно, как минимум, на один из трёх вопросов."
				};
				return View(errorUserLogin);
			}
		}
	}
}
