using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    [Table("Questions")]
    public class Question
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        //[ForeignKey("Quiz")]
        //public int? QuizId { get; set; } 
        public int Position { get; set; }
        public string? Title { get; set; }

        public virtual List<Answer>? Answers { get; set; }

    }
}
