using Microsoft.EntityFrameworkCore;
using Quizer.Models.Quizzes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Quizer.Models.Lobbies
{
    public class ParticipatorAnswer
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Participator")]
        public string ParticipatorId { get; set; }
        [ForeignKey("Answer")]
        public Answer? TestAnswer { get; set; }
        public float? NumberAnswer { get; set; }
        public string? TextAnswer {  get; set; }
    }
}
