﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace SkillLearning.Api.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException validationException)
            {
                logger.LogWarning(validationException, "Ocorreu um erro de validação: {Message}", validationException.Message);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var problemDetails = new ValidationProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7807",
                    Title = "Um ou mais erros de validação ocorreram.",
                    Detail = "Por favor, verifique os detalhes dos erros para mais informações."
                };

                foreach (var error in validationException.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.PropertyName))
                    {
                        problemDetails.Errors[error.PropertyName] = new string[] { };
                    }
                    problemDetails.Errors[error.PropertyName] = problemDetails.Errors[error.PropertyName].Append(error.ErrorMessage).ToArray();
                }

                var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                await context.Response.WriteAsync(json);
            }
            catch (ArgumentException argEx)
            {
                logger.LogWarning(argEx, "Ocorreu um erro de argumento/dado inválido: {Message}", argEx.Message);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Requisição inválida.",
                    Detail = argEx.Message
                };

                var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu uma exceção inesperada: {Message}", ex.Message);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Ocorreu um erro inesperado.",
                    Detail = "Por favor, tente novamente mais tarde.",
                    Type = "https://tools.ietf.org/html/rfc7807/problem/internal-server-error"
                };

                var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                await context.Response.WriteAsync(json);
            }
        }
    }
}