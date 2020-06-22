using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
        [Fact]
        public async void CreateAsync_Should_Not_Be_null()
        {
            const string id = "123";
            //arrange
            var mockedRepo = new Mock<IGenericRepository<SubmissionVM>>();
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var mockedAppSettings = new Mock<IConfiguration>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<SubmissionVM>())).ReturnsAsync(new SubmissionVM { Id = id });
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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

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
            var mockedConfigRepo = new Mock<IGenericRepository<ConfigVM>>();
            var mockedConfig = new Mock<IAppConfiguration<ConfigVM>>();
            var mockedAppSettings = new Mock<IConfiguration>();
            //var mockedIdGenerator = new Mock<IUidGeneratorService>();
            var sut = new SubmissionService(mockedRepo.Object, mockedConfigRepo.Object, mockedConfig.Object, mockedAppSettings.Object);

            var submissionVm = new SubmissionVM { Id = id };
            var doc = new SubmissionVM();
            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<SubmissionVM>())).ReturnsAsync(doc);
            // Act
            Action action = () => sut.UpdateAsync(id, submissionVm);
            // Assert
            action.Should().NotThrow<Exception>();
        }
    }
}
