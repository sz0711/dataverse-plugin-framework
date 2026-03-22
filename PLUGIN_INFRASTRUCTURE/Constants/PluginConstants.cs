namespace PluginInfrastructure.Constants
{
    /// <summary>
    /// Centralized constants for plugin business rules and configuration.
    /// Avoids hard-coded magic values scattered across services.
    /// </summary>
    public static class PluginConstants
    {
        /// <summary>
        /// Revenue threshold (in base currency) above which an account qualifies as strategic.
        /// </summary>
        public const decimal StrategicRevenueThreshold = 5_000_000m;

        /// <summary>
        /// Description text applied to accounts classified as strategic customers.
        /// </summary>
        public const string StrategicCustomerDescription = "Strategic Customer";
    }
}
