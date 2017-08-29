using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Thinktecture.IdentityModel.Web
{
    internal class HttpErrorHandler : IErrorHandler
    {
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var faultException = error as FaultException;
            if (faultException != null)
            {
                if (faultException.Code.SubCode.Name.Equals(WSSecurity10Constants.FaultCodes.FailedAuthentication) &&
                    faultException.Code.SubCode.Namespace.Equals(WSSecurity10Constants.Namespace))
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                }
            }
        }

        public bool HandleError(Exception error)
        {
            return false;
        }
    }
}
