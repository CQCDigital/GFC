using Microsoft.Extensions.DependencyInjection;
using SYE.Models;
using SYE.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SYE.Helpers.DIAutoReg
{
    public static class AutoRegister
    {

        /// <summary>
        /// This finds all the public, non-generic, non-nested classes in an assembly in the provided assemblies.
        /// If no assemblies provided then it scans the assembly that called the method
        /// </summary>
        /// <param name="services">the NET Core dependency injection service</param>
        /// <param name="assemblies">Each assembly you want scanned. If null then scans the the assembly that called the method</param>
        /// <returns></returns>
        public static RegisterData RegisterAssemblyPublicNonGenericClasses(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            var allPublicTypes = assemblies.SelectMany(x => x.GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested));
            return new RegisterData(services, allPublicTypes);
        }
       
        public static IServiceCollection AsPublicImplementedInterfaces(this RegisterData autoRegData)
        {
            if (autoRegData == null) throw new ArgumentNullException(nameof(autoRegData));
            foreach (var classType in autoRegData.TypesToConsider)
            {               
                var attributeValue = classType.GetAttributeValue((LifeTimeAttribute lifeTime) => lifeTime.name);

                if (attributeValue != LifeTime.Ignore)
                {
                    var interfaces = classType.GetTypeInfo().ImplementedInterfaces;
                    foreach (var infc in interfaces.Where(i => i != typeof(IDisposable) && i.IsPublic && !i.IsNested))
                    {
                        autoRegData.Services.Add(new ServiceDescriptor(infc, classType, lifetime(attributeValue)));
                    }
                }
            }

            return autoRegData.Services;
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector)
        where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }

        public static ServiceLifetime lifetime(LifeTime lifeTime)
        {
            switch (lifeTime)
            {
                case LifeTime.Transient:
                    return ServiceLifetime.Transient;
                case LifeTime.Singleton:
                    return ServiceLifetime.Singleton;               
                default:                    
                    return ServiceLifetime.Scoped;
            }           
            
        }               

    }
}
