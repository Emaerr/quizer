using Quizer.Models.Quizzes;

namespace Quizer.Logic.Quizzes
{
    public class QuestionWrapper
    {
        private Question _question;
        private List<Answer> _answers;
        private Answer _correctAnswer;

        public QuestionWrapper(Question question, List<Answer> answers, Answer correctAnswer)
        {
            _question = question;
            _answers = answers;
            _correctAnswer = correctAnswer;
        }
    }
}
