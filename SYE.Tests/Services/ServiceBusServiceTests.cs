using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Services
{
    public class ServiceBusServiceTests
    {
        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception()
        {
            var payload = new SubmissionVM();
            
            //arrange
            var mockedLogger = new Mock<ILogger<ServiceBusService>>();
            var mockedServiceBusConfig = new Mock<ServiceBusConfiguration>();

            mockedServiceBusConfig.SetupAllProperties();
            mockedServiceBusConfig.Object.ConnectionString = "Endpoint=sb://testLink.servicebus.windows.net/;SharedAccessKeyName=testName;SharedAccessKey=testKey";
            mockedServiceBusConfig.Object.QueueName = "testQueueName";
            mockedServiceBusConfig.Object.Enabled = true;

            var sut = new ServiceBusService(mockedServiceBusConfig.Object, mockedLogger.Object);

            //act
            Action action = () => sut.SendMessageAsync(payload);

            //assert
            action.Should().NotThrow<Exception>();

        }

        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception_When_ServiceBus_Config_Invalid()
        {
            var payload = new SubmissionVM();

            //arrange
            var mockedLogger = new Mock<ILogger<ServiceBusService>>();
            var mockedServiceBusConfig = new Mock<ServiceBusConfiguration>();

            mockedServiceBusConfig.SetupAllProperties();
            mockedServiceBusConfig.Object.ConnectionString = "";
            mockedServiceBusConfig.Object.QueueName = "";
            mockedServiceBusConfig.Object.Enabled = true;

            //act
            Action action = () => new ServiceBusService(mockedServiceBusConfig.Object, mockedLogger.Object);

            //assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception_If_Disabled_When_ServiceBus_Config_Invalid()
        {
            var payload = new SubmissionVM();

            //arrange
            var mockedLogger = new Mock<ILogger<ServiceBusService>>();
            var mockedServiceBusConfig = new Mock<ServiceBusConfiguration>();

            mockedServiceBusConfig.SetupAllProperties();
            mockedServiceBusConfig.Object.ConnectionString = "";
            mockedServiceBusConfig.Object.QueueName = "";
            mockedServiceBusConfig.Object.Enabled = false;

            var sut = new ServiceBusService(mockedServiceBusConfig.Object, mockedLogger.Object);

            //act
            Action action = () => sut.SendMessageAsync(payload);

            //assert
            action.Should().NotThrow<Exception>();
        }

    }
}
