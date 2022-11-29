using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;

namespace QuizService.ExceptionHandling
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        ErrorDetails errorDetails;
                        if (contextFeature.Error is KeyNotFoundException)
                        {
                            errorDetails = new ErrorDetails()
                            {
                                StatusCode = (int)HttpStatusCode.NotFound,
                                Message = "Entity Not Found"
                            };
                        }
                        else
                        {
                            errorDetails = new ErrorDetails()
                            {
                                StatusCode = (int)HttpStatusCode.InternalServerError,
                                Message = "Internal Server Error"
                            };
                        }
                        context.Response.StatusCode = errorDetails.StatusCode;
                        await context.Response.WriteAsync(errorDetails.ToString());
                    }
                });
            });
        }
    }
}
