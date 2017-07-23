using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Revalee.NetCore.Settings;
using Revalee.NetCore.Startup;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Recurring
{
    public sealed class RecurringTaskModule : IRecurringTaskModule
    {
        static TaskManifest _manifest;
        readonly HttpContext _context;

        public RecurringTaskModule(IHttpContextAccessor context)
        {
            _context = context?.HttpContext;
        }

        internal static async Task PrepareManifest(HttpContext context, IRevaleeClientRecurringSettings recurringConfig, IRevaleeRegistrar revalee, IOptions<RevaleeOptions> option)
        {
            _manifest = await LoadManifest(context, recurringConfig, revalee, option);
        }
        internal static TaskManifest Manifest => _manifest;
        public RecurringTaskCallbackDetails CallbackDetails => (RecurringTaskCallbackDetails)_context.Items[RouteKeys.IN_PROCESS_CONTEXT_KEY];

        public bool IsProcessingRecurringCallback => _context.Items.ContainsKey((RouteKeys.IN_PROCESS_CONTEXT_KEY));

        public ITaskManifest GetManifest() => _manifest;

        public async Task Restart()
        {
            if (_manifest != null)
            {
                if (!_manifest.IsEmpty)
                {
                    if (!_manifest.IsActive)
                    {
                        await _manifest.Start();
                    }
                }
            }
        }

        static async Task<TaskManifest> LoadManifest(HttpContext context, IRevaleeClientRecurringSettings recurringConfig, IRevaleeRegistrar revalee, IOptions<RevaleeOptions> option)
        {
            TaskManifest manifest = null;

            if (recurringConfig == null || !recurringConfig.TaskModel.Any())
            {
                manifest = new TaskManifest(revalee, context, option);
            }
            else
            {
                manifest = new TaskManifest(revalee, context, option, recurringConfig);

                if (!manifest.IsEmpty)
                {
                    await manifest.Start();
                }
            }

            return manifest;
        }

    }
}
