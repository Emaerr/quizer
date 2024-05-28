using Microsoft.EntityFrameworkCore;
using Quizer.Models.Quizzes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Quizer.Models.Lobbies
{
    [Table("ParticipatorAnswers")]
    public class ParticipatorAnswer
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Participator")]
        public string ParticipatorId { get; set; }
        [Required]
        [ForeignKey("Questions")]
        public virtual Question Question { get; set; }
        public bool IsCorrect { get; set; }
        [ForeignKey("Answer")]
        public virtual Answer? TestAnswer { get; set; }
        public float? NumberAnswer { get; set; }
        public string? TextAnswer {  get; set; }
    }
}
