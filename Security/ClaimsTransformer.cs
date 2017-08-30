using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.Samples
{
    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            return CreateApplicationPrincipal(incomingPrincipal);
        }

        private ClaimsPrincipal CreateApplicationPrincipal(ClaimsPrincipal incomingPrincipal)
        {
            var nameClaim = incomingPrincipal.Identities.First().Claims.First(c => c.Type == ClaimTypes.Name);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, nameClaim.Issuer),
                new Claim(ClaimTypes.Email, nameClaim.Value + "@foo.com"),
                new Claim(ClaimTypes.Role, "Operator")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
}