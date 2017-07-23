using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Revalee.NetCore.Configuration;
using Revalee.NetCore.Internal;
using Revalee.NetCore.Recurring;
//using Revalee.NetCore.Recurring;
using Revalee.NetCore.Settings;
using Revalee.NetCore.Validation;

namespace Revalee.NetCore.Startup
{
    public static class Configure
    {
        /// <summary>
        /// Add the Revalee Service to the container
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the revalee service is added</param>
        /// <returns></returns>
        public static IServiceCollection AddRevaleeService(this IServiceCollection services)
        {
            return services.AddRevaleeService(setupOption: null);
        }

        /// <summary>
        /// Add the Revalee Service to the container
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the revalee service is added</param>
        /// <returns></returns>
        public static IServiceCollection AddRevaleeService(this IServiceCollection services, Action<RevaleeOptions> setupOption)
        {
            services.AddMemoryCache();
            //services.AddDistributedMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IRevaleeRegistrar, SchedulingAgent>();
            services.AddSingleton<IRevaleeClientSettingConfiguration, RevaleeClientSettingConfiguration>();
            services.AddSingleton<IRevaleeRecurringSettingConfiguration, RevaleeRecurringSettingConfiguration>();
            services.AddSingleton<IRevaleeSettingConfigurator, RevaleeSettingConfigurator>();
            services.AddSingleton<IRevaleeClientSettings, RevaleeClientSettings>();
            services.AddSingleton<IRevaleeClientRecurringSettings, RevaleeClientRecurringSettings>();
            services.AddSingleton<IRequestValidator, RequestValidator>();
            services.AddSingleton<ICallbackStateCache, CallbackStateCache>();
            services.AddTransient<IRecurringTaskModule, RecurringTaskModule>();

            if (setupOption != null)
            {
                services.Configure(setupOption);
            }

            return services;
        }

        static IConfiguration _revaleeConfiguration;

        /// <summary>
        /// Configure your application to use revalee task scheduler
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/></param>
        /// <param name="configuration"> The <see cref="IConfiguration"/> container</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void UseRevalee(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            _revaleeConfiguration = configuration;
            var revaleeSetting = app.ApplicationServices.GetService<IRevaleeSettingConfigurator>();
            if (revaleeSetting == null)
            {
                throw new ArgumentNullException(nameof(revaleeSetting));
            }
            revaleeSetting = revaleeSetting.AddClientConfig(configuration);

        }

        /// <summary>
        /// Configure your application to use revalee recurring task scheduler. You must first call <see cref="UseRevalee(IApplicationBuilder, IConfiguration)"/> method
        /// </summary>
        /// <remarks>You must first call <see cref="UseRevalee(IApplicationBuilder, IConfiguration)"/></remarks>
        /// <param name="app">The <see cref="IApplicationBuilder"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void UseRevaleeRecurringTask(this IApplicationBuilder app)
        {
            if (_revaleeConfiguration == null)
            {
                throw new ArgumentNullException(nameof(_revaleeConfiguration));
            }
            var revaleeSetting = app.ApplicationServices.GetService<IRevaleeSettingConfigurator>();

            if (revaleeSetting == null)
            {
                throw new ArgumentNullException(nameof(revaleeSetting));
            }
            revaleeSetting = revaleeSetting.AddRecurringConfig(_revaleeConfiguration);

            app.UseMiddleware<RecurringTaskModuleMiddleware>();

            //var asd = settingService.RecurringSettingConfiguration.CallbackBaseUri;
            //var asds = settingService.RecurringSettingConfiguration.TaskModel;
        }

    }
}
