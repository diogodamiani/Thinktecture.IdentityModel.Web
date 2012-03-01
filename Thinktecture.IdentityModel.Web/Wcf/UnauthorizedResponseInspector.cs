using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Thinktecture.IdentityModel.Web
{
    public class UnauthorizedResponseInspector : IDispatchMessageInspector
    {
        IEnumerable<string> _schemes;

        public UnauthorizedResponseInspector(IEnumerable<string> schemes)
        {
            _schemes = schemes;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // no-op
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (WebOperationContext.Current.OutgoingResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                WebTokenServiceAuthorizationManager.SetAuthenticationHeader(_schemes);
            }
        }
    }

    
}
