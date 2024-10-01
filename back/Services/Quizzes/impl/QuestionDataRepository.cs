
using Microsoft.IdentityModel.Tokens;
using Quizer.Data;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public class QuestionDataRepository : IQuestionDataRepository
    {
        private readonly AppDbContext _context;

        public QuestionDataRepository(AppDbContext context) { 
            _context = context;
        }

        public IEnumerable<QuestionData> GetUserQuizQuestionsData(string userId, string quizGuid)
        {
            var quizzQuery = from q in _context.Quizzes where (q.AuthorId == userId && q.Guid == quizGuid) select q;

            List<QuestionData> result = new List<QuestionData>();
            if (!quizzQuery.IsNullOrEmpty())
            {
                foreach (Question question in quizzQuery.First().Questions)
                {
                    result.Add(GetQuestionDataFromQuestion(question));
                }
            }

            return result;
        }
        public QuestionData? GetUserQuizQuestionData(string userId, string quizGuid, string questionGuid)
        {
            var quizzQuery = from q in _context.Quizzes where (q.AuthorId == userId && q.Guid == quizGuid) select q;

            if (!quizzQuery.IsNullOrEmpty())
            {
                var questionQuerry = from qn in quizzQuery.First().Questions where (qn.Guid == questionGuid) select qn;
                if (!questionQuerry.IsNullOrEmpty()) {
                    return GetQuestionDataFromQuestion(questionQuerry.First());
                }
            }

            return null;
        }

        public string? CreateUserQuizQuestion(string userId, string quizGuid, QuestionType type)
        {
            var quizzQuery = from q in _context.Quizzes where (q.AuthorId == userId && q.Guid == quizGuid) select q;

            if (!quizzQuery.IsNullOrEmpty())
            {
                Question question = new Question() { Type = type };
                quizzQuery.First().Questions.Add(question);
                _context.SaveChanges();
                return question.Guid;
            }

            return null;
        }

        public void UpdateUserQuizQuestion(string userId, string quizGuid, string questionGuid, QuestionInfo info, List<AnswerInfo> answers)
        {
            var quizzQuery = from q in _context.Quizzes where (q.AuthorId == userId && q.Guid == quizGuid) select q;

            if (!quizzQuery.IsNullOrEmpty())
            {
                var questionQuerry = from qn in quizzQuery.First().Questions where (qn.Guid == questionGuid) select qn;
                if (!questionQuerry.IsNullOrEmpty())
                {
                    UpdateQuestionInfo(questionQuerry.First(), info);
                    UpdateQuestionAnswers(questionQuerry.First(), answers);
                }
            }

            _context.SaveChanges();
        }

        public void DeleteUserQuizQuestion(string userId, string quizGuid, string questionGuid)
        {
			var quizzQuery = from q in _context.Quizzes where (q.AuthorId == userId && q.Guid == quizGuid) select q;

			if (!quizzQuery.IsNullOrEmpty())
            { 
				var questionQuerry = from qn in quizzQuery.First().Questions where (qn.Guid == questionGuid) select qn;

				if (!questionQuerry.IsNullOrEmpty())
				{
					var questionsBelow = from qn in quizzQuery.First().Questions where (qn.Position > questionQuerry.First().Position) select qn;
                    foreach (Question question in questionsBelow)
                    {
                        question.Position -= 1;
                        _context.Questions.Update(question);
                    }
                    _context.Questions.Remove(questionQuerry.First());
				}
			}

			_context.SaveChanges();
		}

        private QuestionData GetQuestionDataFromQuestion(Question question)
        {
            List<AnswerData> answers = [];
            foreach (Answer answer in question.Answers) {
                AnswerInfo answerInfo = new AnswerInfo(answer.Title, answer.IsCorrect);
                answers.Add(new AnswerData(answer.Guid, answerInfo));
            }

            QuestionInfo info = new QuestionInfo(question.Position, question.Title, question.Type);

            return new QuestionData(question.Guid, info, answers);
        } 

        private void UpdateQuestionInfo(Question question, QuestionInfo info)
        {
            question.Title = info.Title;
            question.Position = info.Position;
        }

        private void UpdateQuestionAnswers(Question question, List<AnswerInfo> answers)
        {
            question.Answers.Clear();
           
            foreach (AnswerInfo answer in answers)
            {
                question.Answers.Add(new Answer() { Title =  answer.Title, IsCorrect = answer.IsCorrect });
            }
        }
       
    }
}
