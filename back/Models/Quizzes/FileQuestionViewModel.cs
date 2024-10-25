using Quizer.Models.File;

namespace Quizer.Models.Quizzes
{
    public class FileQuestionViewModel
    {
        public FileUpload FileUpload { get; set; }
        public QuestionViewModel Question { get; set; }
    }
}
