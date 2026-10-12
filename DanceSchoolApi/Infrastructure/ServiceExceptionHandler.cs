using System.ComponentModel.DataAnnotations;
using DanceSchoolApi.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DanceSchoolApi.Infrastructure;

public class ServiceExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            EntityNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BusinessRuleException => (StatusCodes.Status409Conflict, exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Данные изменились. Повторите запрос."),
            PostgresException { SqlState: "40001" or "40P01" }
                or DbUpdateException { InnerException: PostgresException { SqlState: "40001" or "40P01" } }
                => (StatusCodes.Status409Conflict, "Данные изменились во время операции. Повторите запрос."),
            DbUpdateException { InnerException: PostgresException { SqlState: "23503" } }
                => (StatusCodes.Status409Conflict, "Операция нарушает связи с другими записями."),
            _ => (0, string.Empty)
        };

        if (status == 0)
            return false;

        await Results.Problem(statusCode: status, title: message, instance: httpContext.Request.Path)
            .ExecuteAsync(httpContext);
        return true;
    }
}
