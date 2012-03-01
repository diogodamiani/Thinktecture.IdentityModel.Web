using Microsoft.IdentityModel.Claims;

namespace Thinktecture.IdentityModel.Web
{
    interface IWebSecurityTokenHandler
    {
        IClaimsPrincipal ValidateWebToken(string token);
    }
}
