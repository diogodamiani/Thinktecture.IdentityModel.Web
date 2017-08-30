using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SoapWindowsClient
{
    class Program
    {
        static EndpointAddress _serviceEndpoint =
               new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/windows");

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

                        // convert SIDs to NT account names
                        "\nConvert SIDs\n".ConsoleYellow();

                        // this approach does one round trip to the authority (e.g. domain controller) per SID
                        identity.ForEach(c =>
                        {
                            if (c.ClaimType == ClaimTypes.GroupSid)
                            {
                                Console.WriteLine(c.Value);
                                var name = new SecurityIdentifier(c.Value).Translate(typeof(NTAccount));
                                name.Value.ConsoleGreen();

                                Console.WriteLine();
                            }
                        });

                        // this approach collects all SIDs first, and does a single round trip
                        var sids = new IdentityReferenceCollection(16);
                        identity.ForEach(c =>
                        {
                            if (c.ClaimType == ClaimTypes.GroupSid)
                            {
                                sids.Add(new SecurityIdentifier(c.Value));
                            }
                        });

                        "\nGroups:\n".ConsoleYellow();

                        var groups = sids.Translate(typeof(NTAccount));
                        groups.ToList().ForEach(g => Console.WriteLine(g.Value));
                    });

                Console.ReadLine();
            }
        }

        private static IService CreateProxy()
        {
            var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            binding.Security.Message.EstablishSecurityContext = false;

            var factory = new ChannelFactory<IService>(binding, _serviceEndpoint);
            return factory.CreateChannel();
        }
    }
}
