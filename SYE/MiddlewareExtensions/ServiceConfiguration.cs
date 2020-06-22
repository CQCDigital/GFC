using ESBHelpers.Config;
using ESBHelpers.Models;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Notify.Client;
using Notify.Interfaces;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using SYE.Services.Wrappers;
using SYE.ViewModels;
using System;
using System.Configuration;
using System.IO;
using System.Security;
using SYE.Models;
using System.Linq;
using SYE.Helpers.DIAutoReg;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Loader;

namespace SYE.MiddlewareExtensions
{
    public static class ServiceConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration Config)
        {
            List<Assembly> allAssemblies = new List<Assembly>();
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var compAssemblyPaths = Directory.GetFiles(path, "*SYE*.dll").ToList();
            
            foreach (string dllPath in compAssemblyPaths)
            {
                if(dllPath.Split('\\').LastOrDefault() != "SYE.Views.dll")
                {
                    var assemblyName = AssemblyLoadContext.GetAssemblyName(dllPath);

                    var assembly = Assembly.Load(assemblyName);

                    services.RegisterAssemblyPublicNonGenericClasses(assembly)                     
                     .AsPublicImplementedInterfaces();
                }                
            }

            services.Configure<ApplicationSettings>(Config.GetSection("ApplicationSettings"));
            services.Configure<CQCRedirection>(Config.GetSection("ConnectionStrings").GetSection("CQCRedirection"));


            services.TryAddSingleton<IGdsValidation, GdsValidation>();


            string notificationApiKey = Config.GetSection("ConnectionStrings:GovUkNotify").GetValue<String>("ApiKey");
            if (string.IsNullOrWhiteSpace(notificationApiKey))
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(notificationApiKey)} from application configuration.");
            }
            services.TryAddSingleton<IAsyncNotificationClient>(_ => new NotificationClient(notificationApiKey));            



            var searchConfiguration = Config.GetSection("ConnectionStrings:SearchDb").Get<SearchConfiguration>();
            if (searchConfiguration == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(searchConfiguration)} from application configuration.");
            }
            services.TryAddSingleton<ICustomSearchIndexClient>(new CustomSearchIndexClient(searchConfiguration.SearchServiceName, searchConfiguration.IndexName, searchConfiguration.SearchApiKey));

            


            var locationDbConfig = Config.GetSection("ConnectionStrings:LocationSearchCosmosDB").Get<LocationConfiguration>();
            if (locationDbConfig == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(locationDbConfig)} from application configuration.");
            }
            var locationDbPolicy = Config.GetSection("CosmosDBConnectionPolicy").Get<ConnectionPolicy>() ?? ConnectionPolicy.Default;

            
            SecureString secKey = new SecureString();
            locationDbConfig.Key.ToCharArray().ToList().ForEach(secKey.AppendChar);
            secKey.MakeReadOnly();

            services.TryAddSingleton<IDocClient>(new DocClient { Endpoint = locationDbConfig.Endpoint, Key = secKey, Policy = locationDbConfig.Policy});
            




            var locationDb = Config.GetSection("CosmosDBCollections:LocationSchemaDb").Get<LocationConfig<Location>>();
            if (locationDb == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(locationDb)} from application configuration.");
            }
            services.TryAddSingleton<ILocationConfig<Location>>(locationDb);
                             
            

            var cosmosDatabaseConnectionConfiguration = Config.GetSection("ConnectionStrings:DefaultCosmosDB").Get<CosmosConnection>();
            if (cosmosDatabaseConnectionConfiguration == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(cosmosDatabaseConnectionConfiguration)} from application configuration.");
            }
            var cosmosDatabaseConnectionPolicy = Config.GetSection("CosmosDBConnectionPolicy").Get<ConnectionPolicy>() ?? ConnectionPolicy.Default;
            services.TryAddSingleton<IDocumentClient>(
                new DocumentClient(
                    new Uri(cosmosDatabaseConnectionConfiguration.Endpoint),
                    cosmosDatabaseConnectionConfiguration.Key,
                    cosmosDatabaseConnectionPolicy
                )
            );
            


            var formSchemaDatabase = Config.GetSection("CosmosDBCollections:FormSchemaDb").Get<AppConfiguration<FormVM>>();
            if (formSchemaDatabase == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(formSchemaDatabase)} from application configuration.");
            }
            services.TryAddSingleton<IAppConfiguration<FormVM>>(formSchemaDatabase);

            
            var submissionsDatabase = Config.GetSection("CosmosDBCollections:SubmissionsDb").Get<AppConfiguration<SubmissionVM>>();
            if (submissionsDatabase == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(submissionsDatabase)} from application configuration.");
            }
            services.TryAddSingleton<IAppConfiguration<SubmissionVM>>(submissionsDatabase);

            

            var configDatabase = Config.GetSection("CosmosDBCollections:ConfigDb").Get<AppConfiguration<ConfigVM>>();
            if (configDatabase == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(configDatabase)} from application configuration.");
            }
            services.TryAddSingleton<IAppConfiguration<ConfigVM>>(configDatabase);


            
            var esbConfig = Config.GetSection("ConnectionStrings:EsbConfig").Get<EsbConfiguration<EsbConfig>>();
            if (esbConfig == null)
            {
                throw new ConfigurationErrorsException($"Failed to load {nameof(esbConfig)} from application configuration.");
            }
            services.AddSingleton<IEsbConfiguration<EsbConfig>>(esbConfig);
            services.TryAddSingleton<IEsbWrapper>(new  EsbWrapper(esbConfig));

            IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            services.AddSingleton<IFileProvider>(physicalProvider);

            

            services.TryAddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.TryAddScoped(typeof(ILocationRepository<>), typeof(LocationRepository<>));
            services.TryAddScoped<IEsbConfiguration<EsbConfig>, EsbConfiguration<EsbConfig>>();
           
        }
    }
}
