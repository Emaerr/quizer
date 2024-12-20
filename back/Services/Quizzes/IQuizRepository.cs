﻿using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public interface IQuizRepository
    {
        public IEnumerable<Quiz> GetQuizzes();

        public Quiz? GetQuizById(int id);

        public Quiz? GetQuizByGuid(string guid);

        public void InsertQuiz(Quiz quiz);

        public void DeleteQuiz(int id);

        public void UpdateQuiz(Quiz quiz);

        public IEnumerable<Quiz> GetUserQuizzes(string userId);

        public void Save();

        public Task SaveAsync();
    }
}
