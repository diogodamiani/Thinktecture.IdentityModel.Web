using Thinktecture.IdentityModel.Tokens;

namespace Thinktecture.Samples
{
    public class UserNameSecurityTokenHandler : GenericUserNameSecurityTokenHandler
    {
        protected override bool ValidateUserNameCredentialCore(string userName, string password)
        {
            return (userName == password);
        }
    }
}