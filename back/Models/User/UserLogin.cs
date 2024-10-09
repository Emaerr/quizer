using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Quizer.Models.User
{
    public class UserLogin
    {
        public string? ReturnUrl { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }
        public UserInput Input { get; set; } = new();

        public class UserInput
        {
            [Required, StringLength(32), Display(Name = "Имя пользователя")]
            public string Username { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }
            [Display(Name = "Запомнить меня")]
            public bool RememberMe { get; set; }
        }
    }
}
