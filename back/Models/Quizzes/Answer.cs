 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    [Table("Answers")]
    public class Answer
    {
        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string Title { get; set; }
        public bool IsCorrect {  get; set; }
        public string? TextAnswer { get; set; }
        public float? NumericalAnswer { get; set; }
        public float? NumericalAnswerEpsilon { get; set; }
    }
}

