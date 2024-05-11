namespace Quizer.Services.Quizzes
{
    public interface IQuizControlService
    {
        public void Start(string quizGuid);
        public void ForceNextQuestion(string lobbyGuid);
        public void Stop(string quizGuid);
    }
}
