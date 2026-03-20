using Microsoft.Xrm.Sdk;
using MODEL;
using DataversePluginFramework.Services.Risk;
using PluginInfrastructure.Services;

namespace DataversePluginFramework.Services.AccountServices
{
    /// <summary>
    /// Domain service for account processing logic.
    /// Inherits from BaseService for common SDK dependencies.
    /// Demonstrates constructor injection with SOLID principles.
    /// </summary>
    public class AccountService : BaseService, IAccountService
    {
        private readonly IRiskService _risk;

        public AccountService(
            IOrganizationService service,
            ITracingService trace,
            IRiskService risk)
            : base(service, trace)
        {
            _risk = risk;
        }

        /// <summary>
        /// Process account and evaluate if it's a strategic customer.
        /// If Revenue > 5,000,000, account is marked as "Strategic Customer".
        /// </summary>
        public void Process(Account account)
        {
            if (account?.Revenue == null)
            {
                Trace.Trace("Account has no revenue data.");
                return;
            }

            if (account.Revenue.Value > 5000000)
            {
                account.Description = "Strategic Customer";

                Trace.Trace($"Strategic account detected: {account.Name} with revenue {account.Revenue.Value}");

                // Delegate risk evaluation to the RiskService
                _risk.Evaluate(account);
            }
            else
            {
                Trace.Trace($"Account {account.Name} is not strategic. Revenue: {account.Revenue.Value}");
            }
        }
    }
}

