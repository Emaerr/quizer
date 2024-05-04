using Microsoft.AspNetCore.Identity;
using Quizer.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    [Table("Quizzes")]
    public class Quiz
    {
        public Quiz() {
            Questions = [];
        }

        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        [ForeignKey("ApplicationUser")]
        public string AuthorId { get; set; }
        public string? Name { get; set; }
        public int TimeLimit { get; set; }

        public virtual List<Question> Questions { get; set; }
    }
}
