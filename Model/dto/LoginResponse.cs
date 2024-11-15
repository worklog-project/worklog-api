using System.Text.Json.Serialization;

namespace worklog_api.Model.dto;

public class LoginResponse
{
    
    [JsonPropertyName("access_token")]
    public string _accessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string _refreshToken { get; set; }

    public LoginResponse(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
    }

    public LoginResponse()
    {
    }
}