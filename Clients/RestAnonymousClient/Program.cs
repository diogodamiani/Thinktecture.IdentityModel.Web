using System;
using System.Net;
using System.Net.Http;
using Thinktecture.Samples;

namespace RestAnonymousClient
{
    class Program
    {
        static Uri _baseAddress = new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/");

        static void Main(string[] args)
        {
            var client = new HttpClient { BaseAddress = _baseAddress };

            var response = client.GetAsync("info").Result;
            response.EnsureSuccessStatusCode();

            var info = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(info);

            Console.WriteLine("trying to access protected resource.");
            response = client.GetAsync("identity").Result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized (401)");
                Console.WriteLine(response.Headers.WwwAuthenticate);
            }
            else
            {
                response.EnsureSuccessStatusCode();
                Console.WriteLine("success");
            }
        }
    }
}
