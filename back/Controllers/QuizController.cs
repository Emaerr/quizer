using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;

namespace Quizer.Controllers
{
    [Route("[controller]")]
    public class QuizController : Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizController(IServiceScopeFactory scopeFactory, UserManager<ApplicationUser> userManager)
        {
            _scopeFactory = scopeFactory;
            _userManager = userManager;
        }

        /// <summary>
        /// List of user quizzes
        /// </summary>
        /// <returns>Index view with a list of QuizViewModel</returns>
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var scope = _scopeFactory.CreateScope();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userQuizzes = quizRepository.GetUserQuizzesData(user.Id);

            List<QuizViewModel> viewModels = new List<QuizViewModel>();
            foreach (var quiz in userQuizzes)
            {
                QuizViewModel viewModel = new()
                {
                    Guid = quiz.Guid,
                    Name = quiz.Info.Name,
                    TimeLimit = quiz.Info.TimeLimit
                };
                viewModels.Add(viewModel);
            }

            return View(viewModels);
        }

        /// <summary>
        /// Creates quiz.
        /// </summary>
        /// <returns>Redirects to the Quiz/Edit/{GUID} page</returns>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var scope = _scopeFactory.CreateScope();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            string guid = quizRepository.Create(user.Id);

            return RedirectToAction("Edit", new { guid = guid });
        }

        /// <summary>
        /// Quiz edit page. 
        /// </summary>
        /// <param name="guid">Quiz GUID</param>
        /// <returns>Edit view with QuizViewModel</returns>
        [HttpGet("Edit/{guid:guid}")]
        public async Task<IActionResult> Edit(string guid)
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

            QuizData? quiz = quizRepository.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(GetQuizViewModel(quiz, GetQuestionViewModels(user.Id, guid, questionRepository.GetUserQuizQuestionsData(user.Id, guid))));
        }

        /// <summary>
        /// Should be used with forms.
        /// </summary>
        /// <param name="guid">Quiz GUID</param>
        /// <param name="name">New quiz name</param>
        /// <param name="timeLimit">New time limit</param>
        /// <returns>View</returns>
        [HttpPost("Edit/{guid:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string guid, [FromForm] string name, [FromForm] int timeLimit)
        {
            var scope = _scopeFactory.CreateScope();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (quizRepository == null)
            {
                return StatusCode(500);
            }

            if (guid == null)
            {
                return BadRequest();
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = quizRepository.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }
            int timeLimitInMilliseconds = timeLimit * 1000;
            quizRepository.UpdateUserQuizInfo(user.Id, quiz.Guid, new QuizInfo(name, timeLimitInMilliseconds));

            return RedirectToAction("Edit", "Quiz");
        }

        /// <summary>
        /// Quiz details.
        /// </summary>
        /// <param name="guid">Quiz GUID</param>
        /// <returns>View with QuizViewModel</returns>
        [HttpGet("Details/{guid:guid}")]
        public async Task<IActionResult> Details(string guid)
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

            QuizData? quiz = quizRepository.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(GetQuizViewModel(quiz, GetQuestionViewModels(user.Id, guid, questionRepository.GetUserQuizQuestionsData(user.Id, guid))));
        }

        /// <summary>
        /// Sends to the deletion page.
        /// </summary>
        /// <param name="guid">Quiz GUID</param>
        /// <returns>Delete view with QuizViewModel</returns>

        [HttpGet("Delete/{guid:guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            var scope = _scopeFactory.CreateScope();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = quizRepository.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            QuizViewModel viewModel = new()
            {
                Guid = quiz.Guid,
                Name = quiz.Info.Name,
                TimeLimit = quiz.Info.TimeLimit
            };

            return View(GetQuizViewModel(quiz)); ;
        }

        /// <summary>
        /// Deletes the quiz
        /// </summary>
        /// <param name="guid">Quiz GUID</param>
        /// <returns>Redirects to Quiz/Index page</returns>
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteConfirm([FromForm] string guid)
        {
            var scope = _scopeFactory.CreateScope();
            var quizRepository = scope.ServiceProvider.GetService<IQuizDataRepository>();
            if (quizRepository == null)
            {
                return StatusCode(500);
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            quizRepository.DeleteUserQuiz(user.Id, guid);

            return RedirectToAction("Index");
        }

        private QuizViewModel GetQuizViewModel(QuizData quiz, List<QuestionViewModel>? questionData = null)
        {
            QuizViewModel viewModel = new()
            {
                Guid = quiz.Guid,
                Name = quiz.Info.Name,
                TimeLimit = quiz.Info.TimeLimit
            };

            if (questionData != null)
            {
                viewModel.Questions = questionData;
            }

            return viewModel;
        }

        private List<QuestionViewModel> GetQuestionViewModels(string userId, string quizGuid, IEnumerable<QuestionData> questionsData)
        {
            List<QuestionViewModel> result = [];

            foreach (QuestionData q in questionsData)
            {
                QuestionViewModel? qvm = GetQuestionViewModel(userId, quizGuid, q);
                if (qvm != null)
                {
                    result.Add(qvm);
                }
            }

            return result;
        }

        private QuestionViewModel? GetQuestionViewModel(string userId, string quizGuid, QuestionData questionData)
        {
            if (questionData == null)
            {
                return null;
            }

            QuestionViewModel questionViewModel = new()
            {
                Guid = questionData.Guid,
                Position = questionData.Info.Position,
                Title = questionData.Info.Title,
            };

            foreach (AnswerData aData in questionData.Answers)
            {
                questionViewModel.Answers.Add(new AnswerViewModel()
                {
                    Guid = aData.Guid,
                    Title = aData.Info.Title,
                    IsCorrect = aData.Info.IsCorrect,
                });
            }

            return questionViewModel;
        }
    }

}