using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading;

namespace Thinktecture.Samples
{
    internal class ServiceLogic
    {
        public ViewClaims GetClaims()
        {
            var id = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            if (id == null)
            {
                throw new SecurityException();
            }

            return new ViewClaims(
                from claim in id.Claims
                select new ViewClaim
                {
                    ClaimType = claim.Type,
                    Value = claim.Value,
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                });
        }

        public string GetInfo()
        {
            return "information for anonymous clients...";
        }
    }
}