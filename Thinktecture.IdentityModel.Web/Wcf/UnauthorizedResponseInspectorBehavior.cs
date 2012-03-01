using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Thinktecture.IdentityModel.Web
{
    public class UnauthorizedResponseInspectorBehavior : IEndpointBehavior
    {
        IEnumerable<string> _schemes;

        public UnauthorizedResponseInspectorBehavior(IEnumerable<string> schemes)
        {
            _schemes = schemes;
        }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new UnauthorizedResponseInspector(_schemes));
        }

        #region No-op
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        { }

        public void Validate(ServiceEndpoint endpoint)
        { }
        #endregion
    }
}
