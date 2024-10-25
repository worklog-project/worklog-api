using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace worklog_api.Model.dto;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Access Token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    // Constructor parameter name must match the property name 'RefreshToken'
    public RefreshTokenRequest(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    // You might also want to add a parameterless constructor
    public RefreshTokenRequest()
    {
    }
}

