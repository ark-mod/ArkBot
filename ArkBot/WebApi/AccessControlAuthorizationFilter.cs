using ArkBot.Configuration.Model;
using ArkBot.Helpers;
using ArkBot.WebApi.Controllers;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ArkBot.WebApi
{
    public class AccessControlAuthorizationFilter : IAutofacAuthorizationFilter
    {
        private IConfig _config;

        public AccessControlAuthorizationFilter(IConfig config)
        {
            _config = config;
        }

        private AccessControlAttribute GetControllerAttribute(HttpControllerDescriptor controllerDescriptor)
        {
            var result = controllerDescriptor
                .GetCustomAttributes<AccessControlAttribute>(true)
                .SingleOrDefault();

            return result;
        }

        private AccessControlAttribute GetActionAttribute(HttpActionDescriptor actionDescriptor, out string idParamName)
        {
            idParamName = null;
            var result = actionDescriptor
                .GetCustomAttributes<AccessControlAttribute>(true)
                .SingleOrDefault();

            if (result != null)
            {
                var idParam = actionDescriptor.GetParameters().SingleOrDefault(x => x.GetCustomAttributes<PlayerIdAttribute>().SingleOrDefault() != null);
                idParamName = idParam?.ParameterName;
                return result;
            }

            return null;
        }

        public async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (!SkipAuthorization(actionContext))
            {
                // bind parameter values

                // todo: binding parameter values like this results in empty FormDataCollection
                // [HttpPost]
                // public async Task<HttpResponseMessage> ActionName(FormDataCollection formData)

                await actionContext.ActionDescriptor.ActionBinding.ExecuteBindingAsync(actionContext, cancellationToken);

                // get controller/action access controll attributes
                var idParamName = (string)null;
                var controllerAttribute = GetControllerAttribute(actionContext.ActionDescriptor.ControllerDescriptor);
                var actionAttribute = GetActionAttribute(actionContext.ActionDescriptor, out idParamName);

                // check if authorized / handle unauthorized
                if (controllerAttribute != null || actionAttribute != null)
                {
                    if (controllerAttribute != null && !IsAuthorized(actionContext, controllerAttribute, null))
                    {
                        HandleUnauthorizedRequest(actionContext);
                        return;
                    }

                    if (actionAttribute != null && !IsAuthorized(actionContext, actionAttribute, idParamName))
                    {
                        HandleUnauthorizedRequest(actionContext);
                        return;
                    }
                }
            }
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            if (!actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();

            return true;
        }

        protected virtual bool IsAuthorized(HttpActionContext actionContext, AccessControlAttribute attribute, string idParamName)
        {
            if (actionContext == null) throw ArgumentNull("actionContext");
            if (attribute == null) throw ArgumentNull("attribute");

            var principal = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;
            var user = WebApiHelper.GetUser(principal, _config);

            var idObj = (object)null;
            if (idParamName != null) actionContext.ActionArguments?.TryGetValue(idParamName, out idObj);

            var controller = actionContext.ControllerContext.Controller as BaseApiController;
            if (controller == null) return false;

            var hasAccess = controller.HasFeatureAccess(attribute.FeatureGroup, attribute.FeatureName, idParamName != null ? idObj?.ToString() : user?.SteamId);

            return hasAccess;
        }

        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw ArgumentNull("actionContext");
            }
            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Request Not Authorized");
        }

        internal static ArgumentNullException ArgumentNull(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }
    }
}
