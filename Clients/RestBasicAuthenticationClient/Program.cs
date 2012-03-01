using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Utility;
using Thinktecture.IdentityModel.Web;
using Thinktecture.Samples;

namespace RestBasicAuthenticationClient
{
    class Program
    {
        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");
        //static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/services/rest/");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                    {
                        "Calling Service".ConsoleYellow();

                        var client = new HttpClient { BaseAddress = _baseAddress };
                        client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue("dominick", "dominick");

                        var response = client.GetAsync("identity").Result;
                        response.EnsureSuccessStatusCode();

                        var identity = response.Content.ReadAsAsync<ViewClaims>().Result;
                        identity.ForEach(c => Helper.ShowClaim(c));

                        // access anonymous method with authN
                        client.GetAsync("info").Result.Content.ReadAsStringAsync().Result.ConsoleGreen();
                        response.EnsureSuccessStatusCode();
                    });

                Console.ReadLine();
            }
        }
    }
}
