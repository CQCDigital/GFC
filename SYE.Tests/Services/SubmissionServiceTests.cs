using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Services
{
    //TODO Discuss how will we handle exceptions/edge cases and add tests accordingly
    /// <summary>
    /// This class tests the submission service is talking to the repository correctly
    /// To achieve this the repository needs to be mocked with faked return data
    /// </summary>
    public class SubmissionServiceTests
    {
        //private Mock<IGenericRepository<SubmissionVM>> _mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
        //private Mock<IServiceBusService> _mockedServiceBusService = new Mock<IServiceBusService>();
        //private SubmissionService _sut;

        //public SubmissionServiceTests()
        //{
        //    _sut = new SubmissionService(_mockedRepo.Object, _mockedServiceBusService.Object);
        //}
        
        [Fact]
        public async void CreateAsync_Should_Not_Be_null()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<SubmissionVM>())).ReturnsAsync(new SubmissionVM { Id = id });
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();

            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);
            //act
            var result = await sut.CreateAsync(new SubmissionVM { Id = id });
            //assert
            result.Should().NotBeNull();
        }
        [Fact]
        public async void CreateAsync_Should_Return_Correct_Data()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<SubmissionVM>())).ReturnsAsync(new SubmissionVM { Id = id });
            //act
            var result = await sut.CreateAsync(new SubmissionVM { Id = id });
            //assert
            result.Id.Should().Be(id);
        }

        [Fact]
        public void DeleteAsync_Should_Not_Throw_Exception()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            mockedRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()));
            // Act
            Action action = () => sut.DeleteAsync(id);
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void GetByIdAsync_Should_Not_Be_Null()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var doc = new DocumentResponse<SubmissionVM>(submissionVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
            //act
            var result = await sut.GetByIdAsync(id);
            //assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async void GetByIdAsync_Should_Return_Correct_Data()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var doc = new DocumentResponse<SubmissionVM>(submissionVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
            //act
            var result = await sut.GetByIdAsync(id);
            //assert
            result.Id.Should().Be(id);
        }

        [Fact]
        public async void FindByAsync_Should_Not_Be_Null()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM {Id = id};
            var query = new List<SubmissionVM> { submissionVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var submissionVms = result as SubmissionVM[] ?? result.ToArray();
            submissionVms.ToList().Should().NotBeNull();
        }

        [Fact]
        public async void FindByAsync_Should_Return_One_Record()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var query = new List<SubmissionVM> { submissionVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var submissionVms = result as SubmissionVM[] ?? result.ToArray();
            submissionVms.Count().Should().Be(1);
        }

        [Fact]
        public async void FindByAsync_Should_Return_Correct_Data()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var query = new List<SubmissionVM> { submissionVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var submissionVms = result as SubmissionVM[] ?? result.ToArray();
            submissionVms.ToList()[0].Id.Should().Be(id);
        }
        [Fact]
        public void UpdateAsyncTest_Should_Not_Throw_Exception()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);     //, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var doc = new SubmissionVM();
            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).ReturnsAsync(doc);
            // Act
            Action action = () => sut.UpdateAsync(id, submissionVm);
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public void UpdateAsyncTest_Should_Throw_Exception()
        {
            const string id = "123";
            var submissionVm = new SubmissionVM { Id = id };

            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);

            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).Throws(new Exception());

            // Act
            Action action = () => sut.UpdateAsync(id, submissionVm);
            // Assert
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception_When_SendMessageAsync_Fails()
        {
            const string id = "123";
            var submissionVm = new SubmissionVM { Id = id };
            var doc = new SubmissionVM();
            
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);

            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).ReturnsAsync(doc);
            mockedServiceBusService.Setup(b => b.SendMessageAsync(It.IsAny<SubmissionVM>())).Throws(new Exception());

            // Act
            Action action = () => sut.SendSubmission(id, submissionVm);
            // Assert
            action.Should().NotThrow<Exception>();
            mockedJobClient.Verify(x => x.Create(
                    It.Is<Job>(job => job.Method.Name == "SendMessageAsync" && job.Args[0] == submissionVm),
                    It.IsAny<EnqueuedState>()
                    ));
        }

        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception_When_BackgroundJob_Fails()
        {
            const string id = "123";
            var submissionVm = new SubmissionVM { Id = id };
            var doc = new SubmissionVM();

            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);

            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).ReturnsAsync(doc);
            mockedServiceBusService.Setup(b => b.SendMessageAsync(It.IsAny<SubmissionVM>())).Throws(new Exception());

            //Ensure exception created by hangfire
            mockedJobClient.Setup(j => j.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Throws(new Exception("testing exception"));

            // Act
            Action action = () => sut.SendSubmission(id, submissionVm);
            // Assert
            action.Should().NotThrow<Exception>();
            mockedJobClient.Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "SendMessageAsync" && job.Args[0] == submissionVm),
                It.IsAny<EnqueuedState>()
            ));
        }

        [Fact]
        public void SendSubmission_Should_Throw_Exception_When_CosmosError()
        {
            const string id = "123";
            var submissionVm = new SubmissionVM { Id = id }; 
            
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);

            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).Throws(new Exception());

            // Act
            Action action = () => sut.SendSubmission(id, submissionVm);
            // Assert
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void SendSubmission_Should_Not_Throw_Exception_When_ServiceBusError()
        {
            const string id = "123";
            var submissionVm = new SubmissionVM { Id = id };
            var doc = new SubmissionVM();

            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedServiceBusService = new Mock<IServiceBusService>();
            var mockedJobClient = new Mock<IBackgroundJobClient>();
            var sut = new SubmissionService(mockedRepo.Object, mockedServiceBusService.Object, mockedJobClient.Object);

            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).ReturnsAsync(doc);
            mockedServiceBusService.Setup(x => x.SendMessageAsync(It.IsAny<SubmissionVM>())).Throws(new Exception());

            // Act
            Action action = () => sut.SendSubmission(id, submissionVm);
            // Assert
            action.Should().NotThrow<Exception>();
        }
    }
}
