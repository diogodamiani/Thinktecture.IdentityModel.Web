using System.ServiceModel.Activation;
using Cors;
using Thinktecture.IdentityModel.Web;

namespace Thinktecture.Samples
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SoapService : IService
    {
        public ViewClaims GetClientIdentity()
        {
            return new ServiceLogic().GetClaims();
        }
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RestService : IRestService
    {
        [CorsBehavior]
        public ViewClaims GetClientIdentity()
        {
            return new ServiceLogic().GetClaims();
        }

        [AllowAnonymousAccess]
        public string GetInfo()
        {
            return new ServiceLogic().GetInfo();
        }
    }
}