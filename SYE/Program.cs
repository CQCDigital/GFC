using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SYE
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
         

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .SuppressStatusMessages(false)
                .ConfigureAppConfiguration((builderContext, configurationBuilder) =>
                {
                    var env = builderContext.HostingEnvironment;
                    if (env.IsEnvironment("Local"))
                    {
                        configurationBuilder.AddUserSecrets<Startup>();
                    }
                    else
                    {
                        var builtConfig = configurationBuilder.Build();
                        var keyVaultEndpoint = builtConfig?.GetValue<string>("KeyVaultName");
                        if (!string.IsNullOrWhiteSpace(keyVaultEndpoint))
                        {
                            var azureServiceTokenProvider = new AzureServiceTokenProvider();
                            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                            configurationBuilder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                        }
                    }
                })
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    var env = builderContext.HostingEnvironment;
                    if (!env.IsEnvironment("Local"))
                    {
                        loggingBuilder.AddApplicationInsights();
                    }
                });
        }
    }
}
