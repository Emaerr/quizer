using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    [Table("Questions")]
    public class Question
    {
        public Question()
        {
            Answers = [];
        }

        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public int Position { get; set; }
        public string Title { get; set; }

        public virtual List<Answer> Answers { get; set; }

    }
}
