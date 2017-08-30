using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
//using Microsoft.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Web;

namespace Thinktecture.Samples.Security
{
    //public class IntegratedWebServiceHostFactory : WebServiceHostFactory
    //{
    //    protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    //    {
    //        var host = base.CreateServiceHost(serviceType, baseAddresses);
            
    //        var binding = new WebHttpBinding(WebHttpSecurityMode.Transport);
    //        binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

    //        host.AddServiceEndpoint(typeof(IRestService), binding, "");

    //        FederatedServiceCredentials.ConfigureServiceHost(host);
    //        return host;
    //    }
    //}
}