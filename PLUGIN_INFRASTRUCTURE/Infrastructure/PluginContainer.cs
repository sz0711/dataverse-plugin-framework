using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginInfrastructure.Infrastructure
{
    /// <summary>
    /// Lightweight IoC (Inversion of Control) container for plugin dependency injection.
    /// Supports type registration, instance caching, and automatic constructor resolution.
    /// </summary>
    public class PluginContainer
    {
        private readonly IServiceProvider _provider;

        private readonly Dictionary<Type, Type> _registrations = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public PluginContainer(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Register an interface with its implementation.
        /// </summary>
        public void Register<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            _registrations[typeof(TInterface)] = typeof(TImplementation);
        }

        /// <summary>
        /// Register a singleton instance.
        /// </summary>
        public void RegisterInstance<T>(T instance)
        {
            _instances[typeof(T)] = instance;
        }

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type type)
        {
            // Check if instance is registered
            if (_instances.ContainsKey(type))
                return _instances[type];

            // Check if type is registered
            if (_registrations.ContainsKey(type))
                return CreateInstance(_registrations[type]);

            // Fall back to service provider (for SDK services)
            return _provider.GetService(type);
        }

        private object CreateInstance(Type type)
        {
            var ctor = type.GetConstructors().First();

            var parameters = ctor.GetParameters();

            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = Resolve(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(type, args);
        }
    }
}
