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
    public class QuizController : Controller
    {
        private readonly IQuizService _quizService;
        UserManager<ApplicationUser> _userManager;

        public QuizController(IQuizService quizService, UserManager<ApplicationUser> userManager)
        {
            _quizService = quizService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userQuizzes = _quizService.GetUserQuizzes(user);

            return View(userQuizzes);
        }

        public async Task<IActionResult> Create()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Quiz quiz = new Quiz() {
                AuthorId = user.Id,
                Name = "Unnamed",
                TimeLimit = 15
                    
            }; 
           _quizService.Insert(quiz);

            return RedirectToAction(nameof(Edit), new {guid = quiz.Guid});
        }

        public async Task<IActionResult> Edit(string guid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Quiz? quiz = _quizService.GetUserQuiz(user, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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

            Quiz? quiz = _quizService.GetUserQuiz(user, guid);
            if (quiz == null)
            {
                return NotFound();
            }

            quiz.Name = name;
            quiz.TimeLimit = timeLimit;

            _quizService.Update(quiz);

            return View(quiz);
        }

    }
}
