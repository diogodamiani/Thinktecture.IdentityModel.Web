using System.Security.Claims;

namespace Thinktecture.IdentityModel.Web
{
    interface IWebSecurityTokenHandler
    {
      ClaimsPrincipal ValidateWebToken(string token);
    }
}
