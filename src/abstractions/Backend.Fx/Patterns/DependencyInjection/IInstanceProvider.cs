using System;
using System.Collections;
using System.Collections.Generic;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IInstanceProvider
    {
        /// <summary>
        /// Gets a service instance valid for the scope by providing its type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object GetInstance(Type serviceType);

        /// <summary>
        /// Gets all service instances valid for the scope by providing their type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IEnumerable GetInstances(Type serviceType);

        /// <summary>
        /// Gets a service instance valid for the scope by providing its type via generic type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetInstance<T>() where T : class;

        /// <summary>
        /// Gets all service instances valid for the scope by providing their type via generic type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetInstances<T>() where T : class;
    }
}