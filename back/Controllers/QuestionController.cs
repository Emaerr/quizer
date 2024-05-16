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

        /// <summary>
        /// List of quiz questions.
        /// </summary>
        /// <param name="quizGuid">Quiz GUID</param>
        /// <returns>Index view with a list of QuestionViewModel</returns>
        [HttpGet("Index")]
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

        /// <summary>
        /// Creates quiz question.
        /// </summary>
        /// <param name="quizGuid">Quiz GUID</param>
        /// <returns>Redirects to Question/Edit/{questionGuid}?quizGuid={quizGuid}</returns>
        [HttpPost("Create/{quizGuid:guid}")]
        public async Task<IActionResult> Create(string quizGuid, [FromQuery] string questionTypeStr)
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

            if (!Enum.TryParse(questionTypeStr, out QuestionType questionType))
            {
                return BadRequest();
            }


            List<QuestionData> questions = questionRepository.GetUserQuizQuestionsData(user.Id, quizGuid).ToList();
            string? newQuestionGuid = questionRepository.CreateUserQuizQuestion(user.Id, quizGuid, questionType);

            if (newQuestionGuid == null)
            {
                return StatusCode(418);
            }

            return RedirectToAction("Edit", new { quizGuid = quizGuid, questionGuid = newQuestionGuid });
        }

        /// <summary>
        /// Quiz edit page.
        /// </summary>
        /// <param name="questionGuid">Question GUID</param>
        /// <param name="quizGuid">Quiz GUID</param>
        /// <returns>Edit view with QuestionViewModel</returns>
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

            ViewData["quizGuid"] = quizGuid;

            return View(GetQuestionViewModel(question));
        }

        /// <summary>
        /// Edits the question. Should be called via JavaScript.
        /// </summary>
        /// <param name="questionGuid">Question GUID</param>
        /// <param name="quizGuid">Quiz GUID</param>
        /// <param name="body">JSON string with question data. See more in the example.</param>
        /// <returns>View</returns>
        /// <example>
        /// Example of the JSON string:
        /// <code>
        /// {"Position" : 0, "Title" : "Is it a good cat?", "Answers" : [{"Title" : "Yep", "IsCorrect" : true}]}}
        /// </code>
        /// </example>
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

                QuestionInfo updatedQuestion = new QuestionInfo(questionViewModel.Position, questionViewModel.Title, question.Info.Type);

                questionRepository.UpdateUserQuizQuestion(user.Id, quizGuid, questionGuid, updatedQuestion, answers);
            }

            ViewData["quizGuid"] = quizGuid;

            return View();
        }

        /// <summary>
        /// Deletes the question.
        /// </summary>
        /// <param name="questionGuid">Question GUID</param>
        /// <param name="quizGuid">Quiz GUID</param>
        /// <returns>Redirects to Question/Index?quizGuid={quizGuid} page</returns>
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteConfirm([FromForm] string questionGuid, [FromForm] string quizGuid)
        {
            var scope = _scopeFactory.CreateScope();
            var questionRepository = scope.ServiceProvider.GetService<IQuestionDataRepository>();
            if (questionRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            questionRepository.DeleteUserQuizQuestion(user.Id, quizGuid, questionGuid);

            return RedirectToAction("Index", new { quizGuid });
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
