using System.IdentityModel.Services;
using System.Web;

namespace Thinktecture.IdentityModel.Web
{
  class ServiceAwareWSFederationAuthenticationModule : WSFederationAuthenticationModule
    {
        public const string DefaultLabel = "IsService";

        protected override void OnAuthorizationFailed(AuthorizationFailedEventArgs e)
        {
            base.OnAuthorizationFailed(e);

            var isWcf = HttpContext.Current.Items[DefaultLabel];

            if (isWcf != null)
            {
                e.RedirectToIdentityProvider = false;
            }
        }
    }
}
