using Microsoft.Azure.Documents.Client;
using Moq;
using SYE.Models;
using SYE.Repository;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Services
{
    public class CosmosServiceTests
    {

        [Fact]
        public void CosmosService_Should_Return_DocumentId()
        {
            //Arrange
            var mockedRepo = new Mock<IGenericRepository<DocumentVm>>();                                  
            var sut = new CosmosService(mockedRepo.Object);

            var docVm = new DocumentVm { DocumentId = 123 };
            mockedRepo.Setup(x => x.GetDocumentId(It.IsAny<string>())).Returns(docVm);

            //Act
            var guid = "abc-123";
            var result = sut.GetDocumentId(guid);

            //Assert
            Assert.Equal(123, result);
        }

    }
}
