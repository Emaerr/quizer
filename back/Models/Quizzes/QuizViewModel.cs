using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Quizer.Models.Quizzes
{
    public class QuizViewModel
    {
        public QuizViewModel() {
            Questions = [];
        }

        [HiddenInput]
        [Required]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        [DisplayName("Name")]
        [Required(ErrorMessage = "Quiz name is required")]
        public string Name { get; set; }
        [DisplayName("Time limit")]
        [Range(5, 60, ErrorMessage = "Time limit must be between 5 and 60 seconds")]
        public int TimeLimit { get; set; }

        public virtual List<QuestionViewModel> Questions { get; set; }
    }
}
