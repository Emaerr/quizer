﻿using System.ComponentModel.DataAnnotations;

namespace Quizer.Models.User
{
	public class UserRegistrationInput
	{
		[Required, StringLength(32), Display(Name = "Имя пользователя")]
		public string Username { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Подтвердить пароль")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required, Display(Name = "Контрольный вопрос 1")]
		public string ControlQuestion1 { get; set; }
		[Required, Display(Name = "Контрольный вопрос 2")]
		public string ControlQuestion2 { get; set; }
		[Required, Display(Name = "Контрольный вопрос 3")]
		public string ControlQuestion3 { get; set; }

		[Required, Display(Name = "Контрольный ответ 1")]
		public string ControlAnswer1 { get; set; }
		[Required, Display(Name = "Контрольный ответ 2")]
		public string ControlAnswer2 { get; set; }
		[Required, Display(Name = "Контрольный ответ 3")]
		public string ControlAnswer3 { get; set; }
	}
}
