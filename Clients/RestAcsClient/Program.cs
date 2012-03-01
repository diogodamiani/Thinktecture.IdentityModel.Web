using System;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.SecurityTokenService;
using Thinktecture.IdentityModel.Utility;
using Thinktecture.IdentityModel.Web;
using Thinktecture.IdentityModel.Web.OAuth2;
using Thinktecture.Samples;

namespace RestAcsClient
{
    class Program
    {
        static EndpointAddress _idpEndpoint =
            new EndpointAddress("https://" + Constants.IdSrv + "/idsrv/issue/wstrust/mixed/certificate");

        static EndpointAddress _acsEndpoint =
            new EndpointAddress("https://" + Constants.ACS + "/v2/wstrust/13/issuedtoken-symmetric");

        static Uri _acsWrapEndpoint = new Uri("https://" + Constants.ACS + "/WRAPv0.9");
        static Uri _acsOAuth2Endpoint = new Uri("https://" + Constants.ACS + "/v2/OAuth2-13");

        static EndpointAddress _acsBaseAddress = new EndpointAddress("https://" + Constants.ACS + "/");

        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                {
                    var samlToken = RequestIdentityToken();
                    var swtToken = RequestServiceTokenWrap(samlToken);

                    "Calling service".ConsoleYellow();
                    var client = new HttpClient { BaseAddress = _baseAddress };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ACS", swtToken);
                    
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
                new CertificateWSTrustBinding(SecurityMode.TransportWithMessageCredential),
                _idpEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;

            factory.Credentials.ClientCertificate.SetCertificate(
                StoreLocation.CurrentUser,
                StoreName.My,
                X509FindType.FindBySubjectDistinguishedName,
                "CN=Client");

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                KeyType = KeyTypes.Bearer,
                AppliesTo = _acsBaseAddress
            };

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;

            return token.TokenXml.OuterXml;
        }

        private static string RequestServiceTokenWrap(string samlToken)
        {
            "Requesting service token".ConsoleYellow();

            var client = new WrapClient(_acsWrapEndpoint);
            return client.IssueAssertion(samlToken, "SAML", _baseAddress).RawToken;
        }

        private static string RequestServiceTokenOAuth2(string samlToken)
        {
            "Requesting service token".ConsoleYellow();

            var client = new OAuth2Client(_acsOAuth2Endpoint);
            return client.RequestAccessTokenAssertion(samlToken, Microsoft.IdentityModel.Tokens.SecurityTokenTypes.Saml2TokenProfile11, _baseAddress.AbsoluteUri).AccessToken;
        }
    }
}
