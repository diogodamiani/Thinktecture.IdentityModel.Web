using System;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Security;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.SecurityTokenService;
using Thinktecture.Samples;

namespace SoapAdfsClient
{
    class Program
    {
        static EndpointAddress _idpEndpoint =
            new EndpointAddress("https://" + Constants.ADFS + "/adfs/services/trust/13/windowstransport");

        static EndpointAddress _serviceEndpoint =
            new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/adfs");

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
            // request identity token from ADFS
            SecurityToken token = RequestIdentityToken();

            // set up factory and channel
            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;
            
            var factory = new ChannelFactory<IService>(binding, _serviceEndpoint);
            factory.Credentials.SupportInteractive = false;

            // enable WIF on channel factory
            factory.ConfigureChannelFactory();

            return factory.CreateChannelWithIssuedToken(token);
        }

        private static SecurityToken RequestIdentityToken()
        {
            "Requesting identity token".ConsoleYellow();

            var factory = new WSTrustChannelFactory(
                new WindowsWSTrustBinding(SecurityMode.Transport),
                _idpEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                KeyType = KeyTypes.Symmetric,
                AppliesTo = new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/")
            };

            return factory.CreateChannel().Issue(rst);
        }
    }
}
