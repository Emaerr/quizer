namespace Quizer.Models.Lobbies
{
    public record UserPointsData(string displayName, int points);

    public class StatsViewModel
    {
        /// <summary>
        /// string - user GUID
        /// int - points
        /// </summary>
        public Dictionary<string, UserPointsData> UserPoints { get; set; } = [];
    }
}
