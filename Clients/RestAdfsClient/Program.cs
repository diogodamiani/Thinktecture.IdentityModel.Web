using System;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.SecurityTokenService;
using Thinktecture.IdentityModel.Utility;
using Thinktecture.Samples;

namespace RestAdfsClient
{
    class Program
    {
        static EndpointAddress _idpEndpoint =
            new EndpointAddress("https://adfs.leastprivilege.vm/adfs/services/trust/13/windowstransport");

        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                {
                    var token = RequestIdentityToken();

                    "Calling service".ConsoleYellow();
                    var client = new HttpClient { BaseAddress = _baseAddress };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SAML", token);

                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var identity = response.Content.ReadAsAsync<ViewClaims>().Result;
                    identity.ForEach(c => Helper.ShowClaim(c));
                });

                Console.ReadLine();
            }
        }

        private static string RequestIdentityToken()
        {
            "Requesting identity token".ConsoleYellow();

            var factory = new WSTrustChannelFactory(
                new WindowsWSTrustBinding(SecurityMode.Transport),
                _idpEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                KeyType = KeyTypes.Bearer,
                /* TokenType = Microsoft.IdentityModel.Tokens.SecurityTokenTypes.Saml2TokenProfile11, */
                AppliesTo = new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/rest/")
            };

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;
            return token.TokenXml.OuterXml;
        }
    }
}
