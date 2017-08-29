
using System.Security.Claims;

namespace Thinktecture.IdentityModel.Web
{
    public class WebTokenWebServiceHostConfiguration
    {
        public bool RequireSsl { get; set; }
        public bool EnableRequestAuthorization { get; set; }
        public bool AllowAnonymousAccess { get; set; }
        public ClaimsAuthenticationManager ClaimsAuthenticationManager { get; set; }
        public ClaimsAuthorizationManager ClaimsAuthorizationManager { get; set; }

        public WebTokenWebServiceHostConfiguration()
        {
            RequireSsl = true;
            EnableRequestAuthorization = true;
            AllowAnonymousAccess = false;
        }
    }
}
