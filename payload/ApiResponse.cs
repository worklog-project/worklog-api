using System.Text.Json;
using System.Text.Json.Serialization;
using worklog_api.helper;

namespace worklog_api.payload;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Data { get; set; }

    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Pagination Pagination { get; set; }
    
    public ApiResponse(int statusCode, string message, T data)
    {
        this.StatusCode = statusCode;
        this.Message = message;
        this.Data = data;
    }

    public ApiResponse(int statusCode, string message, T data, Pagination pagination)
    {
        this.StatusCode = statusCode;
        this.Message = message;
        this.Data = data;
        this.Pagination = pagination;
    }
    
    public ApiResponse()
    {
    }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}