using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Linq;
using QuizService.Services;

namespace QuizService.Controllers;

//TODO every method in controller should be async
//TODO every method in controller should be implemented with repository, service pattern

[Route("api/quizzes")]
public class QuizController : Controller
{
    private readonly IDbConnection _connection;
    private readonly ISQuizService _quizService; 

    public QuizController(IDbConnection connection, ISQuizService quizService)
    {
        _connection = connection;
        _quizService = quizService;
    }

    // GET api/quizzes
    [HttpGet]
    public IEnumerable<QuizResponseModel> Get()
    {
        return this._quizService.GetQuizzes();
    }

    //TODO I prefer to use a concrete type, QuizResponseModel instead of an object
    // GET api/quizzes/5
    [HttpGet("{id}")]
    public object Get(int id)
    {
        return this._quizService.GetById(id);
    }

    // POST api/quizzes
    //TODO I would rather use CreatedAtAction, this would return created status, add a location header and return created quiz.
    [HttpPost]
    public IActionResult Post([FromBody]QuizCreateModel value)
    {
        var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
        var id = _connection.ExecuteScalar(sql);
        return Created($"/api/quizzes/{id}", null);
    }

    //TODO I would rather check if there is a quiz with this id, and if it does not exist return NotFound()
    // This check should be in the repository.
    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody]QuizUpdateModel value)
    {
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new {Id = id, Title = value.Title});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    //TODO First check if there is a quiz with this id, and if it does not exists return NotFound()
    //This check should be in the repository
    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new {Id = id});
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    //TODO I would rather use CreatedAtAction, this would return created status, add a location header and return created question.
    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody]QuestionCreateModel value)
    {
        const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
        var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new { Id = id });
        if (quiz == null)
            return NotFound();
        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
        var questionId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuizId = id});
        return Created($"/api/quizzes/{id}/questions/{questionId}", null);
    }

    //TODO I would first check if there is a question with this id, and if it does not exist return NotFound()
    // This check should be in the repository.
    //TODO delete id parameter from method definition
    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody]QuestionUpdateModel value)
    {
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new {QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    //TODO First check if there is a question with this id, and if does not exists return NotFound()
    //This check should be in the repository
    //TODO delete parameter int id from method definition
    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        _connection.ExecuteScalar(sql, new {QuestionId = qid});
        return NoContent();
    }

    //TODO First check if there is a question with this id, and if does not exists return NotFound()
    //This check should be in repository
    //I would rather use CreatedAtAction, this would return created status, add a location header and return created answer.
    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody]AnswerCreateModel value)
    {
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        var answerId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuestionId = qid});
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
    }

    //TODO First check if there is an answer with this id, and if does not exist return NotFound()
    // This check should be in repository.
    //TODO delete in and qid parameters from method definition
    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody]AnswerUpdateModel value)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new {AnswerId = qid, Text = value.Text});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    //TODO First check if there is an answer with this id, and if does not exist return NotFound()
    // This check should be in repository.
    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new {AnswerId = aid});
        return NoContent();
    }

    // POST api/quizzes/5/play
    [HttpPost("{id}/play")]
    public int GetNumberOfCorrectAnswers(int id, [FromBody] Dictionary<int, int> answers)
    {
        if (answers.Count == 0)
        {
            return 0;
        }

        QuizResponseModel quiz = (QuizResponseModel)this._quizService.GetById(id);
        int counter = 0;
        for (int i = 0; i < quiz.Questions.Count(); i++)
        {
            var questionId = quiz.Questions.ToList()[i].Id;
            var correctAnswerId = quiz.Questions.ToList()[i].CorrectAnswerId;
            if (answers.ContainsKey(questionId) && answers[questionId] == correctAnswerId)
            {
                counter += 1;
            }
        }
        return counter;
    }
}