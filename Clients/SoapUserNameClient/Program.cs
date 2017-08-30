using System;
using System.ServiceModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SoapUserNameClient
{
    class Program
    {
        static EndpointAddress _serviceEndpoint =
            new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/username");

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
            var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            binding.Security.Message.EstablishSecurityContext = false;

            var factory = new ChannelFactory<IService>(binding, _serviceEndpoint);
            factory.Credentials.UserName.UserName = "dominick"; 
            factory.Credentials.UserName.Password = "dominick";

            return factory.CreateChannel();
        }
    }
}
