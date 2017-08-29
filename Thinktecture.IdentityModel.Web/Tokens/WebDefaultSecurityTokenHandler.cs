using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web;

namespace Thinktecture.IdentityModel.Web
{
    class WebDefaultSecurityTokenHandler : SecurityTokenHandler, IWebSecurityTokenHandler
    {
        public ClaimsPrincipal ValidateWebToken(string token)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }

            //return ClaimsPrincipal.CreateFromPrincipal(HttpContext.Current.User);
            return new ClaimsPrincipal(HttpContext.Current.User.Identity);
        }

        public override string[] GetTokenTypeIdentifiers()
        {
            return new string[] { "*" };
        }

        public override Type TokenType
        {
            get { return null; }
        }
    }
}
