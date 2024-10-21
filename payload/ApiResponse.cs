using System.Text.Json;

namespace worklog_api.payload;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public ApiResponse(int statusCode, string message, T data)
    {
        this.StatusCode = statusCode;
        this.Message = message;
        this.Data = data;
    }

    public ApiResponse()
    {
    }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}