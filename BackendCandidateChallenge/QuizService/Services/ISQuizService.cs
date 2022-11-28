using QuizService.Model;
using System.Collections.Generic;

namespace QuizService.Services
{
    public interface ISQuizService
    {
        /// <summary>
        /// Get Quizzes.
        /// </summary>
        /// <returns></returns>
        IEnumerable<QuizResponseModel> GetQuizzes();

        /// <summary>
        /// Get Quiz By Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        object GetById(int id);
    }
}
