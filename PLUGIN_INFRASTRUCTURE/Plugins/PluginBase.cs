using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using PluginInfrastructure.Infrastructure;

namespace PluginInfrastructure.Plugins
{
    /// <summary>
    /// Generic base class for Dataverse plugins with early binding and type safety.
    /// Implements Template Method pattern for event-driven plugin logic.
    /// Handles SDK service initialization, stage/message routing, and dependency injection.
    /// Plugins should be thin adapters between the Dataverse event pipeline and the domain layer.
    /// </summary>
    public abstract class PluginBase<T> : IPlugin
        where T : Entity
    {
        /// <summary>
        /// IoC container for dependency resolution.
        /// </summary>
        protected PluginContainer Container { get; private set; }

        /// <summary>
        /// Execution context with stage, message, and event information.
        /// </summary>
        protected IPluginExecutionContext Context { get; private set; }

        /// <summary>
        /// Tracing service for plugin logging.
        /// </summary>
        protected ITracingService Tracing { get; private set; }

        /// <summary>
        /// Current record being processed (strongly typed).
        /// </summary>
        protected T CurrentRecord { get; private set; }

        /// <summary>
        /// Main entry point for the plugin execution.
        /// Initializes all infrastructure and routes to appropriate handler methods.
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Get execution context
                var context =
                    (IPluginExecutionContext)serviceProvider
                    .GetService(typeof(IPluginExecutionContext));

                // Get organization service factory
                var factory =
                    (IOrganizationServiceFactory)serviceProvider
                    .GetService(typeof(IOrganizationServiceFactory));

                // Create organization service with current user's credentials
                var orgService =
                    factory.CreateOrganizationService(context.UserId);

                // Get tracing service for logging
                var tracing =
                    (ITracingService)serviceProvider
                    .GetService(typeof(ITracingService));

                // Convert entity to strongly-typed version
                T currentRecord = null;
                if (context.InputParameters.Contains("Target"))
                    currentRecord =  ((Entity)context.InputParameters["Target"])?.ToEntity<T>();


                // Initialize IoC container
                var container = new PluginContainer(serviceProvider);

                // Register SDK services
                container.RegisterInstance<IOrganizationService>(orgService);
                container.RegisterInstance<ITracingService>(tracing);

                // Register domain-specific services (Template Method pattern)
                RegisterDomainServices(container);

                // Store in properties for access in handler methods
                Container = container;
                Context = context;
                Tracing = tracing;
                CurrentRecord = currentRecord;

                // Route to appropriate handler based on stage and message
                RouteExecution();
            }
            catch (Exception ex)
            {
                var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                tracing?.Trace($"[{GetType().Name}] PluginBase.Execute FATAL: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Override in derived class to register domain-specific services.
        /// SDK services (IOrganizationService, ITracingService) are already registered.
        /// </summary>
        protected virtual void RegisterDomainServices(PluginContainer container)
        {
            // Default implementation - no domain services
            // Override in derived class to register plugin-specific services
        }

        /// <summary>
        /// Routes execution to appropriate handler methods based on stage and message.
        /// Template Method pattern - orchestrates the flow.
        /// </summary>
        private void RouteExecution()
        {
            // PluginStage: PreValidation = 10, PreOperation = 20, PostOperation = 40
            const int PreValidation = 10;
            const int PreOperation = 20;
            const int PostOperation = 40;

            var entityId = CurrentRecord?.Id;
            var correlationId = Context.CorrelationId;
            Tracing.Trace($"[{GetType().Name}] Start: {Context.MessageName} | Stage={Context.Stage} | Entity={entityId} | Correlation={correlationId}");

            try
            {
                switch (Context.Stage)
                {
                    case PreValidation:
                    case PreOperation:
                        RoutePreOperation();
                        break;

                    case PostOperation:
                        RoutePostOperation();
                        break;

                    default:
                        Tracing.Trace($"No handler for stage: {Context.Stage}");
                        break;
                }

                Tracing.Trace($"[{GetType().Name}] Completed: {Context.MessageName} | Stage={Context.Stage}");
            }
            catch (Exception ex)
            {
                Tracing.Trace($"[{GetType().Name}] Error in {Context.MessageName} Stage={Context.Stage}: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Routes pre-operation events to specific message handlers.
        /// </summary>
        private void RoutePreOperation()
        {
            switch (Context.MessageName)
            {
                case "Create":
                    OnPreOperationCreate();
                    break;

                case "Update":
                    OnPreOperationUpdate();
                    break;

                case "Delete":
                    OnPreOperationDelete();
                    break;

                default:
                    OnPreOperationOther();
                    break;
            }
        }

        /// <summary>
        /// Routes post-operation events to specific message handlers.
        /// </summary>
        private void RoutePostOperation()
        {
            switch (Context.MessageName)
            {
                case "Create":
                    OnPostOperationCreate();
                    break;

                case "Update":
                    OnPostOperationUpdate();
                    break;

                case "Delete":
                    OnPostOperationDelete();
                    break;

                default:
                    OnPostOperationOther();
                    break;
            }
        }

        /// <summary>
        /// Called on Pre-Operation Create event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPreOperationCreate()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Pre-Operation Update event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPreOperationUpdate()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Pre-Operation Delete event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPreOperationDelete()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Pre-Operation other events.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPreOperationOther()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Post-Operation Create event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPostOperationCreate()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Post-Operation Update event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPostOperationUpdate()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Post-Operation Delete event.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPostOperationDelete()
        {
            // Default implementation - override in derived class
        }

        /// <summary>
        /// Called on Post-Operation other events.
        /// Override in derived class to implement custom logic.
        /// Access CurrentRecord property to get the entity.
        /// </summary>
        protected virtual void OnPostOperationOther()
        {
            // Default implementation - override in derived class
        }
    }
}
