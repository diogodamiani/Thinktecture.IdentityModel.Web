using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Cors;

namespace Thinktecture.IdentityModel.Web
{
    public class WebTokenWebServiceHost : WebServiceHost
    {
        protected WebTokenWebServiceHostConfiguration _configuration;
        protected WebSecurityTokenHandlerCollectionManager _manager;

        public WebTokenWebServiceHost(Type serviceType, WebSecurityTokenHandlerCollectionManager tokenManager, params Uri[] baseAddresses)
            : this(serviceType, tokenManager, new WebTokenWebServiceHostConfiguration(), baseAddresses)
        { }

        public WebTokenWebServiceHost(Type serviceType, WebSecurityTokenHandlerCollectionManager tokenManager, WebTokenWebServiceHostConfiguration configuration, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            _configuration = configuration;
            _manager = tokenManager;

            Authorization.ServiceAuthorizationManager = new WebTokenServiceAuthorizationManager(tokenManager, configuration);
            Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            
            Description.Behaviors.Add(new OperationAccessServiceBehavior());
            Description.Behaviors.Add(new AdvertiseWcfInHttpPipelineBehavior());
        }

        protected override void OnOpening()
        {
            base.OnOpening();

            Description.Endpoints.ToList().ForEach(ep => ep.Behaviors.Add(new UnauthorizedResponseInspectorBehavior(_manager.RegisteredSchemes)));
            
            // CORS support
            //Description.Endpoints.ToList().ForEach(ep => ep.Behaviors.Add(new CorsBehaviorAttribute()));
            
            Description.Behaviors.Add(new HttpErrorHandlerBehavior());

            // strip out unprotected endpoints
            if (_configuration.RequireSsl)
            {
                var unprotectedEndpoints =
                    Description.Endpoints.Where(ep => ((WebHttpBinding)ep.Binding).Security.Mode == WebHttpSecurityMode.None)
                                         .Select(ep => ep);
                unprotectedEndpoints.ToList().ForEach(ep => Description.Endpoints.Remove(ep));

                if (Description.Endpoints.Count == 0)
                {
                    throw new ServiceActivationException("No SSL secured endpoints found");
                }
            }
        }
    }
}
