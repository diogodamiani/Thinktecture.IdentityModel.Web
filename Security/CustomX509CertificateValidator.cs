using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.Samples
{
    public class CustomX509CertificateValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
            // custom validation
        }
    }
}