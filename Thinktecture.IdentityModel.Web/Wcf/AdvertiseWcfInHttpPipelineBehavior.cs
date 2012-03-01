using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Collections.ObjectModel;

namespace Thinktecture.IdentityModel.Web
{
    public class AdvertiseWcfInHttpPipelineBehavior : Attribute, IServiceBehavior, IDispatchMessageInspector
    {
        private string _label;

        public AdvertiseWcfInHttpPipelineBehavior()
        {
            _label = ServiceAwareWSFederationAuthenticationModule.DefaultLabel;
        }

        public AdvertiseWcfInHttpPipelineBehavior(string label)
        {
            _label = label;
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (var ep in cd.Endpoints)
                {
                    ep.DispatchRuntime.MessageInspectors.Add(new AdvertiseWcfInHttpPipelineBehavior(_label));
                }
            }
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[_label] = true;
            }
        }

        #region not implemented
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        { }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }
        #endregion
    }
}
