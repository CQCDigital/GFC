using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moq;
using SYE.Services;
using SYE.Services.Wrappers;
using Xunit;

namespace SYE.Tests.Services
{
    public class SearchServiceTests
    {
        [Fact]
        public void Search_Should_Get_Result()
        {
            //arrange
            var expectedResult = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category1"
            };
            var doc = new Document
            {
                {"id", expectedResult.Id},
                {"locationName", expectedResult.Name},
                {"postalAddressLine1", expectedResult.Address},
                {"postalAddressTownCity", expectedResult.Town},
                {"postalCode", expectedResult.PostCode},
                {"region", expectedResult.Region},
                {"syeInspectionCategories", new[] {"test category1"} }
            };

            var mockedIndexClient = new Mock<ICustomSearchIndexClient>();
            var documentResult = new DocumentSearchResult
            {
                Results = new List<SearchResult> { new SearchResult { Document = doc } }
            };

            mockedIndexClient.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchParameters>())).ReturnsAsync(documentResult);

            //act
            var sut = new SearchService(mockedIndexClient.Object);
            var result = sut.GetPaginatedResult("searchString", 1, 10, string.Empty, true).Result.Data[0];

            //assert
            result.Id.Should().Be(expectedResult.Id);
            result.Name.Should().Be(expectedResult.Name);
            result.Address.Should().Be(expectedResult.Address);
            result.Town.Should().Be(expectedResult.Town);
            result.PostCode.Should().Be(expectedResult.PostCode);
            result.Region.Should().Be(expectedResult.Region);
            result.Category.Should().Be(expectedResult.Category);
            result.Page.Should().Be(1);
            result.Index.Should().Be(1);
        }
        [Fact]
        public void Search_Should_Get_One_Record()
        {
            //arrange
            var setupResult = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var doc = new Document
            {
                {"rid", setupResult.Id},
                {"locationName", setupResult.Name},
                {"postalAddressLine1", setupResult.Address},
                {"postalAddressTownCity", setupResult.Town},
                {"postalCode", setupResult.PostCode},
                {"region", setupResult.Region},
                {"syeInspectionCategories", new[] {"test category1", "test category2"} }
            };

            var mockedIndexClient = new Mock<ICustomSearchIndexClient>();
            var documentResult = new DocumentSearchResult
            {
                Count = 1,
                Results = new List<SearchResult> { new SearchResult { Document = doc } }
            };

            mockedIndexClient.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchParameters>())).ReturnsAsync(documentResult);

            //act
            var sut = new SearchService(mockedIndexClient.Object);
            var result = sut.GetPaginatedResult("searchString", 1, 10, string.Empty, true).Result;

            //assert
            result.Count.Should().Be(1);
        }
        [Fact]
        public void Search_Should_Throw_Exception()
        {
            //arrange
            var mockedIndexClient = new Mock<ICustomSearchIndexClient>();

            mockedIndexClient.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchParameters>())).Throws(new Exception());

            //act
            var sut = new SearchService(mockedIndexClient.Object);

            Func<Task> act = async () => { await sut.GetPaginatedResult("searchString", 1, 10, string.Empty, true); };

            //assert
            act.Should().Throw<Exception>();
        }
        [Fact]
        public void GetCount_Should_Return_One()
        {
            //arrange
            var setupResult = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var doc = new Document
            {
                {"rid", setupResult.Id},
                {"locationName", setupResult.Name},
                {"postalAddressLine1", setupResult.Address},
                {"postalAddressTownCity", setupResult.Town},
                {"postalCode", setupResult.PostCode},
                {"region", setupResult.Region},
                {"syeInspectionCategories", new[] {"test category1", "test category2"} }
            };

            var mockedIndexClient = new Mock<ICustomSearchIndexClient>();
            var documentResult = new DocumentSearchResult
            {
                Count = 1,
                Results = new List<SearchResult> { new SearchResult { Document = doc } }
            };

            mockedIndexClient.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchParameters>())).ReturnsAsync(documentResult);

            //act
            var sut = new SearchService(mockedIndexClient.Object);
            var result = sut.GetPaginatedResult("searchString", 1, 10, string.Empty, true).Result;

            //assert
            result.Count.Should().Be(1);
        }
        [Fact]
        public void GetFacets_Should_Return_Facets()
        {
            //arrange
            var setupResult = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var doc = new Document
            {
                {"rid", setupResult.Id},
                {"locationName", setupResult.Name},
                {"postalAddressLine1", setupResult.Address},
                {"postalAddressTownCity", setupResult.Town},
                {"postalCode", setupResult.PostCode},
                {"region", setupResult.Region},
                {"syeInspectionCategories", new[] {"test category1", "test category2"}}
            };

            var mockedIndexClient = new Mock<ICustomSearchIndexClient>();
            var documentResult = new DocumentSearchResult
            {
                Count = 1,
                Facets = new FacetResults(),
                Results = new List<SearchResult> { new SearchResult { Document = doc } }

            };
            documentResult.Facets.Add("key", new List<FacetResult> { new FacetResult { Value = "value" } });

            mockedIndexClient.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchParameters>())).ReturnsAsync(documentResult);

            //act
            var sut = new SearchService(mockedIndexClient.Object);
            var result = sut.GetPaginatedResult("searchString", 1, 10, string.Empty, true).Result;

            //assert
            result.Count.Should().Be(1);
        }
    }
}