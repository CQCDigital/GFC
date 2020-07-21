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
    /// <summary>
    /// This class tests the submission service for the post-completion feedback survey is talking to the repository correctly. To achieve this the repository needs to be mocked with faked return data
    /// </summary>
    public class PfSurveySubmissionServiceTests
    {
        //Shared repo and service for all tests:
        static Mock<IGenericRepository<PfSurveyVM>> mockedRepo = new Mock<IGenericRepository<PfSurveyVM>>();
        PfSurveySubmissionService sut = new PfSurveySubmissionService(mockedRepo.Object);

        #region CreateAsync tests
        [Fact]
        public async void CreateAsync_Should_Not_Be_null()
        {
            //arrange
            const string id = "123"; 
            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<PfSurveyVM>())).ReturnsAsync(new PfSurveyVM { Id = id });
            //act
            var result = await sut.CreateAsync(new PfSurveyVM { Id = id });
            //assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async void CreateAsync_Should_Return_Correct_Data()
        {
            //arrange
            const string id = "123";
            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<PfSurveyVM>())).ReturnsAsync(new PfSurveyVM { Id = id });
            //act
            var result = await sut.CreateAsync(new PfSurveyVM { Id = id });
            //assert
            result.Id.Should().Be(id);
        }

        [Fact]
        public async void CreateAsync_Should_Not_Throw_Exception()
        {
            //arrange
            const string id = "123";
            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<PfSurveyVM>())).ReturnsAsync(new PfSurveyVM { Id = id });
            // Act
            Action action = () => sut.CreateAsync(new PfSurveyVM { Id = id });
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void CreateAsync_Should_Throw_Exception()
        {
            //arrange
            const string id = "123";
            mockedRepo.Setup(x => x.CreateAsync(It.IsAny<PfSurveyVM>())).Throws(new Exception());
            // Act
            Action action = () => sut.CreateAsync(new PfSurveyVM { Id = id });
            // Assert
            action.Should().Throw<Exception>();
        }
        #endregion

        #region DeleteAsync tests
        [Fact]
        public void DeleteAsync_Should_Not_Throw_Exception()
        {
            //arrange
            const string id = "123";
            mockedRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()));
            // Act
            Action action = () => sut.DeleteAsync(id);
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void DeleteAsync_Should_Throw_Exception()
        {
            //arrange
            const string id = "123";
            mockedRepo.Setup(x => x.DeleteAsync(It.IsAny<string>())).Throws(new Exception());
            // Act
            Action action = () => sut.DeleteAsync(id);
            // Assert
            action.Should().Throw<Exception>();
        }
        #endregion

        #region GetbyIdAsync tests
        [Fact]
        public async void GetByIdAsync_Should_Not_Be_Null()
        {
            //arrange
            const string id = "123"; 
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new DocumentResponse<PfSurveyVM>(pfSurveyVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
            //act
            var result = await sut.GetByIdAsync(id);
            //assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async void GetByIdAsync_Should_Return_Correct_Data()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new DocumentResponse<PfSurveyVM>(pfSurveyVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
            //act
            var result = await sut.GetByIdAsync(id);
            //assert
            result.Id.Should().Be(id);
        }

        [Fact]
        public async void GetByIdAsync_Should_Not_Throw_Exception()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new DocumentResponse<PfSurveyVM>(pfSurveyVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(doc);
            // Act
            Action action = () => sut.GetByIdAsync(id);
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void GetByIdAsync_Should_Throw_Exception()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new DocumentResponse<PfSurveyVM>(pfSurveyVm);
            mockedRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws(new Exception());
            // Act
            Action action = () => sut.GetByIdAsync(id);
            // Assert
            action.Should().Throw<Exception>();
        }
        #endregion

        #region FindByAsync tests
        [Fact]
        public async void FindByAsync_Should_Not_Be_Null()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM {Id = id};
            var query = new List<PfSurveyVM> { pfSurveyVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var pfSurveyVms = result as PfSurveyVM[] ?? result.ToArray();
            pfSurveyVms.ToList().Should().NotBeNull();
        }

        [Fact]
        public async void FindByAsync_Should_Return_One_Record()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var query = new List<PfSurveyVM> { pfSurveyVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var pfSurveyVms = result as PfSurveyVM[] ?? result.ToArray();
            pfSurveyVms.Count().Should().Be(1);
        }

        [Fact]
        public async void FindByAsync_Should_Return_Correct_Data()
        {
            //arrange
            const string id = "123"; 
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var query = new List<PfSurveyVM> { pfSurveyVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            var result = await sut.FindByAsync(m => m.Id == id);
            //assert
            var pfSurveyVms = result as PfSurveyVM[] ?? result.ToArray();
            pfSurveyVms.ToList()[0].Id.Should().Be(id);
        }

        [Fact]
        public async void FindByAsync_Should_Not_Throw_Exception()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var query = new List<PfSurveyVM> { pfSurveyVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).ReturnsAsync(query);
            //act
            Action action = () => sut.FindByAsync(m => m.Id == id);
            //assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void FindByAsync_Should_Throw_Exception()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var query = new List<PfSurveyVM> { pfSurveyVm }.AsQueryable();
            mockedRepo.Setup(x => x.FindByAsync(m => m.Id == id)).Throws(new Exception());
            //act
            Action action = () => sut.FindByAsync(m => m.Id == id);
            //assert
            action.Should().Throw<Exception>();
        }
        #endregion

        #region UpdateAsync tests
        [Fact]
        public void UpdateAsync_Should_Not_Throw_Exception()
        {
            //arrange
            const string id = "123"; 
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new PfSurveyVM();
            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<PfSurveyVM>())).ReturnsAsync(doc);
            // Act
            Action action = () => sut.UpdateAsync(id, pfSurveyVm);
            // Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public async void UpdateAsync_Should_Throw_Exception()
        {
            //arrange
            const string id = "123";
            var pfSurveyVm = new PfSurveyVM { Id = id };
            var doc = new PfSurveyVM();
            mockedRepo.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<PfSurveyVM>())).Throws(new Exception());
            // Act
            Action action = () => sut.UpdateAsync(id, pfSurveyVm);
            // Assert
            action.Should().Throw<Exception>();
        }
        #endregion
    }
}
