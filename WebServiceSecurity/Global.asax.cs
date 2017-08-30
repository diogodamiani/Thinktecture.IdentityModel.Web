using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Web;

namespace Thinktecture.Samples
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
        }

        private void RegisterRoutes(RouteCollection routes)
        {
            var configuration = new WebTokenWebServiceHostConfiguration
            {
                RequireSsl = true,
                EnableRequestAuthorization = false,
                AllowAnonymousAccess = true
            };
            
            // web tokens
            routes.Add(new ServiceRoute(
                "rest",
                new WebTokenWebServiceHostFactory(SetupSecurityTokenHandler(), configuration),
                typeof(RestService)));

            //routes.Add(new ServiceRoute(
            //    "restwin",
            //    new IntegratedWebServiceHostFactory(),
            //    typeof(RestService)));
        }

        private WebSecurityTokenHandlerCollectionManager SetupSecurityTokenHandler()
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
            adfsConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri("https://" + Constants.WebHost + "/webservicesecurity/rest/"));
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
                "https://" + Constants.WebHost + "/webservicesecurity/rest/",
                "ds9t7JPEsprLRxWvnFjGr+xOhWOy5H8ZHEr5z/rJbi8=");
            #endregion

            #region IdSrv SWT
            manager.AddSimpleWebTokenHandler(
                "IdSrv",
                "http://identity.thinktecture.com/trust",
                "https://" + Constants.WebHost + "/webservicesecurity/rest/",
                "yM7+ti12DiFWcg8t5EfdQbOIgdZCchkETYSXxmvTY0s=");
            #endregion

            #region Allow ASP.NET based authentication
            manager.AddDefaultHandler();
            #endregion

            return manager;
        }
    }
}