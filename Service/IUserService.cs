using worklog_api.Model.dto;

namespace worklog_api.Service;

public interface IUserService
{
    Task<LoginResponse> Login(LoginRequest loginRequest);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
}