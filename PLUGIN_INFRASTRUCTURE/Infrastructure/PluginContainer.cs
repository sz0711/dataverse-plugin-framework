using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private readonly Dictionary<Type, ConstructorInfo> _ctorCache = new Dictionary<Type, ConstructorInfo>();

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
            {
                var instance = CreateInstance(_registrations[type]);
                _instances[type] = instance;
                return instance;
            }

            // Fall back to service provider (for SDK services)
            return _provider.GetService(type);
        }

        private object CreateInstance(Type type)
        {
            var ctor = GetConstructor(type);
            var parameters = ctor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                args[i] = Resolve(paramType);

                if (args[i] == null)
                    throw new InvalidOperationException(
                        $"PluginContainer: Cannot resolve parameter '{parameters[i].Name}' " +
                        $"of type '{paramType.FullName}' for '{type.FullName}'. " +
                        "Ensure it is registered or available via the IServiceProvider.");
            }

            return Activator.CreateInstance(type, args);
        }

        private ConstructorInfo GetConstructor(Type type)
        {
            if (_ctorCache.TryGetValue(type, out var cached))
                return cached;

            var ctors = type.GetConstructors();
            if (ctors.Length == 0)
                throw new InvalidOperationException(
                    $"PluginContainer: Type '{type.FullName}' has no public constructors.");

            // Use the constructor with the most parameters (greedy strategy)
            var ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
            _ctorCache[type] = ctor;
            return ctor;
        }
    }
}
