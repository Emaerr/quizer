﻿using Quizer.Models.Lobbies;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyRepository
    {
        public IEnumerable<Lobby> GetLobbies();

        public Lobby? GetLobby(int id);

        public void InsertLobby(Lobby lobby);

        public void DeleteLobby(int id);

        public void UpdateLobby(Lobby lobby);

        public void Save();
    }
}
