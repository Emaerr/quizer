using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    public class Quiz
    {
        [Required]
        public int Id { get; set; }
        public int Author { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [Range(5, 60)]
        public int TimeLimit { get; set; }
    }
}
