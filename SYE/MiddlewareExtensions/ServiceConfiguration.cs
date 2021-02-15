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
using SYE.Helpers;
using SYE.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Logging;

namespace SYE.MiddlewareExtensions
{
    public static class ServiceConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration Config)
        {
            var helper = new ServiceConfigurationHelper(services, Config);

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

            //Service Bus configuration
            var serviceBusConfiguration = Config.GetSection("ConnectionStrings:ServiceBus").Get<ServiceBusConfiguration>();
            helper.ConfirmPresent(serviceBusConfiguration);
            services.TryAddSingleton<IServiceBusConfiguration>(serviceBusConfiguration);
            services.TryAddSingleton<IServiceBusService>(provider => new ServiceBusService(provider.GetRequiredService<IServiceBusConfiguration>(), provider.GetRequiredService<ILogger<ServiceBusService>>()));
            
            //Hangfire fire-and-forget service (used for Service Bus)
            services.AddHangfire(c => c.UseMemoryStorage());
            //services.AddHangfireServer();
            services.TryAddSingleton<IBackgroundJobClient>(new BackgroundJobClient(new MemoryStorage()));

            //Search and location service configuration
            var searchConfiguration = Config.GetSection("ConnectionStrings:SearchDb").Get<SearchConfiguration>();
            helper.ConfirmPresent(searchConfiguration);
            services.TryAddSingleton<ICustomSearchIndexClient>(new CustomSearchIndexClient(searchConfiguration.SearchServiceName, searchConfiguration.IndexName, searchConfiguration.SearchApiKey));

            var locationDbConfig = Config.GetSection("ConnectionStrings:LocationSearchCosmosDB").Get<LocationConfiguration>();
            helper.ConfirmPresent(locationDbConfig);
            var locationDbPolicy = Config.GetSection("CosmosDBConnectionPolicy").Get<ConnectionPolicy>() ?? ConnectionPolicy.Default;
            
            SecureString secKey = new SecureString();
            locationDbConfig.Key.ToCharArray().ToList().ForEach(secKey.AppendChar);
            secKey.MakeReadOnly();

            services.TryAddSingleton<IDocClient>(new DocClient { Endpoint = locationDbConfig.Endpoint, Key = secKey, Policy = locationDbConfig.Policy});

            var locationDb = Config.GetSection("CosmosDBCollections:LocationSchemaDb").Get<LocationConfig<Location>>();
            helper.ConfirmPresent(locationDb);
            services.TryAddSingleton<ILocationConfig<Location>>(locationDb);
                             
            
            //Cosmos database connection configuration
            var cosmosDatabaseConnectionConfiguration = Config.GetSection("ConnectionStrings:DefaultCosmosDB").Get<CosmosConnection>();
            helper.ConfirmPresent(cosmosDatabaseConnectionConfiguration);
            var cosmosDatabaseConnectionPolicy = Config.GetSection("CosmosDBConnectionPolicy").Get<ConnectionPolicy>() ?? ConnectionPolicy.Default;

            services.TryAddSingleton<IDocumentClient>(
                new DocumentClient(
                    new Uri(cosmosDatabaseConnectionConfiguration.Endpoint),
                    cosmosDatabaseConnectionConfiguration.Key,
                    cosmosDatabaseConnectionPolicy
                )
            );

            //Add IAppConfiguration services for each database object
            helper.CreateDbObjectSingleton<FormVM>("FormSchemaDb");
            helper.CreateDbObjectSingleton<SubmissionVM>("SubmissionsDb");
            helper.CreateDbObjectSingleton<DocumentVm>("SubmissionsDb");
            helper.CreateDbObjectSingleton<ConfigVM>("ConfigDb");
            helper.CreateDbObjectSingleton<PfSurveyVM>("PostFeedbackDb");
            helper.CreateDbObjectSingleton<UserActionVM>("ActionsDb");

            
            //ESB service
            var esbConfig = Config.GetSection("ConnectionStrings:EsbConfig").Get<EsbConfiguration<EsbConfig>>();
            helper.ConfirmPresent(esbConfig);
            services.AddSingleton<IEsbConfiguration<EsbConfig>>(esbConfig);
            services.TryAddSingleton<IEsbWrapper>(new EsbWrapper(esbConfig));


            //Other general services
            IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            services.AddSingleton<IFileProvider>(physicalProvider);

            services.TryAddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.TryAddScoped(typeof(ILocationRepository<>), typeof(LocationRepository<>));

            services.TryAddScoped<IEsbConfiguration<EsbConfig>, EsbConfiguration<EsbConfig>>();
        }
    }
}
