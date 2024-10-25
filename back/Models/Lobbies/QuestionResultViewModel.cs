using Quizer.Models.Quizzes;

namespace Quizer.Models.Lobbies
{
    public class QuestionResultViewModel
    {
        public QuestionResultViewModel(QuestionViewModel questionViewModel, ParticipatorAnswer? participatorAnswer) {
            QuestionViewModel = questionViewModel;
            ParticipatorAnswer = participatorAnswer;
        }

        public QuestionViewModel QuestionViewModel { get; set; }
        public ParticipatorAnswer? ParticipatorAnswer { get; set; }
    }
}
