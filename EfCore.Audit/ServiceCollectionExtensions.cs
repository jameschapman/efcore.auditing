using System;
using Microsoft.Extensions.DependencyInjection;

namespace EfCore.Audit
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Sets up Entity Framework auditing and registers any subscribers
        /// </summary>
        public static IServiceCollection RegisterEntityFrameworkAuditing<TDbContext>(this IServiceCollection services,
            ServiceLifetime serviceLifetime,
            Action<AuditOptions> configureOptions = null,
            Func<IServiceProvider, IPostSaveAction<TDbContext>> postSaveAction = null)
        {
            return services.RegisterEntityFrameworkAuditing1(serviceLifetime, provider =>
            {
                var options = new AuditOptions();
                configureOptions?.Invoke(options);
                return options;
            }, postSaveAction);
        }

        private static IServiceCollection RegisterEntityFrameworkAuditing1<TDbContext>(this IServiceCollection services,
            ServiceLifetime serviceLifetime,
            Func<IServiceProvider, object> factory, Func<IServiceProvider, IPostSaveAction<TDbContext>> postSaveAction)
        {
            services.Add(new ServiceDescriptor(typeof(AuditOptions), factory, serviceLifetime));

            if (postSaveAction != null)
                services.Add(
                    new ServiceDescriptor(typeof(IPostSaveAction<TDbContext>), postSaveAction, serviceLifetime));

            return services;
        }
    }
}