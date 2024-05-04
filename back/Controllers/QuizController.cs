using System;
using System.Collections.Generic;
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
        private readonly IQuizService _quizService;
        UserManager<ApplicationUser> _userManager;

        public QuizController(IQuizService quizService, UserManager<ApplicationUser> userManager)
        {
            _quizService = quizService;
            _userManager = userManager;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userQuizzes = _quizService.GetUserQuizzesData(user.Id);

            List<QuizViewModel> viewModels = new List<QuizViewModel>();
            foreach (var quiz in userQuizzes)
            {
                QuizViewModel viewModel = new() {
                    Guid = quiz.Guid,
                    Name = quiz.Name,
                    TimeLimit = quiz.TimeLimit
                };
                viewModels.Add(viewModel);
            }

            return View(viewModels);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            string guid = _quizService.Create(user.Id);

            return RedirectToAction("Edit", new { guid = guid });
        }


        [HttpGet("Edit/{guid:guid}")]
        public async Task<IActionResult> Edit(string guid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = _quizService.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            QuizViewModel viewModel = new() {
                Guid = quiz.Guid,
                Name = quiz.Name,
                TimeLimit = quiz.TimeLimit
            };

            foreach (QuestionData qData in quiz.Questions) {
                QuestionViewModel questionViewModel = new()
                {
                    Guid = qData.Guid,
                    Position = qData.Position,
                    Title = qData.Title,
                };

                foreach (AnswerData aData in qData.Answers) {
                    questionViewModel.Answers.Add(new AnswerViewModel() { Guid = aData.Guid,
                        Title = aData.Title,
                        IsCorrect = aData.isCorrect
                    });
                }

                viewModel.Questions.Add(questionViewModel);
            }

            return View(viewModel);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Edit/{guid:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string guid, [FromForm] string name, [FromForm] int timeLimit)
        {
            if (guid == null)
            {
                return BadRequest();
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = _quizService.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            QuizData quizUpdated = new(quiz.Guid, quiz.AuthorId, name, timeLimit, quiz.Questions);

            _quizService.Update(quizUpdated);

            return View();
        }

        [HttpGet("Details/{guid:guid}")]
        public async Task<IActionResult> Details(string guid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = _quizService.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            QuizViewModel viewModel = new()
            {
                Guid = quiz.Guid,
                Name = quiz.Name,
                TimeLimit = quiz.TimeLimit
            };

            foreach (QuestionData qData in quiz.Questions)
            {
                QuestionViewModel questionViewModel = new()
                {
                    Guid = qData.Guid,
                    Position = qData.Position,
                    Title = qData.Title,
                };

                foreach (AnswerData aData in qData.Answers)
                {
                    questionViewModel.Answers.Add(new AnswerViewModel()
                    {
                        Guid = aData.Guid,
                        Title = aData.Title,
                        IsCorrect = aData.isCorrect
                    });
                }

                viewModel.Questions.Add(questionViewModel);
            }

            return View(viewModel);
        }

        [HttpGet("Delete/{guid:guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            QuizData? quiz = _quizService.GetUserQuizData(user.Id, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            QuizViewModel viewModel = new()
            {
                Guid = quiz.Guid,
                Name = quiz.Name,
                TimeLimit = quiz.TimeLimit
            };

            return View(viewModel);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteConfirm([FromForm] string guid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            _quizService.DeleteUserQuiz(user.Id, guid);

            return RedirectToAction("Index");
        }
    }
}
