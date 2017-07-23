using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Revalee.NetCore.Settings;
using Revalee.NetCore.Startup;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Recurring
{
    internal class RecurringTaskModuleMiddleware
    {
        readonly RequestDelegate _next;
        readonly IRevaleeClientRecurringSettings _recurringConfig;
        readonly IOptions<RevaleeOptions> _option;
        readonly IRevaleeRegistrar _revalee;

        public RecurringTaskModuleMiddleware(RequestDelegate next, IRevaleeClientRecurringSettings recurringConfig, IRevaleeRegistrar revalee, IOptions<RevaleeOptions> option)
        {
            _next = next;
            _recurringConfig = recurringConfig;
            _revalee = revalee;
            _option = option;
        }

        public Task Invoke(HttpContext context)
        {
            try
            {
                TaskManifest _manifest = LoadedManifest(context).Result;

                if (context?.Request != null && _manifest != null)
                {
                    var request = context.Request;
                    RequestAnalysis analysis = _manifest.AnalyzeRequest(context.Request).Result;

                    if (analysis.IsRecurringTask)
                    {
                        ConfiguredTask taskConfig;
                        HttpStatusCode statusCode;

                        if (_manifest.TryGetTask(analysis.TaskIdentifier, out taskConfig))
                        {
                            if (request.Method == "POST")
                            {
                                if (_revalee.ValidateCallback(context))
                                {
                                    if (taskConfig.SetLastOccurrence(analysis.Occurrence))
                                    {
                                        _manifest.Reschedule(taskConfig).Wait();
                                        context.Items.Add(RouteKeys.IN_PROCESS_CONTEXT_KEY, BuildCallbackDetails(request, taskConfig.Identifier));
                                        context.Request.Path = new PathString(taskConfig.Url.AbsolutePath);

                                        goto Continue;
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Accepted;
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.Unauthorized;
                                }
                            }
                            else
                            {
                                if (request.Method == "GET" || request.Method == "HEAD")
                                {
                                    if (request.Headers["Expect"] == "100-continue")
                                    {
                                        context.Response.StatusCode = (int)HttpStatusCode.Continue;
                                        goto Continue;
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.MethodNotAllowed;
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.NotImplemented;
                                }
                            }
                        }
                        else
                        {
                            statusCode = HttpStatusCode.NoContent;
                        }

                        context.Response.StatusCode = (int)statusCode;
                        return context.Response.WriteAsync("");
                    }
                }
            }
            catch (Exception ex)
            {

            }

            Continue:
            return _next(context);
        }

        async Task<TaskManifest> LoadedManifest(HttpContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_recurringConfig == null)
            {
                throw new ArgumentNullException(nameof(_recurringConfig));
            }

            if (_revalee == null)
            {
                throw new ArgumentNullException(nameof(_revalee));
            }

            if (_option == null)
            {
                throw new ArgumentNullException(nameof(_option));
            }

            if (RecurringTaskModule.Manifest == null)
            {
                await RecurringTaskModule.PrepareManifest(context, _recurringConfig, _revalee, _option);
            }
            return RecurringTaskModule.Manifest;
        }
        private RecurringTaskCallbackDetails BuildCallbackDetails(HttpRequest request, string identifier)
        {
            return new RecurringTaskCallbackDetails(request.Form[CallbackDetailKeys.CALLBACK_ID], request.Form[CallbackDetailKeys.CALLBACK_TIME], request.Form[CallbackDetailKeys.CURRENT_SERVICE_TIME], identifier);
        }
    }
}
