using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using worklog_api.error;

namespace worklog_api.filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

public class JWTConfiguration
{
    public readonly ILogger<JWTConfiguration> _logger;
    public readonly IConfiguration _configuration;

    public JWTConfiguration(ILogger<JWTConfiguration> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
{
    // Existing service configurations...

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Disable the automatic mapping of claim types.
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                RoleClaimType = "roles",
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]))
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        _logger.LogWarning("Token expired at {ExpiredAt}", context.Exception.Data["exp"]);
                        throw new AuthorizationException("Token expired");
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenInvalidIssuerException))
                    {
                        _logger.LogWarning("Invalid token issuer");
                        throw new AuthorizationException("Invalid token");

                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenInvalidAudienceException))
                    {
                        _logger.LogWarning("Invalid token audience");
                        throw new AuthorizationException("Invalid token");

                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                    {
                        _logger.LogWarning("Invalid signing key");
                        throw new AuthorizationException("Invalid token");
                    }
                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    _logger.LogInformation("Token validated successfully for user: {User}", context.Principal.Identity.Name);
                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        if (context.AuthenticateFailure != null)
                        {
                            _logger.LogWarning("Invalid token. Sending 401 Unauthorized response: {Error}", context.AuthenticateFailure.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Authorization failed. Token was not provided or valid.");
                            throw new AuthorizationException("Authorization failed. Token was not provided or valid.");
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireGroupLeader", policy => 
            policy.RequireAssertion(context => 
            {
                var claims = context.User.Claims.ToList();
                foreach (var claim in claims)
                {
                    Console.WriteLine(claim);
                }
                var hasRequiredClaim = context.User.HasClaim(c => 
                    c.Type == "roles" 
                );
                var hasRolesClaim = context.User.HasClaim(c => 
                    c.Value == "Group Leader"
                );
                if (!hasRequiredClaim)
                {
                    _logger.LogWarning("Authorization failed: Token does not have the required role claim.");
                    throw new AuthorizationException("Authorization failed: Token does not have the required role claim.");
                }
                if (!hasRolesClaim)
                {
                    _logger.LogWarning("Authorization failed: User does not have the required 'Group-Leader' role claim.");
                    throw new ForbiddenException("forbidden for this user");
                }
                _logger.LogInformation("Authorization succeeded for role 'User'.");
                return true;
            })
        );
        options.AddPolicy("RequireMekanik", policy =>
            policy.RequireAssertion(context =>
            {
                 var claims = context.User.Claims.ToList();
                 foreach (var claim in claims)
                 {
                     Console.WriteLine(claim);
                 }
                 var hasRequiredClaim = context.User.HasClaim(c =>
                     c.Type == "roles"
                 );
                 var hasRolesClaim = context.User.HasClaim(c =>
                     c.Value == "Mekanik"
                 );
                 if (!hasRequiredClaim)
                 {
                     _logger.LogWarning("Authorization failed: Token does not have the required role claim.");
                     throw new AuthorizationException("Authorization failed: Token does not have the required role claim.");
                 }
                 if (!hasRolesClaim)
                 {
                     _logger.LogWarning("Authorization failed: User does not have the required 'Group-Leader' role claim.");
                     throw new ForbiddenException("forbidden for this user");
                 }
                 _logger.LogInformation("Authorization succeeded for role 'User'.");
                 return true;
            })
        );

        options.AddPolicy("RequireDataPlanner", policy =>
            policy.RequireAssertion(context =>
            {
                 var claims = context.User.Claims.ToList();
                 foreach (var claim in claims)
                 {
                     Console.WriteLine(claim);
                 }
                 var hasRequiredClaim = context.User.HasClaim(c =>
                     c.Type == "roles"
                 );
                 var hasRolesClaim = context.User.HasClaim(c =>
                     c.Value == "Data Planner"
                 );
                 if (!hasRequiredClaim)
                 {
                     _logger.LogWarning("Authorization failed: Token does not have the required role claim.");
                     throw new AuthorizationException("Authorization failed: Token does not have the required role claim.");
                 }
                 if (!hasRolesClaim)
                 {
                     _logger.LogWarning("Authorization failed: User does not have the required 'Group-Leader' role claim.");
                     throw new ForbiddenException("forbidden for this user");
                 }
                 _logger.LogInformation("Authorization succeeded for role 'User'.");
                 return true;
            })
        );
    });

    _logger.LogInformation("JWT Configuration added successfully");
}


  
}