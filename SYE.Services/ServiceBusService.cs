using System;
using System.Threading.Tasks;
using SYE.Models.SubmissionSchema;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SYE.Repository;

namespace SYE.Services
{
    /// <summary>
    /// Service to connect to Azure Service Bus specified in appsettings/keyvault and post submissions as messages.
    /// </summary>
    public interface IServiceBusService
    {
        Task SendMessageAsync(SubmissionVM item);
    }

    /// <inheritdoc cref="IServiceBusService"/>
    public class ServiceBusService: IServiceBusService
    {
        //private vars
        private readonly ILogger _internalLogger;
        private string _connectionString;
        private string _queueName;
        private bool _enabled;
        private ServiceBusClient _client;
        private ServiceBusSender _sender;

        public ServiceBusService(IServiceBusConfiguration config, ILogger<ServiceBusService> logger)
        {
            _internalLogger = logger;
            _connectionString = config.ConnectionString;
            _queueName = config.QueueName;
            _enabled = config.Enabled;
            if (config.Enabled)
            {
                try
                {
                    _client = new ServiceBusClient(_connectionString);
                    _sender = _client.CreateSender(_queueName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _internalLogger.LogError(e, "Failed to create Service Bus client or sender");
                    //Todo: Once the data pipe to the service bus enters production we will need to throw this error
                    //throw;
                }
            }
        }

        /// <summary>
        /// Wrap submissionVM in an Azure message format, then post it to the service bus.
        /// </summary>
        /// <param name="submission">Submission to send to the service bus</param>
        public async Task SendMessageAsync(SubmissionVM submission)
        {
            //If disabled, return false
            if (_enabled)
            {
                //Parse submission to json
                var messageBody = JsonConvert.SerializeObject(submission);

                // Wrap json in a message object
                ServiceBusMessage message = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json"
                };

                // send the message
                try
                {
                    await _sender.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    //If errors occur, log them but do not stop process or throw error
                    _internalLogger.LogError(ex, "Failed to send message: message content =  {0}", messageBody);
                    // Todo: Throwing this error will enable hangfire retries - to be implemented at a later date.
                    // throw;
                }
            }
            else
            {
                _internalLogger.LogWarning("Submission not sent to Service Bus: Service Bus disabled. GFC id = {0}", submission.Id);
            }
        }
    }
}
