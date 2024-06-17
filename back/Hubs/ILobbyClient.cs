namespace Quizer.Hubs
{
    public interface ILobbyClient
    {
        Task RedirectToQuestion();
        Task SendAnswer();
        Task RedirectToBreak();
        Task RedirectToResult();
    }
}
