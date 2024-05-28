namespace Quizer.Models.Lobbies
{
    public class StatsViewModel
    {
        /// <summary>
        /// string - user display name
        /// int - points
        /// </summary>
        public Dictionary<string, int> UserPoints { get; set; } = [];
    }
}
