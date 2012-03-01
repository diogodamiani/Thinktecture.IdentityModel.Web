using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Thinktecture.IdentityModel.Web
{
    class HttpErrorHandlerBehavior : IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                cd.ErrorHandlers.Add(new HttpErrorHandler());
            }
        }

        #region No-op
        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            // no-op
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // no-op
        }
        #endregion
    }
}
