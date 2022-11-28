using QuizService.Repositories.Interfaces;
using System.Collections.Generic;
using QuizService.Model;

namespace QuizService.Services
{
    public class SQuizService : ISQuizService
    {
        private readonly IQuizRepository _quizRepository;

        public SQuizService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public IEnumerable<QuizResponseModel> GetQuizzes()
        {
            return this._quizRepository.GetQuizzes();
        }

        public object GetById(int id)
        {
            return this._quizRepository.GetById(id);
        }
    }
}
