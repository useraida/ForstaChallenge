﻿using System.Text.Json;

namespace QuizService.ExceptionHandling
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this); 
        }
    }
}
