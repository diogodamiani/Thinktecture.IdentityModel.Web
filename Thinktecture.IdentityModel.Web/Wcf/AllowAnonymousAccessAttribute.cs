using System;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAccessAttribute : Attribute, IOperationBehavior, IOperationInvoker
    {
        private IOperationInvoker _innerOperationInvoker;        
        private object[] _outputs;
        private bool _checkForAuthenticatedAccess;

        public AllowAnonymousAccessAttribute()
        {
            // by default there is no check for access
            _checkForAuthenticatedAccess = false;
        }

        internal AllowAnonymousAccessAttribute(bool checkForAuthenticatedAccess)
        {
            _checkForAuthenticatedAccess = checkForAuthenticatedAccess;
        }

        #region IOperationBehavior Members

        public void Validate(OperationDescription operationDescription)
        {
            // no-op
        }

        /// <summary>
        ///   Implements a modification or extension of the service across an operation.
        /// </summary>
        /// <param name = "operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        /// <param name = "dispatchOperation">The run-time object that exposes customization properties for the operation described by <paramref name = "operationDescription" />.</param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            _innerOperationInvoker = dispatchOperation.Invoker;
            dispatchOperation.Invoker = this;
        }

        /// <summary>
        ///   Implements a modification or extension of the client across an operation.
        /// </summary>
        /// <param name = "operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
        /// <param name = "clientOperation">The run-time object that exposes customization properties for the operation described by <paramref name = "operationDescription" />.</param>
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // no-op
        }

        public void AddBindingParameters(OperationDescription operationDescription,
                                         BindingParameterCollection bindingParameters)
        {
            // no-op
        }

        #endregion

        #region IOperationInvoker Members

        public object[] AllocateInputs()
        {
            return _innerOperationInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            if (_checkForAuthenticatedAccess)
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    return _innerOperationInvoker.Invoke(instance, inputs, out outputs);
                }
                else
                {
                    throw new WebFaultException(HttpStatusCode.Unauthorized);
                }
            }
            else
            {
                return _innerOperationInvoker.Invoke(instance, inputs, out outputs);
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            var f = Task<object>.Factory.StartNew((o) => Invoke(instance, inputs, out _outputs), state);
            if (callback != null) f.ContinueWith((res) => callback(f));

            return f;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            outputs = _outputs;
            return ((Task<object>)result).Result;
        }

        public bool IsSynchronous
        {
            get { return _innerOperationInvoker.IsSynchronous; }
        }

        #endregion
    }
}
