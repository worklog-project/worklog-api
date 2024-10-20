using System.Text.Json;

namespace worklog_api.payload;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}