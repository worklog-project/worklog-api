namespace worklog_api.error;

public class ForbiddenException : Exception
{
    public ForbiddenException(String message) : base(message)
    {
        
    }
    
}