 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Quizzes
{
    [Table("Answers")]
    public class Answer
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Guid {  get; set; } = System.Guid.NewGuid().ToString();
        //[ForeignKey("Question")]
        //public int? QuestionId { get; set; }
        [StringLength(50)]
        public string? Title { get; set; }
        public bool IsCorrect {  get; set; }
    }
}

