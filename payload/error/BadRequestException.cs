namespace worklog_api.error;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}