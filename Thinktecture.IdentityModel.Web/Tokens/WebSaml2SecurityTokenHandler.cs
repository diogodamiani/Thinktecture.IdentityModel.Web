using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Xml;

namespace Thinktecture.IdentityModel.Web
{
    public class WebSaml2SecurityTokenHandler : Saml2SecurityTokenHandler, IWebSecurityTokenHandler
    {
        public WebSaml2SecurityTokenHandler()
            : base()
        { }

        public WebSaml2SecurityTokenHandler(SecurityTokenHandlerConfiguration configuration)
            : base()
        {
            Configuration = configuration;
        }

        public ClaimsPrincipal ValidateWebToken(string token)
        {
            var securityToken = ReadToken(new XmlTextReader(new StringReader(token)));
            return new ClaimsPrincipal(ValidateToken(securityToken));
        }
    }
}
