using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;

namespace Thinktecture.IdentityModel.Web
{
    class WebTokenServiceAuthorizationManager : ServiceAuthorizationManager
    {
        WebSecurityTokenHandlerCollectionManager _manager;
        WebTokenWebServiceHostConfiguration _configuration;

        ServiceConfiguration _serviceConfiguration = IdentityModelConfiguration.ServiceConfiguration;

        public WebTokenServiceAuthorizationManager(WebSecurityTokenHandlerCollectionManager manager, WebTokenWebServiceHostConfiguration configuration)
        {
            _manager = manager;
            _configuration = configuration;
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            var properties = operationContext.ServiceSecurityContext.AuthorizationContext.Properties;
            var to = operationContext.IncomingMessageHeaders.To.AbsoluteUri;

            ClaimsPrincipal principal;
            if (TryGetPrincipal(out principal))
            {
                // set the IClaimsPrincipal
                if (_configuration.ClaimsAuthenticationManager != null)
                {
                    principal = _configuration.ClaimsAuthenticationManager.Authenticate(to, principal);
                }
                else
                {
                    principal = _serviceConfiguration.ClaimsAuthenticationManager.Authenticate(to, principal);
                }

                properties["Principal"] = principal;
            }
            else
            {
                if (_configuration.AllowAnonymousAccess)
                {
                    // set anonymous principal
                    principal = new ClaimsPrincipal(); //ClaimsPrincipal.AnonymousPrincipal;
                    properties["Principal"] = principal;
                }
                else
                {
                    return false;
                }
            }

            if (!_configuration.EnableRequestAuthorization)
            {
                return true;
            }

            return CallClaimsAuthorization(principal, operationContext);
        }

        private bool TryGetPrincipal(out ClaimsPrincipal principal)
        {
            principal = null;

            // check headers - authorization and x-authorization
            var headers = WebOperationContext.Current.IncomingRequest.Headers;
            if (headers != null)
            {
                var authZheader = headers[HttpRequestHeader.Authorization] ?? headers["X-Authorization"];
                if (!string.IsNullOrEmpty(authZheader))
                {
                    int sep = authZheader.IndexOf(' ');
                    if (sep != -1)
                    {
                        var scheme = authZheader.Substring(0, sep);
                        var token = authZheader.Substring(sep + 1);

                        try
                        {
                            principal = _manager.ValidateWebToken(scheme, token);
                        }
                        catch
                        {
                            throw new WebFaultException(HttpStatusCode.Unauthorized);
                        }

                        return (principal != null);
                    }
                    else
                    {
                        throw new SecurityTokenValidationException("Malformed authorization header");
                    }
                }
                else
                {
                    try
                    {
                        principal = _manager.ValidateWebToken("*", string.Empty);
                        return (principal != null);
                    }
                    catch
                    {
                        throw new WebFaultException(HttpStatusCode.Unauthorized);
                    }
                }
            }
            
            return false;
        }

        bool CallClaimsAuthorization(ClaimsPrincipal principal, OperationContext operationContext)
        {
            string action = string.Empty;

            var property = operationContext.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (property != null)
            {
                action = property.Method;
            }

            Uri to = operationContext.IncomingMessageHeaders.To;

            if (to == null || string.IsNullOrEmpty(action))
            {
                return false;
            }

            var context = new AuthorizationContext(principal, to.AbsoluteUri, action);

            if (_configuration.ClaimsAuthorizationManager != null)
            {
                return _configuration.ClaimsAuthorizationManager.CheckAccess(context);
            }

            return _serviceConfiguration.ClaimsAuthorizationManager.CheckAccess(context);
        }

        internal static void SetAuthenticationHeader(IEnumerable<string> schemes)
        {
            var sb = new StringBuilder(16);
            schemes.ToList().ForEach(s => sb.AppendFormat("{0},", s));

            if (sb.Length != 0)
            {
                var headers = WebOperationContext.Current.OutgoingResponse.Headers;
                //headers.Add(HttpResponseHeader.WwwAuthenticate, sb.ToString().TrimEnd(',', ' '));

                headers.Add(HttpResponseHeader.WwwAuthenticate, schemes.ToList().First());
            }
        }
    }
}
