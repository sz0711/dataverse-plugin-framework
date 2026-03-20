using DataversePluginFramework.Infrastructure;
using DataversePluginFramework.Services.AccountServices;
using PluginInfrastructure.Infrastructure;
using PluginInfrastructure.Plugins;
using MODEL;

namespace DataversePluginFramework.Plugins
{
    public class AccountPlugin : PluginBase<Account>
    {
        /// <summary>
        /// Register domain-specific services for this plugin.
        /// </summary>
        protected override void RegisterDomainServices(PluginContainer container)
        {
            ServiceRegistration.RegisterServices(container);
        }

        /// <summary>
        /// Handle Account Create event in pre-operation stage.
        /// </summary>
        protected override void OnPreOperationCreate()
        {
            Tracing.Trace("OnPreOperationCreate: Processing new account");
            ProcessAccount();
        }

        /// <summary>
        /// Handle Account Update event in pre-operation stage.
        /// </summary>
        protected override void OnPreOperationUpdate()
        {
            Tracing.Trace("OnPreOperationUpdate: Processing account update");
            ProcessAccount();
        }

        /// <summary>
        /// Process account and evaluate if it's a strategic customer.
        /// </summary>
        private void ProcessAccount()
        {
            // Resolve account service and process
            var accountService = Container.Resolve<IAccountService>();
            accountService.Process(CurrentRecord);
        }
    }
}

