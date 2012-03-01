using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Thinktecture.IdentityModel.Web
{
    public class WebTokenWebServiceHostFactory : WebServiceHostFactory
    {
        WebSecurityTokenHandlerCollectionManager _manager;
        WebTokenWebServiceHostConfiguration _configuration;

        public WebTokenWebServiceHostFactory(WebSecurityTokenHandlerCollectionManager manager)
            : this(manager, new WebTokenWebServiceHostConfiguration())
        { }

        public WebTokenWebServiceHostFactory(WebSecurityTokenHandlerCollectionManager manager, WebTokenWebServiceHostConfiguration configuration)
        {
            _configuration = configuration;
            _manager = manager;    
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var host = new WebTokenWebServiceHost(serviceType, _manager, _configuration, baseAddresses);
            return host;
        }
    }
}
