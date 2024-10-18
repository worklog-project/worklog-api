namespace worklog_api.error;

public class InternalServerError : Exception
{
    public InternalServerError(string message) : base(message)
    {
    }
}