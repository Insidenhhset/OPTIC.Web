namespace Optic.Identity.Exceptions
{
    public abstract class ApplicationException : Exception
    {
        public int StatusCode { get; }
        public string Title { get; }
        public string? Field { get; }

        protected ApplicationException(
            string message,
            int statusCode,
            string title,
            string? field = null)
            : base(message)
        {
            StatusCode = statusCode;
            Title = title;
            Field = field;
        }
    }

    public class BadRequestException : ApplicationException
    {
        public BadRequestException(string message)
            : base(message, 400, "Bad Request")
        {
        }
    }

    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message)
            : base(message, 404, "Not Found")
        {
        }
    }

    public class ConflictException : ApplicationException
    {
        public ConflictException(string message, string? field = null)
            : base(message, 409, "Conflict", field)
        {
        }
    }

    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException(string message)
            : base(message, 403, "Forbidden")
        {
        }
    }


    public class ServiceUnavailableException : ApplicationException
    {
        public ServiceUnavailableException(string message)
            : base(message, 503, "Service Unavailable")
        {
        }
    }


}
