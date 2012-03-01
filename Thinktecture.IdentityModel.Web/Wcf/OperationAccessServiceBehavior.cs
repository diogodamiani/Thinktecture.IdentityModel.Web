using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Thinktecture.IdentityModel.Web
{
    public class OperationAccessServiceBehavior : IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var endpoint in serviceDescription.Endpoints)
            {
                foreach (var operation in endpoint.Contract.Operations)
                {
                    if (operation.Behaviors.Find<AllowAnonymousAccessAttribute>() == null)
                    {
                        operation.Behaviors.Add(new AllowAnonymousAccessAttribute(checkForAuthenticatedAccess: true));
                    }
                }
            }
        }

        #region No-Op
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // no-op
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // no-op
        }
        #endregion
    }
}
