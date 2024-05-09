using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Quizer.Controllers
{

    [Route("[controller]")]
    public class QuestionController : Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        UserManager<ApplicationUser> _userManager;

        public QuestionController(IServiceScopeFactory scopeFactory, UserManager<ApplicationUser> userManager)
        {
            _scopeFactory = scopeFactory;
            _userManager = userManager;
        }

        [HttpGet("Index/{quizGuid:guid}")]
        public async Task<IActionResult> Index(string quizGuid)
        {
            var scope = _scopeFactory.CreateScope();
            var questionRepository = scope.ServiceProvider.GetService<IQuestionDataRepository>();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (questionRepository == null || quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            IEnumerable<QuestionData> questions = questionRepository.GetUserQuizQuestionsData(user.Id, quizGuid);

            List<QuestionViewModel> questionViewModels = [];
            foreach (QuestionData questionData in questions)
            {
                questionViewModels.Add(GetQuestionViewModel(questionData));
            }

            return View(questionViewModels);
        }

        [HttpGet("Create/{quizGuid:guid}")]
        public async Task<IActionResult> Create(string quizGuid)
        {
            var scope = _scopeFactory.CreateScope();
            var questionRepository = scope.ServiceProvider.GetService<IQuestionDataRepository>();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (questionRepository == null || quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            List<QuestionData> questions = questionRepository.GetUserQuizQuestionsData(user.Id, quizGuid).ToList();
            string? newQuestionGuid = questionRepository.CreateUserQuizQuestion(user.Id, quizGuid);

            if (newQuestionGuid == null)
            {
                return StatusCode(418);
            }

            return RedirectToAction("Edit", new { quizGuid = quizGuid, questionGuid = newQuestionGuid });
        }

        [HttpGet("Edit/{questionGuid:guid}")]
        public async Task<IActionResult> Edit(string questionGuid, string quizGuid)
        {
            var scope = _scopeFactory.CreateScope();
            var questionRepository = scope.ServiceProvider.GetService<IQuestionDataRepository>();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (questionRepository == null || quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuestionData? question = questionRepository.GetUserQuizQuestionData(user.Id, quizGuid, questionGuid);
            if (question == null)
            {
                return NotFound();
            }

            return View(GetQuestionViewModel(question));
        }

        [HttpPost("Edit/{questionGuid:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string questionGuid, string quizGuid, [FromBody] string body)
        {
            var scope = _scopeFactory.CreateScope();
            var questionRepository = scope.ServiceProvider.GetService<IQuestionDataRepository>();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (questionRepository == null || quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuestionData? question = questionRepository.GetUserQuizQuestionData(user.Id, quizGuid, questionGuid);
            if (question == null)
            {
                return NotFound();
            }

            QuestionViewModel? questionViewModel = JsonSerializer.Deserialize<QuestionViewModel>(body);
            if (questionViewModel != null)
            {
                List<AnswerInfo> answers = [];
                foreach (AnswerViewModel answerViewModel in questionViewModel.Answers)
                {
                    answers.Add(new AnswerInfo(answerViewModel.Title, answerViewModel.IsCorrect));
                }

                QuestionInfo updatedQuestion = new QuestionInfo(questionViewModel.Position, questionViewModel.Title);

                questionRepository.UpdateUserQuizQuestion(user.Id, quizGuid, questionGuid, updatedQuestion, answers);
            }

            return View(GetQuestionViewModel(question));
        }

        private QuestionViewModel GetQuestionViewModel(QuestionData qData)
        {
            QuestionViewModel questionViewModel = new()
            {
                Guid = qData.Guid,
                Position = qData.Info.Position,
                Title = qData.Info.Title,
            };

            foreach (AnswerData aData in qData.Answers)
            {
                questionViewModel.Answers.Add(new AnswerViewModel()
                {
                    Guid = aData.Guid,
                    Title = aData.Info.Title,
                    IsCorrect = aData.Info.IsCorrect
                });
            }

            return questionViewModel;
        }
    }
}
