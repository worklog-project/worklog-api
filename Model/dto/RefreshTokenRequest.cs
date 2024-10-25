using System.ComponentModel.DataAnnotations;

namespace worklog_api.Model.dto;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Access Toke is required")]
    public string access_token { get; set; }

    public RefreshTokenRequest(string accessToken)
    {
        access_token = accessToken;
    }
}