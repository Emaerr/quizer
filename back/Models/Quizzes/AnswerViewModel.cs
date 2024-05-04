using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Quizer.Models.Quizzes
{
    public class AnswerViewModel
    {
        [HiddenInput]
        [Required]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        [StringLength(64, ErrorMessage = "Max title length is 64 symbols")]
        public string Title { get; set; }
        public bool IsCorrect { get; set; }
    }
}
