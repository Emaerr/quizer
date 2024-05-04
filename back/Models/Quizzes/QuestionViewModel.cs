using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Quizer.Models.Quizzes
{
    public class QuestionViewModel
    {
        public QuestionViewModel() {
            Answers = [];
        }

        [HiddenInput]
        [Required]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        [HiddenInput]
        [Required]
        public int Position { get; set; }
        [DisplayName("Title")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public virtual List<AnswerViewModel> Answers { get; set; }
    }
}
