using System;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.IdentityModel.Tokens;
using System.ServiceModel.Web;
using Thinktecture.IdentityModel.Web;
using Thinktecture.Samples;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var restHost = CreateRestServiceHost();
            restHost.Open();

            var winRestHost = CreateWinRestServiceHost();
            winRestHost.Open();

            Console.WriteLine("REST Service running...");
            restHost.Description.Endpoints.ToList().ForEach(ep => Console.WriteLine(ep.Address));
            Console.WriteLine("WinAuth REST Service running...");
            winRestHost.Description.Endpoints.ToList().ForEach(ep => Console.WriteLine(ep.Address));
            
            Console.ReadLine();

            restHost.Close();
            winRestHost.Close();
        }

        private static WebServiceHost CreateWinRestServiceHost()
        {
            var winSecBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            winSecBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

            var winRestHost = new WebServiceHost(typeof(RestService),
                new Uri("https://" + Constants.SelfHost + "/services/restwin"));
            winRestHost.AddServiceEndpoint(typeof(IRestService), winSecBinding, String.Empty);

            //FederatedServiceCredentials.ConfigureServiceHost(winRestHost);

            return winRestHost;
        }

        private static WebTokenWebServiceHost CreateRestServiceHost()
        {
            var manager = SetupSecurityTokenHandler();
            var configuration = SetupServiceHostConfiguration();

            var restHost = new WebTokenWebServiceHost(typeof(RestService),
                manager,
                configuration,
                new Uri("https://" + Constants.SelfHost + "/services/rest"));

            return restHost;
        }

        private static WebTokenWebServiceHostConfiguration SetupServiceHostConfiguration()
        {
            var configuration = new WebTokenWebServiceHostConfiguration
            {
                RequireSsl = true,
                EnableRequestAuthorization = false,
                AllowAnonymousAccess = true
            };
            return configuration;
        }

        private static WebSecurityTokenHandlerCollectionManager SetupSecurityTokenHandler()
        {
            var manager = new WebSecurityTokenHandlerCollectionManager();

            #region Basic Authentication
            // basic authentication
            manager.AddBasicAuthenticationHandler((username, password) => username == password);

            // sample to use membership provider
            //manager.AddBasicAuthenticationHandler((username, password) => Membership.ValidateUser(username, password));
            #endregion

            #region SAML
            // SAML via ADFS
            var registry = new ConfigurationBasedIssuerNameRegistry();
            registry.AddTrustedIssuer("d1 c5 b1 25 97 d0 36 94 65 1c e2 64 fe 48 06 01 35 f7 bd db", "ADFS");

            var adfsConfig = new SecurityTokenHandlerConfiguration();
            adfsConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri("https://" + Constants.SelfHost + "/webservicesecurity/rest/"));
            adfsConfig.IssuerNameRegistry = registry;
            adfsConfig.CertificateValidator = X509CertificateValidator.None;


            // token decryption (read from config)
            //adfsConfig.ServiceTokenResolver = IdentityModelConfiguration.ServiceConfiguration.CreateAggregateTokenResolver();

            //manager.AddSaml11SecurityTokenHandler("SAML", adfsConfig);
            manager.AddSaml2SecurityTokenHandler("SAML", adfsConfig);

            #endregion

            #region ACS SWT
            manager.AddSimpleWebTokenHandler(
                "ACS",
                "https://" + Constants.ACS + "/",
                "https://" + Constants.SelfHost + "/webservicesecurity/rest/",
                "ds9t7JPEsprLRxWvnFjGr+xOhWOy5H8ZHEr5z/rJbi8=");
            #endregion

            #region IdSrv SWT
            manager.AddSimpleWebTokenHandler(
                "IdSrv",
                "http://identity.thinktecture.com/trust",
                "https://" + Constants.SelfHost + "/webservicesecurity/rest/",
                "yM7+ti12DiFWcg8t5EfdQbOIgdZCchkETYSXxmvTY0s=");
            #endregion

            return manager;
        }
    }
}
