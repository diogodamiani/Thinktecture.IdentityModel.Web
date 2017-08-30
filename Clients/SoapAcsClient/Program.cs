using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.SecurityTokenService;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SoapAcsClient
{
    class Program
    {
        static EndpointAddress _idpEndpoint =
            new EndpointAddress("https://" + Constants.IdSrv + "/idsrv/issue/wstrust/mixed/certificate");

        static EndpointAddress _acsEndpoint =
            new EndpointAddress("https://" + Constants.ACS + "/v2/wstrust/13/issuedtoken-symmetric");

        static EndpointAddress _serviceEndpoint =
            new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/acs");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                    {
                        var proxy = CreateProxy();

                        "Calling service".ConsoleYellow();
                        var identity = proxy.GetClientIdentity();

                        identity.ForEach(c => Helper.ShowClaim(c));
                    });

                Console.ReadLine();
            }
        }

        private static IService CreateProxy()
        {
            // request identity token from IdSrv
            SecurityToken identityToken = RequestIdentityToken();

            // request token for service from ACS
            SecurityToken serviceToken = RequestServiceToken(identityToken);

            // set up factory and channel
            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;

            var factory = new ChannelFactory<IService>(binding, _serviceEndpoint);
            factory.Credentials.SupportInteractive = false;

            // enable WIF on channel factory
            factory.ConfigureChannelFactory();

            return factory.CreateChannelWithIssuedToken(serviceToken);
        }

        private static SecurityToken RequestIdentityToken()
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
                KeyType = KeyTypes.Symmetric,
                AppliesTo = _acsEndpoint
            };

            return factory.CreateChannel().Issue(rst);
        }

        private static SecurityToken RequestServiceToken(SecurityToken identityToken)
        {
            "Requesting service token".ConsoleYellow();

            var binding = new IssuedTokenWSTrustBinding();
            binding.SecurityMode = SecurityMode.TransportWithMessageCredential;

            var factory = new WSTrustChannelFactory(
                binding,
                _acsEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;
            factory.Credentials.SupportInteractive = false;

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                AppliesTo = new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/"),
                KeyType = KeyTypes.Symmetric
            };

            factory.ConfigureChannelFactory();
            var channel = factory.CreateChannelWithIssuedToken(identityToken);
            var token = channel.Issue(rst);

            return token;
        }
    }
}
