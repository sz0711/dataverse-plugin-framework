using DataversePluginFramework.Services.AccountServices;
using DataversePluginFramework.Services.Risk;
using PluginInfrastructure.Infrastructure;

namespace DataversePluginFramework.Infrastructure
{
    /// <summary>
    /// Plugin-specific service registration for DemoPlugin002.
    /// Centralized registration of all domain services (Account, Risk) used by the plugin.
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Register all domain-specific services.
        /// </summary>
        public static void RegisterServices(PluginContainer container)
        {
            // Register domain services with constructor injection
            container.Register<IAccountService, AccountService>();
            container.Register<IRiskService, RiskService>();

            // Add additional services here:
            // container.Register<INewService, NewService>();
        }
    }
}

