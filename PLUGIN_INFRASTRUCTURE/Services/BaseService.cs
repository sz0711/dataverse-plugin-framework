using Microsoft.Xrm.Sdk;

namespace PluginInfrastructure.Services
{
    /// <summary>
    /// Base class for all plugin domain services.
    /// Provides common access to SDK services (tracing, organization service).
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        /// Tracing service for logging.
        /// </summary>
        protected ITracingService Trace { get; }

        /// <summary>
        /// Organization service for CRM operations.
        /// </summary>
        protected IOrganizationService Service { get; }

        /// <summary>
        /// Initialize service with SDK dependencies.
        /// </summary>
        protected BaseService(IOrganizationService service, ITracingService trace)
        {
            Service = service;
            Trace = trace;
        }
    }
}
