using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    public class Question
    {
        [Required]
        public int Id { get; set; }
        [ForeignKey("Quizs")]
        public int QuizId { get; set; }
        public int Position { get; set; }
        public int CorrectAnswer {  get; set; }
        public string? Title { get; set; }

    }
}
