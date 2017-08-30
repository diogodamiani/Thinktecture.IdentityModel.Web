using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.Web;
using Thinktecture.Samples;

namespace RestDirectWrapClient
{
    class Program
    {
        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");
        static Uri _wrapAddress = new Uri("https://" + Constants.IdSrv + "/idsrv/issue/wrap");

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                    {
                        var swt = GetServiceToken();
                        CallService(swt);
                        
                    });

                Console.ReadLine();
            }
        }

        private static string GetServiceToken()
        {
            "Requesting token".ConsoleYellow();

            var wrapClient = new WrapClient(_wrapAddress);
            return wrapClient.Issue("bob", "abc!123", _baseAddress).RawToken;
        }

        private static void CallService(string swt)
        {
            "Calling service".ConsoleYellow();

            var client = new HttpClient { BaseAddress = _baseAddress };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IdSrv", swt);

            var response = client.GetAsync("identity").Result;
            response.EnsureSuccessStatusCode();

            var identity = response.Content.ReadAsAsync<ViewClaims>().Result;
            identity.ForEach(c => Helper.ShowClaim(c));

        }
    }
}
