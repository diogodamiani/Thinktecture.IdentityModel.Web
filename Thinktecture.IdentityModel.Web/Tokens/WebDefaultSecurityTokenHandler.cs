using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Claims;
using System.Web;
using Microsoft.IdentityModel.Tokens;

namespace Thinktecture.IdentityModel.Web
{
    class WebDefaultSecurityTokenHandler : SecurityTokenHandler, IWebSecurityTokenHandler
    {
        public IClaimsPrincipal ValidateWebToken(string token)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }

            return ClaimsPrincipal.CreateFromPrincipal(HttpContext.Current.User);
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
