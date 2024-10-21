namespace worklog_api.error;

public class AuthorizationException : Exception
{
    public AuthorizationException(String message) : base(message) {
    }
}