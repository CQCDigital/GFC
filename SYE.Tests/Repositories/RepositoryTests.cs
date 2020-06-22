using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SYE.Models.SubmissionSchema;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using SYE.Repository;
using SYE.Tests.TestHelpers;
using Xunit;
//TODO resolve this issue around mocking the IDocumentClient
/*
namespace SYE.Tests.Repositories
{
    /// <summary>
    /// This class will test the repository methods
    /// In order to do this we need to mock up the component that talks directly to the db (DocumentClient)
    /// We the the DocumentClient to return face data so we can check that the repository method returns the correct data
    /// </summary>
    public class RepositoryTests
    {
        private IGenericRepository<SubmissionVM> _repo;
        private Mock<IDocumentClient> _mockDocumentClient;
        public RepositoryTests()
        {
            Initialise();
        }
        [Fact]
        public async void GetByIdAsyncTest()
        {
            //arrange
            var submissionVm = new SubmissionVM { Id = "123" };
            var doc = new DocumentResponse<SubmissionVM>(submissionVm);
            _mockDocumentClient.Setup(x => x.ReadDocumentAsync<SubmissionVM>(It.IsAny<Uri>(), null, default(CancellationToken))).ReturnsAsync(doc);
            //mockProductRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(mockSubmissionVm);

            //test
            var result = await _repo.GetByIdAsync("123");

            //assert
            result.Should().NotBeNull();
            //Assert.Equal(mockProductDocument.Id, result.Id);
        }
        [Fact]
        public async void FindByAsyncTest()
        {
            var submissionVm = new SubmissionVM { Id = "123" };
            var query = new List<SubmissionVM> { submissionVm }.AsQueryable();

            _mockDocumentClient.Setup(x => x.CreateDocumentQuery<SubmissionVM>(It.IsAny<Uri>(), It.IsAny<string>(), null)).Returns(query);

            var result = await _repo.FindByAsync(m => m.Id == "123");
            result.Should().NotBeNull();
        }
        [Fact]
        public async void CreateAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void UpdateAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void DeleteAsync()
        {
            throw new NotImplementedException();
        }

        private void Initialise()
        {
            //setup the repo
            var appConfig = new AppConfiguration { DatabaseId = "sye_submissions", CollectionId = "456" };
            appConfig.DatabaseId = "";
            appConfig.CollectionId = "456";

            _mockDocumentClient = new Mock<IDocumentClient>();
            var mockConfig = new Mock<IAppConfiguration>();
            _repo = new GenericRepository<SubmissionVM>(appConfig, _mockDocumentClient.Object);
        }
    }
}
*/