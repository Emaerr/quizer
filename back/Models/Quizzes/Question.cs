using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    public enum QuestionType
    {
        Test,
        TextEntry,
        NumberEntry
    }

    [Table("Questions")]
    public class Question
    {

        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public int Position { get; set; }
        public string Title { get; set; }
        public QuestionType Type { get; set; }
        public string? TextAnswer { get; set; }
        public float? NumericalAnswer { get; set; }
        public float? NumericalAnswerEpsilon { get; set; }

        public virtual List<Answer> TestAnswers { get; set; } = [];

    }
}
