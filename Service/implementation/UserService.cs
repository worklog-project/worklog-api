using System.IdentityModel.Tokens.Jwt;
using worklog_api.Model.dto;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using worklog_api.error;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Novell.Directory.Ldap;


namespace worklog_api.Service;

public class UserService : IUserService
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;

    private string ldapHost;
    private int ldapPort;
    private string adUsername;
    private string adPassword;
    private string baseDN;
    private string jwtSecret; // At least 32 characters
    private string jwtIssuer;
    private string jwtAudience;


    public UserService(ILogger<UserService> logger, IConfiguration configuration, JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        _logger = logger;
        _configuration = configuration;

        ldapHost = _configuration["LdapSettings:Host"];
        ldapPort = int.Parse(_configuration["LdapSettings:Port"]);
        baseDN = _configuration["LdapSettings:BaseDN"];
        jwtSecret = _configuration["JwtSettings:Secret"];
        jwtIssuer = _configuration["JwtSettings:Issuer"];
        jwtAudience = _configuration["JwtSettings:Audience"];
        adUsername = _configuration["LdapSettings:ADUSER"];
        adPassword = _configuration["LdapSettings:ADPASS"];
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var authenticateUser = AuthenticateUser(loginRequest.username, loginRequest.password);
        var refreshJwtToken = GenerateJwtToken(loginRequest.username, null, "refresh");
        var loginResponse = new LoginResponse();
        loginResponse._accessToken = authenticateUser;
        loginResponse._refreshToken = refreshJwtToken;
        return loginResponse;
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {
        
        var tokenHandler = _jwtSecurityTokenHandler;

        // Check if it's a Bearer token
        if (!refreshTokenRequest.RefreshToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Refresh token refresh token is invalid");
            throw new AuthorizationException("Token must be a Bearer token");
        }

        // Remove 'Bearer ' prefix if present
        var token = refreshTokenRequest.RefreshToken.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        
        try
        {
            // Validate the existing token
            var principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            // Extract user information from the token
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Refresh token refresh token is invalid : id null");
                throw new AuthorizationException("Invalid token");
            }

            string userPrincipalName = $"{adUsername}@kpp.com";
            var userRoles = GetUserRoles(userPrincipalName, adPassword, userId);
            // Generate new tokens

            var generateJwtToken = GenerateJwtToken(userId, userRoles, "access");

            // Return new tokens
            return new LoginResponse(generateJwtToken, token);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new AuthorizationException("Refresh Token has expired");
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            _logger.LogWarning("Refresh token is invalid : invalid issuer");
            throw new AuthorizationException("Invalid token");
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            _logger.LogWarning("Refresh token is invalid : invalid audience");
            throw new AuthorizationException("Invalid token");
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            _logger.LogWarning("Refresh token is invalid : invalid signature key");
            throw new AuthorizationException("Invalid token");
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError($"Refresh Token failed: {ex.Message}");
            throw new AuthorizationException("Refresh Token failed");
        }
    }

    public string AuthenticateUser(string username, string password)
    {
        string userPrincipalName = $"{username}@kpp.com";
        var userRoles = GetUserRoles(userPrincipalName, password, username);
        var generateJwtToken = GenerateJwtToken(username, userRoles, "access");
        
        return generateJwtToken;
    }

    public List<String> GetUserRoles(string username, string password, string usernameLooked)
    {
        try
        {
            List<string> roles = new List<string>();
            using (var conn = new LdapConnection())
            {
                conn.Connect(ldapHost, ldapPort);
                _logger.LogInformation($"Attempting to bind with DN: {username}");
                conn.Bind(username, password);
                _logger.LogInformation($"User {username} authenticated successfully.");
                
                string[] attributes = { "memberOf" };
                var results = conn.Search(
                    baseDN,
                    LdapConnection.SCOPE_SUB,
                    $"(sAMAccountName={usernameLooked})",
                    attributes,
                    false
                );

                if (results.HasMore())
                {
                    var entry = results.Next();
                    var memberOf = entry.getAttribute("memberOf");

                    if (memberOf != null)
                    {
                        foreach (string group in memberOf.StringValueArray)
                        {
                            string role = group.Split(',')[0].Split('=')[1];
                            roles.Add(role);
                        }
                    }
                }
            }
            return roles;
        }
        catch (LdapException e)
        {
            _logger.LogError($"Authentication failed for user {username}. Error: {e.LdapErrorMessage}");
            _logger.LogError($"Error code: {e.ResultCode}");
            _logger.LogError($"Full exception: {e}");
            if (e.ResultCode == 49)
            {
                throw new BadRequestException("Username or password is incorrect.");
            }
            throw new InternalServerError("Internal Server Error");
        }
        catch (Exception e)
        {
            _logger.LogError($"Unexpected error during authentication for user {username}. Error: {e.Message}");
            _logger.LogError($"Full exception: {e}");
            throw new InternalServerError("Internal Server Error");
        }
    }

    
    public string GenerateJwtToken(string username, List<string> roles, string flag)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            } 
        }
        SymmetricSecurityKey key  = null;
        SigningCredentials creds = null;
        JwtSecurityToken token =null;


        if (flag == "access")
        {
            key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
        }else if(flag == "refresh")
        {
            key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(5),
                signingCredentials: creds);
                    
        }
        else
        {
            throw new InternalServerError("errors when generating token");
        }
     
        return _jwtSecurityTokenHandler.WriteToken(token);
    }
}