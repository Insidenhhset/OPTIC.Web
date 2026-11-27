using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Optic.Host.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context,Exception exception,CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
            return false;

        int status;
        string title;
        string detail;
        string? field = null;

        if (exception is Identity.Exceptions.ApplicationException appException)
        {
            status = appException.StatusCode;
            title = appException.Title;
            detail = appException.Message;
            field = appException.Field;
        }
        else if (exception is SqlException sqlException
                 && sqlException.Number == 50011)
        {
            status = 409;
            title = "Conflict";
            detail = "Employee Code already exists.";
            field = "EmployeeCode";
        }
        else if (exception is SqlException sqlException2
                 && sqlException2.Number == 50012)
        {
            status = 409;
            title = "Conflict";
            detail = "Username already exists.";
            field = "Username";
        }
        else if (exception is SqlException sqlException3
                 && sqlException3.Number == 50013)
        {
            status = 409;
            title = "Conflict";
            detail = "Email already exists.";
            field = "Email";
        }
        else if (exception is SqlException sqlException4
                 && sqlException4.Number == 50002)
        {
            status = 404;
            title = "Not Found";
            detail = "User was not found.";
        }
        else if (exception is SqlException sqlException5
                 && sqlException5.Number == 50004)
        {
            status = 404;
            title = "Not Found";
            detail = "User was not found or is already deleted.";
        }
        else
        {
            status = 500;
            title = "Internal Server Error";
            detail = "Something went wrong. Please try again later.";

            _logger.LogError(
                exception,
                "Unhandled exception occurred.");
        }

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (field is not null)
            problemDetails.Extensions["field"] = field;

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
