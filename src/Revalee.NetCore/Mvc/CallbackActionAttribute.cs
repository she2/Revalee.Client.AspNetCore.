using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Revalee.NetCore.Internal;

namespace Revalee.NetCore.Mvc
{
    /// <summary>
    /// Represents an attribute that will restrict access to previously requested callbacks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CallbackActionAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        IRevaleeRegistrar _revalee;
        ICallbackStateCache _callBackState;

        /// <summary>
        /// Called when a process requests authorization for the marked callback action.
        /// </summary>
        /// <param name="filterContext">The filter context, which encapsulates information for using <see cref="T:Revalee.Client.Mvc.CallbackActionAttribute" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext" /> parameter is null.</exception>
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                Debug.Fail("Revalee Error", $"Revalee::{nameof(AuthorizationFilterContext)} parameter of {nameof(OnAuthorization)} method of {nameof(CallbackActionAttribute)} is null");

                throw new ArgumentNullException(nameof(filterContext));
            }

            _revalee = (IRevaleeRegistrar)filterContext.HttpContext.RequestServices.GetService(typeof(IRevaleeRegistrar));
            _callBackState = (ICallbackStateCache)filterContext.HttpContext.RequestServices.GetService(typeof(ICallbackStateCache));

            if (filterContext.HttpContext == null
                || filterContext.HttpContext.Request == null
                || !_revalee.ValidateCallback(filterContext.HttpContext))// (filterContext.HttpContext.Request))
            {
                filterContext.Result = new UnauthorizedResult();
            }


        }

        /// <summary>
        /// Called before the action executes to supply cached state information if present.
        /// </summary>
        /// <param name="filterContext">The filter context, which encapsulates information for using <see cref="T:Revalee.Client.Mvc.CallbackActionAttribute" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext" /> parameter is null.</exception>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext == null)
            {
                Debug.Fail("Revalee Error", $"Revalee::{nameof(AuthorizationFilterContext)} parameter of {nameof(OnActionExecuting)} method of {nameof(CallbackActionAttribute)} is null");
                throw new ArgumentNullException(nameof(filterContext));
            }

            const string callbackIdFormParameterName = "callbackId";
            const string stateActionParameterName = "state";

            if (filterContext.HttpContext != null
                && filterContext.HttpContext.Request != null
                && filterContext.ActionArguments != null
                && filterContext.ActionDescriptor != null
                )
            {
                string callbackId = filterContext.HttpContext.Request.Form[callbackIdFormParameterName];

                if (!string.IsNullOrEmpty(callbackId))
                {
                    if (filterContext.ActionArguments.ContainsKey(stateActionParameterName))
                    {
                        object cachedState = _callBackState.RecoverCallbackState(callbackId);

                        if (cachedState != null)
                        {
                            foreach (var stateParameter in filterContext.ActionDescriptor.Parameters)
                            {
                                if (string.Equals(stateParameter.Name, stateActionParameterName, StringComparison.OrdinalIgnoreCase)
                                    && stateParameter.ParameterType.IsAssignableFrom(cachedState.GetType()))
                                {
                                    filterContext.ActionArguments[stateActionParameterName] = cachedState;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
