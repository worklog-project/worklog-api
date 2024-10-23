using System.Security.Claims;
using worklog_api.Model;


namespace worklog_api.helper
{
    public class JWT
    {
        public static UserModel GetUserInfo(HttpContext ctx)
        {
             
            // Get the JWT claims
            var claimsIdentity = ctx.User.Identity as ClaimsIdentity;

            // Extract specific claim values (e.g., username, role, etc.)
            //var username = claimsIdentity?.FindFirst(ClaimTypes.Sub)?.Value;

            // Or custom claims from the token
            var username = claimsIdentity?.FindFirst("sub")?.Value;
            var role = claimsIdentity?.FindFirst("roles")?.Value;

            var user = new UserModel {
                username = username,
                role = role
            };

            return user;
        }
    }
}
