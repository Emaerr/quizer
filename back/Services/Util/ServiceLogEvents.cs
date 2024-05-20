namespace Quizer.Services.Util
{
    public class ServiceLogEvents
    {
        public const int UserJoinedLobby = 2000;
        public const int UserLeftLobby = 2001;
        public const int LobbyStarted = 2002;
        public const int LobbyStopped = 2003;
        public const int NextQuestionForced = 2004;
        public const int LobbyCreated = 2005;
        public const int AnswerRegistered = 2006;

        public const int UserLobbyJoinError = 2100;
        public const int UserLobbyLeaveError = 2101;
        public const int LobbyStartError = 2102;
        public const int LobbyStopError = 2103;
        public const int NextQuestionForcingError = 2104;
        public const int LobbyCreationError = 2105;
        public const int AnswerRegistrationError = 2106;
    }
}
