
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
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    
    private string ldapHost ;
    private int ldapPort ;
    private string baseDN;
    private string jwtSecret; // At least 32 characters
    private string jwtIssuer;
    private string jwtAudience;


    public UserService(ILogger<UserService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        ldapHost = _configuration["LdapSettings:Host"];
        ldapPort = int.Parse(_configuration["LdapSettings:Port"]);
        baseDN = _configuration["LdapSettings:BaseDN"];
        jwtSecret = _configuration["JwtSettings:Secret"];
        jwtIssuer = _configuration["JwtSettings:Issuer"];
        jwtAudience = _configuration["JwtSettings:Audience"];
    }
    
    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var authenticateUser = AuthenticateUser(loginRequest.username, loginRequest.password);
        var loginResponse = new LoginResponse();
        loginResponse.access_token = authenticateUser;
        return loginResponse;
    }
    
    public string AuthenticateUser(string username, string password)
    {
        string userDN = $"userPrincipalName={username},CN=Users,{baseDN}";
        string userPrincipalName = $"{username}@kpp.com";

        try
        {
            List<string> roles = new List<string>();
            using (var conn = new LdapConnection())
            {
                conn.Connect(ldapHost, ldapPort);
                _logger.LogInformation($"Attempting to bind with DN: {userDN}");
                conn.Bind(userPrincipalName, password);
                _logger.LogInformation($"User {username} authenticated successfully.");
                
                string[] attributes = { "memberOf" };
                var results = conn.Search(
                    baseDN,
                    LdapConnection.SCOPE_SUB,
                    $"(sAMAccountName={username})",
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
                var generateJwtToken = GenerateJwtToken(username, roles);
                return generateJwtToken;
            }
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

    
    public string GenerateJwtToken(string username, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(5),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}