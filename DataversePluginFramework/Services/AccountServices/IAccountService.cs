using MODEL;

namespace DataversePluginFramework.Services.AccountServices
{
    /// <summary>
    /// Service interface for processing account business logic.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Process account and apply business rules.
        /// </summary>
        void Process(Account account);
    }
}

