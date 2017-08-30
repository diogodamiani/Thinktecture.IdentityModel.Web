using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SoapCertificateClient
{
    class Program
    {
        static EndpointAddress _serviceEndpoint = 
            new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/certificate");

        static void Main(string[] args)
        {
            while (true)
            {
                "Calling Service".ConsoleYellow();

                Helper.Timer(() =>
                {
                    var proxy = CreateProxy();
                    var identity = proxy.GetClientIdentity();

                    identity.ForEach(c => Helper.ShowClaim(c));
                });

                Console.ReadLine();
            }
        }

        private static IService CreateProxy()
        {
            // set up factory and channel
            var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            binding.Security.Message.EstablishSecurityContext = false;

            // set credentials
            var factory = new ChannelFactory<IService>(binding, _serviceEndpoint);
            factory.Credentials.ClientCertificate.SetCertificate(
                StoreLocation.CurrentUser,
                StoreName.My,
                X509FindType.FindBySubjectDistinguishedName,
                "CN=Client");

            return factory.CreateChannel();
        }
    }
}
