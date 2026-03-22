using System;
using Microsoft.Xrm.Sdk;
using MODEL;
using DataversePluginFramework.Services.Risk;
using PluginInfrastructure.Constants;
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
        /// If Revenue > threshold, account is marked as strategic and risk is evaluated.
        /// </summary>
        public void Process(Account account)
        {
            if (account == null)
            {
                Trace.Trace("AccountService: Account is null, skipping processing.");
                return;
            }

            if (account.Revenue == null)
            {
                Trace.Trace($"AccountService: Account '{account.Name ?? account.Id.ToString()}' has no revenue data.");
                return;
            }

            if (account.Revenue.Value > PluginConstants.StrategicRevenueThreshold)
            {
                account.Description = PluginConstants.StrategicCustomerDescription;

                Trace.Trace($"AccountService: Strategic account detected: {account.Name} with revenue {account.Revenue.Value}");

                try
                {
                    _risk.Evaluate(account);
                }
                catch (Exception ex)
                {
                    Trace.Trace($"AccountService: Risk evaluation failed for account '{account.Name}': {ex.Message}");
                    // Risk evaluation is non-critical; account processing continues.
                }
            }
            else
            {
                Trace.Trace($"AccountService: Account '{account.Name}' is not strategic. Revenue: {account.Revenue.Value}");
            }
        }
    }
}

