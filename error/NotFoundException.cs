namespace worklog_api.error;

public class NotFoundException : Exception
    
{
    public NotFoundException( string message ) : base( message )
    {
        
    }
}