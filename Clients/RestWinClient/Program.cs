using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using Microsoft.IdentityModel.Claims;
using Thinktecture.IdentityModel.Utility;
using Thinktecture.Samples;

namespace RestWindowsClient
{
    class Program
    {
        //static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/restwin/");
        static Uri _baseAddress = new Uri("https://" + Constants.SelfHost + "/services/restwin/");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                {
                    "Calling Service".ConsoleYellow();

                    var handler = new HttpClientHandler();
                    handler.UseDefaultCredentials = true;
                    var client = new HttpClient(handler) { BaseAddress = _baseAddress };

                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var identity = response.Content.ReadAsAsync<ViewClaims>().Result;
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
    }
}
