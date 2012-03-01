using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Claims;

namespace Thinktecture.Samples
{
    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override IClaimsPrincipal Authenticate(string resourceName, IClaimsPrincipal incomingPrincipal)
        {
            return CreateApplicationPrincipal(incomingPrincipal);
        }

        private IClaimsPrincipal CreateApplicationPrincipal(IClaimsPrincipal incomingPrincipal)
        {
            var nameClaim = incomingPrincipal.Identities.First().Claims.First(c => c.ClaimType == ClaimTypes.Name);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, nameClaim.Issuer),
                new Claim(ClaimTypes.Email, nameClaim.Value + "@foo.com"),
                new Claim(ClaimTypes.Role, "Operator")
            };

            return ClaimsPrincipal.CreateFromIdentity(new ClaimsIdentity(claims));
        }
    }
}