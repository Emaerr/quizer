using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models.File;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using System;
using System.Configuration;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Quizer.Controllers
{

    [Route("[controller]")]
    public class QuestionController : Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        UserManager<ApplicationUser> _userManager;
        IConfiguration _configuration;

        public QuestionController(IServiceScopeFactory scopeFactory, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _userManager = userManager;
            _configuration = configuration;
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

            return CreatedAtAction(nameof(Edit), new {guid = newQuestionGuid});
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

            string? userFilePath = _configuration["UserFileDirPath"];
            if (userFilePath == null)
            {
                throw new ConfigurationErrorsException("UserFileDirPath setting not found");
            }

            string path = Path.Combine(userFilePath, "question_" + questionGuid);

            QuestionViewModel questionViewModel = GetQuestionViewModel(question);
            FileUpload fileUpload = new FileUpload(path, true);

            FileQuestionViewModel fileQuestionViewModel = new FileQuestionViewModel() {
                FileUpload = fileUpload, Question = questionViewModel 
            };

            return View(fileQuestionViewModel);
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
        /// {"Position" : 0, "Title" : "Is it a good cat?", "Image" : "7ac4e327-7e95-49bb-b438-315dcfe4a69c", "Answers" : [{"Title" : "Yep", "IsCorrect" : true}]}}
        /// </code>
        /// </example>
        [HttpPost("Edit/{questionGuid:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string questionGuid, string quizGuid, [FromBody] QuestionViewModel questionViewModel)
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

            try
            {
                if (questionViewModel != null)
                {
                    List<AnswerInfo> answers = new List<AnswerInfo>();
                    foreach (var answerViewModel in questionViewModel.Answers)
                    {
                        answers.Add(new AnswerInfo(answerViewModel.Title, answerViewModel.IsCorrect));
                    }

                    QuestionInfo updatedQuestion = new QuestionInfo(questionViewModel.Position, questionViewModel.Title, question.Info.Type);

                    questionRepository.UpdateUserQuizQuestion(user.Id, quizGuid, questionGuid, updatedQuestion, answers);
                }
                else
                {
                    return BadRequest("Invalid JSON data.");
                }
            }
            catch (Exception exception)
            {
                return BadRequest("An error occurred while processing the request: \r\n" + exception.Message);
            }

            ViewData["quizGuid"] = quizGuid;

            return Ok();
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

            return Ok();
        }

		public async Task<IActionResult> UploadImage(IFormFile file)
		{
			ApplicationUser? user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized();
			}

			string? userFilePath = _configuration["UserFileDirPath"];
			if (userFilePath == null)

			{
				throw new ConfigurationErrorsException("UserFileDirPath setting not found");
			}

			string? extenstion = Path.GetExtension(file.FileName);
			if (extenstion == null)
			{
				throw new ArgumentException();
			}

			string fileGuid = Guid.NewGuid().ToString();
			string fileName = fileGuid + "." + extenstion;
			string filePath = userFilePath + fileName;

			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			string protocolAndDomain = new Uri(HttpContext.Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority);

			string url = protocolAndDomain + "/images/" + fileName;

			return Created(url, fileGuid);
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
