using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.Web.OAuth2;
using Thinktecture.Samples;

namespace RestDirectOAuth2Client
{
    class Program
    {
        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");
        static Uri _oauth2Address = new Uri("https://" + Constants.IdSrv + "/idsrv/issue/oauth2/");

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

        private static string GetServiceToken()
        {
            "Requesting token".ConsoleYellow();

            var client = new OAuth2Client(_oauth2Address);
            var response = client.RequestAccessTokenUserName("bob", "abc!123", _baseAddress.AbsoluteUri);

            return response.AccessToken;
        }
    }
}
