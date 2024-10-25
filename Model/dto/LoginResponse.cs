namespace worklog_api.Model.dto;

public class LoginResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }

    public LoginResponse(string accessToken, string refreshToken)
    {
        access_token = accessToken;
        refresh_token = refreshToken;
    }

    public LoginResponse()
    {
    }
}