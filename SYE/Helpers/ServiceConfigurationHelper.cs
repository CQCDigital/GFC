using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SYE.Repository;

namespace SYE.Helpers
{
    public class ServiceConfigurationHelper
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _config;

        /// <summary>
        /// Helper class for ConfigureServices, containing common tasks
        /// </summary>
        /// <param name="services">IServiceCollection that ConfigureServices operates on</param>
        /// <param name="config">Configuration source json for ConfigureServices</param>
        public ServiceConfigurationHelper(IServiceCollection services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        /// <summary>
        /// Helper method: add IAppConfiguration services for a database object
        /// </summary>
        /// <typeparam name="T">Class representing the database object</typeparam>
        /// <param name="objectReference">Identifier for the database object reference in config, from User Secrets or KeyVault</param>
        public void CreateDbObjectSingleton<T>(string objectReference) where T : class
        {
            var cosmosObject = _config.GetSection($"CosmosDBCollections:{objectReference}").Get<AppConfiguration<T>>();
            ConfirmPresent(cosmosObject);
            _services.TryAddSingleton<IAppConfiguration<T>>(cosmosObject);
        }

        /// <summary>
        /// Helper method: Throws ConfigurationErrorsException if object is null
        /// </summary>
        /// <param name="obj">This must be nullable</param>
        public void ConfirmPresent(Object obj)
        {
            if (obj == null)
                throw new ConfigurationErrorsException($"Failed to load {nameof(obj)} from application configuration.");
        }
    }

}
