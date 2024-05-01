using Quizer.Models.Quizzes;

namespace Quizer.Logic.Quizzes
{
    /// <summary>
    /// Should be used after the quiz has started.
    /// </summary>
    public class QuizWrapper
    {
        private Quiz _quiz;
        private List<QuestionWrapper> _questions = new List<QuestionWrapper>();

        public QuizWrapper(Quiz quiz, List<QuestionWrapper> questions)
        {
            _quiz = quiz;
            _questions = questions;
        }
    }
}
