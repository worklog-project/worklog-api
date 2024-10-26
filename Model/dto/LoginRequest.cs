using System.ComponentModel.DataAnnotations;

namespace worklog_api.Model.dto;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string username { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string password { get; set; }

    public LoginRequest(string username, string password)
    {
        this.username = username;
        this.password = password;
    }

    public LoginRequest()
    {
    }
}