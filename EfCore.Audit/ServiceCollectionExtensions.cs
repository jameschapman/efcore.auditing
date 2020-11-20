using System;
using Microsoft.Extensions.DependencyInjection;

namespace EfCore.Audit
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Sets up Entity Framework auditing and registers any subscribers
        /// </summary>
        public static IServiceCollection RegisterEntityFrameworkAuditing(this IServiceCollection services,
            ServiceLifetime serviceLifetime,
            Action<AuditOptions> configureOptions = null)
        {
            return services.RegisterEntityFrameworkAuditing(serviceLifetime, provider =>
            {
                var options = new AuditOptions();
                configureOptions?.Invoke(options);
                return options;
            });
        }

        public static IServiceCollection RegisterEntityFrameworkAuditing(this IServiceCollection services,
            ServiceLifetime serviceLifetime,
            Func<IServiceProvider, object> factory)
        {
            services.Add(new ServiceDescriptor(typeof(AuditOptions), factory, serviceLifetime));

            return services;
        }
    }
}