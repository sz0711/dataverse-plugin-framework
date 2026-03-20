using MODEL;

namespace DataversePluginFramework.Services.Risk
{
    /// <summary>
    /// Service interface for evaluating account risk.
    /// </summary>
    public interface IRiskService
    {
        /// <summary>
        /// Evaluate risk for an account.
        /// </summary>
        void Evaluate(Account account);
    }
}

