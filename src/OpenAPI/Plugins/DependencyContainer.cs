using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using OpenAPI.Events;

namespace OpenAPI.Plugins
{
    /// <summary>
    ///     A dependency injection container
    /// </summary>
    public class DependencyContainer
    {
        private ConcurrentDictionary<Type, IServiceItem> Services { get; set; }

        public DependencyContainer()
        {
            Services = new ConcurrentDictionary<Type, IServiceItem>();
        }

        /// <summary>
        ///     Tries to resolve a service
        /// </summary>
        /// <param name="dependency">The resolved service.</param>
        /// <typeparam name="TType">The type of service to resolve</typeparam>
        /// <returns>Whether the service was able to be resolved or not</returns>
        public bool TryResolve<TType>(out TType dependency)
        {
            var resolved = Resolve<TType>();
            if (resolved.Equals(default(TType)))
            {
                dependency = default;
                return false;
            }

            dependency = (TType) resolved;

            return true;
        }

        /// <summary>
        ///     Tries to resolve a service
        /// </summary>
        /// <param name="dependency">The resolved service.</param>
        /// <param name="type">The type of service to resolve</param>
        /// <returns>Whether the service was able to be resolved or not</returns>
        public bool TryResolve(Type type, out object dependency)
        {
            var resolved = Resolve(type);
            if (resolved == null)
            {
                dependency = null;
                return false;
            }

            dependency = resolved;

            return true;
        }

        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <typeparam name="TType">The type to resolve.</typeparam>
        /// <returns>The resolved service</returns>
        public TType Resolve<TType>()
        {
            if (Services.TryGetValue(typeof(TType), out var value))
            {
                return (TType) value.GetInstance();
            }

            return default;
        }

        /// <summary>
        ///     Resolve a service
        /// </summary>
        /// <returns>The resolved service</returns>
        public object Resolve(Type type)
        {
            if (Services.TryGetValue(type, out var value))
            {
                return value.GetInstance();
            }

            return null;
        }

        /// <summary>
        ///     Remove a service from dependency injection
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public void Remove(Type type)
        {
            if (Services.TryRemove(type, out var serviceItem))
            {
                serviceItem.Dispose();
            }
        }
        
        /// <summary>
        ///     Remove a service from dependency injection
        /// </summary>
        public void Remove<TType>()
        {
            Remove(typeof(TType));
        }
        
        /// <summary>
        ///     Registers a new service
        /// </summary>
        /// <param name="lifetime">How long to keep the service alive for</param>
        /// <typeparam name="TType">The type of service to register</typeparam>
        /// <exception cref="DuplicateTypeException">Thrown when a service of the same type has already been registered</exception>
        public void Register<TType>(DependencyLifetime lifetime = DependencyLifetime.Singleton)
        {
            IServiceItem item;

            switch (lifetime)
            {
                case DependencyLifetime.Singleton:
                    item = new SingletonServiceItem(this, typeof(TType));
                    break;

                case DependencyLifetime.Transient:
                    item = new TransientServiceItem(this, typeof(TType));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
            
            var type = typeof(TType);
            if (!Services.TryAdd(type, item))
            {
                throw new DuplicateTypeException();
            }
        }
        
        /// <summary>
        ///     Registers a new singleton service
        /// </summary>
        /// <param name="value">The instance to use for dependency injection</param>
        /// <typeparam name="TType">The type of service to register</typeparam>
        /// <exception cref="DuplicateTypeException">Thrown when a service of the same type has already been registered</exception>
        public void RegisterSingleton<TType>(TType value)
        {
            var type = typeof(TType);
            if (!Services.TryAdd(type, new SingletonServiceItem(this, type, value)))
            {
                throw new DuplicateTypeException();
            }
        }

        /// <summary>
        ///     Registers a new singleton service
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void RegisterSingleton(Type type, object value)
        {
            if (!Services.TryAdd(type, new SingletonServiceItem(this, type, value)))
            {
                throw new DuplicateTypeException();
            }
        }

        /// <summary>
        ///     Use the DependencyContainer to create an instance for any type with a public constructor.
        /// </summary>
        /// <param name="type">The type of the instance to create</param>
        /// <returns>An instance of <paramref name="type"/></returns>
        /// <exception cref="MissingMethodException">No public constructors were found</exception>
        /// <exception cref="Exception">Could not resolve all required parameters</exception>
        public object CreateInstanceOf(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            
            if (constructors.Length == 0)
                throw new MissingMethodException($"Could not find a public constructor");

            List<object> parameters = new List<object>();
            
            ConstructorInfo resultingConstructor = null;
            foreach (var constructor in constructors)
            {
                var requiredParams = constructor.GetParameters();
                
                foreach (var param in requiredParams)
                {
                    if (!TryResolve(param.ParameterType, out var obj))
                        break;
                    
                    parameters.Add(obj);
                }

                if (parameters.Count == requiredParams.Length)
                {
                    resultingConstructor = constructor;
                    break;
                }
                
                parameters.Clear();
            }
            
            if (resultingConstructor == null)
                throw new Exception("Could not find suitable constructor.");

            var instance = resultingConstructor.Invoke(parameters.ToArray());
            return instance;
        }
        
        /// <summary>
        ///     Use the DependencyContainer to create an instance for any type with a public constructor.
        /// </summary>
        /// <exception cref="MissingMethodException">No public constructors were found</exception>
        /// <exception cref="Exception">Could not resolve all required parameters</exception>
        public TType CreateInstanceOf<TType>()
        {
            return (TType) CreateInstanceOf(typeof(TType));
        }

        private interface IServiceItem : IDisposable
        {
            object GetInstance();
        }

        private abstract class ServiceItemBase : IServiceItem
        {
            protected DependencyContainer Parent { get; }
            protected Type Type { get; }

            public ServiceItemBase(DependencyContainer parent, Type type)
            {
                Parent = parent;
                Type = type;
            }

            protected object Construct()
            {
                return Parent.CreateInstanceOf(Type);
            }
            
            /// <inheritdoc />
            public abstract object GetInstance();


            /// <inheritdoc />
            public abstract void Dispose();
        }

        private class SingletonServiceItem : ServiceItemBase
        {
            private object _value = null;
            public SingletonServiceItem(DependencyContainer parent, Type type, object value) : this(parent, type)
            {
                _value = value;
            }

            public SingletonServiceItem(DependencyContainer parent, Type type) : base(parent, type)
            {
                
            }
            
            /// <inheritdoc />
            public override object GetInstance()
            {
                if (_value == null)
                {
                    _value = Construct();
                }
                return _value;
            }
            
            /// <inheritdoc />
            public override void Dispose()
            {
                
            }
        }
        
        private class TransientServiceItem : ServiceItemBase
        {
            public TransientServiceItem(DependencyContainer parent, Type type) : base(parent, type)
            {
            }
            
            public override object GetInstance()
            {
                return Construct();
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public override void Dispose()
            {
            }
        }
    }

    /// <summary>
    ///     Used to determine a services lifetime
    /// </summary>
    public enum DependencyLifetime
    {
        /// <summary>
        ///     Keep 1 instance throughout the service lifetime
        /// </summary>
        Singleton,
        
        /// <summary>
        ///     Create a new instance everytime it is requested
        /// </summary>
        Transient
    }
}