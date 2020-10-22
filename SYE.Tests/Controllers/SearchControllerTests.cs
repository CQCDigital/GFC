using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using GDSHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using SYE.Controllers;
using SYE.Helpers;
using SYE.Models;
using SYE.Models.Response;
using SYE.Services;
using SYE.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using System.Web;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using NSubstitute;
using SYE.Filters;

namespace SYE.Tests.Controllers
{
    public class SearchControllerTests 
    {
        private Mock<ISessionService> mockSession;
        private Mock<ISearchService> mockService;
        private Mock<IOptions<ApplicationSettings>> mockSettings;
        private Mock<IGdsValidation> mockValidation;
        private Mock<IPageHelper> mockPageHelper;
        private Mock<IUrlHelper> mockUrlHelper;
        private Mock<HttpContext> mockHttpContext;
        private Mock<IConfiguration> mockConfiguration;
        private ActionContext actionContext;
        private ApplicationSettings appSettings;
        
        public SearchControllerTests()
        {
            mockSession = new Mock<ISessionService>();
            mockService = new Mock<ISearchService>();           
            mockSettings = new Mock<IOptions<ApplicationSettings>>();
            mockValidation = new Mock<IGdsValidation>();
            mockUrlHelper = new Mock<IUrlHelper>();
            mockHttpContext = new Mock<HttpContext>();
            mockConfiguration = new Mock<IConfiguration>();
            mockPageHelper = new Mock<IPageHelper>();

            actionContext = new ActionContext()
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };
            
            appSettings = new ApplicationSettings() {
                GFCUrls = new GFCUrls() {
                    StartPage = "https://www.cqc.org.uk/give-feedback-on-care", 
                    RedirectUrl = "https://www.cqc.org.uk/give-feedback-on-care"
                },
                FormStartPage = "test",
                SiteTextStrings = new SiteTextStrings() {
                    ReviewPage = "check-your-answers",
                    ReviewPageId = "CheckYourAnswers",
                    BackLinkText = "test back link",
                    SiteTitleSuffix = " - test suffix",
                    DefaultServiceName = "default service name",
                    CategoriesForExactlyWhereQuestion = new List<string> { "Category in list" }
                },
                QuestionStrings = new QuestionStrings()
                {
                    GoodBadFeedbackQuestion = new GoodBadFeedbackQuestion()
                    {
                        id = "test-goodBadFeedbackQuestion",
                        GoodFeedbackAnswer = "good-journey"
                    }

                }
            };
            mockSettings.Setup(ap => ap.Value).Returns(appSettings);

        }
        [Fact]
        public void ShouldBeRedirectedToCQCStartpageWhenUserVisitDirectlyToFindAServicePage()
        {

            // arrange
            var httpContext = new DefaultHttpContext();

            var context = new ActionExecutingContext(
             actionContext,
             new List<IFilterMetadata>(),
             new Dictionary<string, object>(),
             new Mock<Controller>().Object
             );

            //Act
            var sut = new RedirectionFilter(mockSettings.Object, mockSession.Object);

            sut.OnActionExecuting(context);

            //Assert
            context.HttpContext.Response.Headers.Values.ElementAtOrDefault(0)
                .Should()
                .BeEquivalentTo("https://www.cqc.org.uk/give-feedback-on-care");
        }

        [Fact]
        public void SearchResultsShouldGetCorrectResult()
        {
            //arrange
            var expectedRecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category",
                Index = 1,
                Page = 1
            };
            var expectedResult = new SearchServiceResult() { Data = new List<SearchResult>() };
            expectedResult.Data.Add(expectedRecord);
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;
            var result = sut.SearchResults("search");

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);
            var resultToCompare = model.Data[0];
            resultToCompare.Id.Should().Be(expectedRecord.Id);
            resultToCompare.Name.Should().Be(expectedRecord.Name);
            resultToCompare.Address.Should().Be(expectedRecord.Address);
            resultToCompare.PostCode.Should().Be(expectedRecord.PostCode);
            resultToCompare.Town.Should().Be(expectedRecord.Town);
            resultToCompare.Region.Should().Be(expectedRecord.Region);
            resultToCompare.Category.Should().Be(expectedRecord.Category);
            resultToCompare.Index.Should().Be(1);
            resultToCompare.Page.Should().Be(1);
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldReturnEmptyList()
        {
            //arrange
            SearchServiceResult expectedResult = null;

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults("search");

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.Count.Should().Be(0);
            model.ShowResults.Should().Be(true);
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldReturnMaxSearchCharsError()
        {
            //arrange
            var search = new string('a', 5000);
            var cleansearch = search;

            var mockUrlHelper = new Mock<IUrlHelper>();
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(cleansearch);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults(search);

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowExceededMaxLengthMessage.Should().Be(true);
            model.ShowResults.Should().Be(false);
        }

        [Fact]
        public void SearchResults_Should_Return_Internal_Error()
        {
            //arrange            
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            Action act = () => sut.SearchResults("search");
            //assert
            act.Should().Throw<Exception>().Where(ex => ex.Data.Contains("GFCError"));
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldGetCorrectCountResult()
        {
            //arrange
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult() { Data = new List<Models.SearchResult>() };
            expectedResult.Data.Add(expectedrecord);

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults("search", 1);

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);
            model.Data.Count.Should().Be(1);
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldGetCorrectFacetResult()
        {
            //arrange
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult()
            {
                Facets = new EditableList<string> { "Test Facet" },
                Data = new List<Models.SearchResult>()
            };
            expectedResult.Data.Add(expectedrecord);

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults("search", 1, "TestFacet");

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);
            model.Facets.Count.Should().Be(1);
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldGetCorrectSelectedFacetResult()
        {
            //arrange
            var search = "test search";
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult()
            {
                Facets = new EditableList<string> { "TestFacet" },
                Data = new List<Models.SearchResult>()
            };
            expectedResult.Data.Add(expectedrecord);

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockSession.Setup(x => x.GetUserSearch()).Returns(search);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(search);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults(search, 1, "TestFacet");

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);
            model.Facets[0].Selected.Should().Be(true);
            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldApplyCorrectSelectedFacet()
        {
            //arrange
            var search = "test search";
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult() { Data = new List<SearchResult> { expectedrecord } };

            var facets = new List<SelectItem>
                    {
                        new SelectItem {Text = "Facet1", Selected = true},
                        new SelectItem {Text = "Facet2", Selected = true},
                        new SelectItem {Text = "Facet3", Selected = false},
                        new SelectItem {Text = "Face41", Selected = false}
                    };
            var facetsModal = new List<SelectItem> { }; //Pass empty list of Modal facets
            var expectedTotalCount = facets.Count();
            var expectedSelectedCount = facets.Count(x => x.Selected);

            var facetsList = new EditableList<string>();
            facetsList.AddRange(facets.Select(x => x.Text));
            expectedResult.Facets = facetsList;

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockSession.Setup(x => x.GetUserSearch()).Returns(search);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(search);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            //var result = sut.ApplyFilter(search, facets, facetsModal);
            var redirectResult = sut.ApplyFilter(search, facets, facetsModal) as RedirectToActionResult;
            var mySearch = ((object[])redirectResult.RouteValues.Values)[0];
            var myFilter = ((object[])redirectResult.RouteValues.Values)[2];
            var result = sut.SearchResults(mySearch.ToString(), 1, myFilter.ToString());

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);

            //check number of facets is correct
            model.Facets.Count.Should().Be(expectedTotalCount);
            model.FacetsModal.Count.Should().Be(expectedTotalCount);
            model.Facets.Count(x => x.Selected).Should().Be(expectedSelectedCount);
            model.FacetsModal.Count(x => x.Selected).Should().Be(expectedSelectedCount);

            //check the selected facets are correct
            var selected = model.Facets.Where(x => x.Selected).Select(x => x.Text).ToList();
            foreach (var facet in facets.Where(x => x.Selected))
            {
                selected.Contains(facet.Text).Should().BeTrue();
            }
            var selectedModal = model.FacetsModal.Where(x => x.Selected).Select(x => x.Text).ToList();
            foreach (var facet in facetsModal.Where(x => x.Selected))
            {
                selectedModal.Contains(facet.Text).Should().BeTrue();
            }

            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldApplyCorrectSelectedFacetWhenModalFacetsSupplied()
        {
            //arrange
            var search = "test search";
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult() { Data = new List<SearchResult> { expectedrecord } };

            var facets = new List<SelectItem> { };  //Pass empty list of standard facets
            var facetsModal = new List<SelectItem>
                    {
                        new SelectItem {Text = "Facet1", Selected = true},
                        new SelectItem {Text = "Facet2", Selected = true},
                        new SelectItem {Text = "Facet3", Selected = false},
                        new SelectItem {Text = "Face41", Selected = false}
                    };
            var expectedTotalCount = facetsModal.Count();
            var expectedSelectedCount = facetsModal.Count(x => x.Selected);

            var facetsList = new EditableList<string>();
            facetsList.AddRange(facetsModal.Select(x => x.Text));
            expectedResult.Facets = facetsList;

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockSession.Setup(x => x.GetUserSearch()).Returns(search);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(search);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            //var result = sut.ApplyFilter(search, facets, facetsModal);
            var redirectResult = sut.ApplyFilter(search, facets, facetsModal) as RedirectToActionResult;
            var mySearch = ((object[])redirectResult.RouteValues.Values)[0];
            var myFilter = ((object[])redirectResult.RouteValues.Values)[2];
            var result = sut.SearchResults(mySearch.ToString(), 1, myFilter.ToString());

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);

            //check number of facets is correct
            model.Facets.Count.Should().Be(expectedTotalCount);
            model.FacetsModal.Count.Should().Be(expectedTotalCount);
            model.Facets.Count(x => x.Selected).Should().Be(expectedSelectedCount);
            model.FacetsModal.Count(x => x.Selected).Should().Be(expectedSelectedCount);

            //check the selected facets are correct
            var selected = model.Facets.Where(x => x.Selected).Select(x => x.Text).ToList();
            foreach (var facet in facets.Where(x => x.Selected))
            {
                selected.Contains(facet.Text).Should().BeTrue();
            }
            var selectedModal = model.FacetsModal.Where(x => x.Selected).Select(x => x.Text).ToList();
            foreach (var facet in facetsModal.Where(x => x.Selected))
            {
                selectedModal.Contains(facet.Text).Should().BeTrue();
            }

            mockService.Verify();
        }

        [Fact]
        public void SearchResultsShouldNotApplyFacetsIfTwoSetsOfFacetsSupplied()
        {
            //arrange
            var search = "test search";
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new SearchServiceResult() { Data = new List<SearchResult> { expectedrecord } };

            var facets = new List<SelectItem>
                    {
                        new SelectItem {Text = "Facet1", Selected = true},
                        new SelectItem {Text = "Facet2", Selected = false},
                        new SelectItem {Text = "Facet3", Selected = true},
                        new SelectItem {Text = "Face41", Selected = false}
                    };
            var facetsModal = new List<SelectItem>
                    {
                        new SelectItem {Text = "Facet1", Selected = true},
                        new SelectItem {Text = "Facet2", Selected = true},
                        new SelectItem {Text = "Facet3", Selected = false},
                        new SelectItem {Text = "Face41", Selected = false}
                    };

            var expectedTotalCount = facets.Count();
            var expectedSelectedCount = 0; //As we are passing content that should never be supplied, we want to never select facets to filter by.

            var facetsList = new EditableList<string>();
            facetsList.AddRange(facets.Select(x => x.Text));
            expectedResult.Facets = facetsList;

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockSession.Setup(x => x.GetUserSearch()).Returns(search);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedResult).Verifiable();

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(search);

            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));


            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var redirectResult = sut.ApplyFilter(search, facets, facetsModal) as RedirectToActionResult;
            var mySearch = ((object[])redirectResult.RouteValues.Values)[0];
            var myFilter = ((object[])redirectResult.RouteValues.Values)[2];
            var result = sut.SearchResults(mySearch.ToString(), 1, myFilter.ToString());

            //assert
            //var redirectResult = result as RedirectToActionResult;
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchResultsVM;
            model.ShowResults.Should().Be(true);

            //check number of facets is correct
            model.Facets.Count.Should().Be(expectedTotalCount);
            model.FacetsModal.Count.Should().Be(expectedTotalCount);
            model.Facets.Count(x => x.Selected).Should().Be(expectedSelectedCount);
            model.FacetsModal.Count(x => x.Selected).Should().Be(expectedSelectedCount);

            //check the selected facets are correct - in this case no facets should be selected
            var selected = model.Facets.Where(x => x.Selected).Select(x => x.Text).ToList();
            selected.Should().BeEmpty();

            var selectedModal = model.FacetsModal.Where(x => x.Selected).Select(x => x.Text).ToList();
            selectedModal.Should().BeEmpty();

            mockService.Verify();
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void SearchResultsWithEmptySearchShouldReturnErrorMessage(string search)
        {
            //arrange
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(search);

            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            tempData["search"] = "";
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.Url = mockUrlHelper.Object;
            sut.TempData = tempData;

            var result = sut.SearchResults(search, 1);

            //assert
            var viewResult = result as ViewResult;
            var model = viewResult.Model as SearchResultsVM;
            model.ErrorMessage.Should().NotBe(null);
            model.ErrorMessage.Should().NotBe(string.Empty);

        }


        [Fact]
        public void SearchResultsShouldReturnInternalError()
        {
            //arrange
            var expectedrecord = new Models.SearchResult
            {
                Id = "testid",
                Name = "test location",
                Address = "test address",
                PostCode = "12345",
                Town = "test town",
                Region = "test region",
                Category = "test category"
            };
            var expectedResult = new List<Models.SearchResult>();
            expectedResult.Add(expectedrecord);


            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            Action act = () => sut.SearchResults("search", 1);

            //assert
            act.Should().Throw<Exception>().Where(ex => ex.Data.Contains("GFCError"));
        }

        [Fact]
        public void IndexShouldReturnEmptySearchVm()
        {
            var temData = new Mock<TempDataSerializer>();
            var mockTempDataProvider = new Mock<SessionStateTempDataProvider>(temData.Object);

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");


            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);

            sut.TempData = new TempDataDictionary(mockHttpContext.Object, mockTempDataProvider.Object);
            sut.Url = mockUrlHelper.Object;

            //act
            var result = sut.Index();

            //assert
            var viewResult = result as ViewResult;

            var model = viewResult.Model as SearchVm;
            model.Should().NotBe(null);
            model.SearchTerm.Should().Be(null);
        }

        [Fact]
        public void SelectLocationShouldRedirectToFormIndex()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };
            var goodBadFeedbackQuestions = new List<QuestionVM>()
            {
                new QuestionVM() { QuestionId = "test-goodBadFeedbackQuestion", Answer = "goodBadFeedbackAnswer" }
            };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test", Questions = goodBadFeedbackQuestions } } };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            var testIConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("mockappsettings.json")
                .Build();

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, options, mockValidation.Object, mockPageHelper.Object, testIConfiguration);
            sut.ControllerContext = controllerContext;
            var result = sut.SelectLocation(new UserSessionVM());

            //assert
            var redirectResult = result as RedirectToActionResult;

            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Form");
        }

        [Theory]
        [InlineData("Category Not In List", "True")]
        [InlineData("Category in list", "False")]
        [InlineData("Category in list, Other Category", "False")]
        //If any part of locationCategory matches a listed category, don't skip, just redirect to the default next page as expected.
        public void SelectLocationShould_SetSkipWhereToTrue_OnlyIfListedCategoryNotPresent(string locationCategory, string expectedFlagValue)
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location", LocationCategory = locationCategory};
            var goodBadFeedbackQuestions = new List<QuestionVM>() { new QuestionVM() { QuestionId = "test-goodBadFeedbackQuestion", Answer = "test-journeytype" } };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test", Questions = goodBadFeedbackQuestions} } };
            var defaultNextPage = "DefaultNextPageValue";
            RouteValueDictionary expectedRouteValues = new RouteValueDictionary
            {
                {"id", "DefaultNextPageValue"},
                {"searchReferrer", "select-location"}
            };

            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);

            string returnedFormData = "";
            mockSession.Setup(x => x.UpdateFormData(It.IsAny<IEnumerable<DataItemVM>>()))
                .Callback<IEnumerable<DataItemVM>>((obj) => returnedFormData = obj.FirstOrDefault().Value)
                .Verifiable();

            mockPageHelper.Setup(x => x.GetNextPageIdFromPage(It.IsAny<FormVM>(), It.IsAny<String>()))
                .Returns(defaultNextPage);
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            var testIConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("mockappsettings.json")
                .Build();
            
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, options, mockValidation.Object, mockPageHelper.Object, testIConfiguration);
            sut.ControllerContext = controllerContext;
            var result = sut.SelectLocation(userVm);

            //assert
            var redirectResult = result as RedirectToActionResult;
            
            mockSession.Verify();
            returnedFormData.Should().BeEquivalentTo(expectedFlagValue);
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Form");
        }

        [Fact]
        public void SelectLocationShouldCallSessionToSaveProvider()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.SetUserSessionVars(userVm)).Verifiable();
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, options, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            sut.SelectLocation(userVm);

            //assert
            mockSession.Verify();
        }
        [Fact]
        public void SelectLocationShouldCallSessionToSaveForm()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var oldlocationName = "test location1";
            var newlocationName = "test location2";

            var replacements = new Dictionary<string, string>
            {
                {oldlocationName, newlocationName}
            };


            var goodBadFeedbackQuestions = new List<QuestionVM>() { new QuestionVM() { QuestionId = "test-goodBadFeedbackQuestion" } };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test", Questions = goodBadFeedbackQuestions } } };

            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = oldlocationName };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.SaveFormVmToSession(formVm, replacements)).Verifiable();
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("test location2");

            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, options, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            sut.SelectLocation(new UserSessionVM { LocationName = newlocationName });

            //assert
            var mySession = mockSession.Object.GetUserSession();

            Assert.Equal(mySession.LocationName, userVm.LocationName);
            Assert.Equal(mySession.LocationId, userVm.LocationId);
            Assert.Equal(mySession.LocationCategory, userVm.LocationCategory);

            mockSession.Verify();
        }

        [Fact]
        public void SelectLocationShouldReturnInternalError()
        {
            //arrange
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.SetUserSessionVars(userVm)).Throws(new Exception());

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            Action act = () => sut.SelectLocation(userVm);
            //assert
            act.Should().Throw<Exception>().Where(ex => ex.Data.Contains("GFCError"));
        }
        [Fact]
        public void SearchShouldReturn550StatusCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ISessionService>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>())).Returns(true);
            mockService.Setup(x => x.GetPaginatedResult(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), true)).Throws(new Exception());

            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SearchResults("searchString");
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(550);
        }
        [Fact]
        public void SelectLocationShouldReturn551ErrorCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var formVm = new FormVM { Pages = new List<PageVM> { new PageVM { PageId = "test" } } };
            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.SaveFormVmToSession(It.IsAny<FormVM>(), It.IsAny<Dictionary<string, string>>())).Throws(new Exception());
            mockSession.Setup(x => x.LoadLatestFormIntoSession(It.IsAny<Dictionary<string, string>>())).Throws(new Exception());
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SelectLocation(userVm);
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(551);
        }

        [Fact]
        public void LocationNotFoundShouldReturn552ErrorCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var userVm = new UserSessionVM { ProviderId = "123", LocationId = "234", LocationName = "test location" };

            mockSession.Setup(x => x.GetUserSession()).Returns(userVm);
            mockSession.Setup(x => x.SaveFormVmToSession(It.IsAny<FormVM>(), It.IsAny<Dictionary<string, string>>())).Throws(new Exception());
            mockValidation.Setup(x => x.CleanText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns("abc");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.LocationNotFound();
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(552);
        }

        [Fact]
        public void SearchResultsShouldReturn568ErrorCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            mockSession.Setup(x => x.GetLastPage()).Returns("you-have-sent-your-feedback");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SearchResults("searchString");
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(568);
        }
        [Fact]
        public void IndexShouldReturn568ErrorCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            mockSession.Setup(x => x.GetLastPage()).Returns("you-have-sent-your-feedback");
            //act
            var sut = new SearchController(mockService.Object, mockSession.Object, mockSettings.Object, mockValidation.Object, mockPageHelper.Object, mockConfiguration.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.Index();
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(568);
        }
    }
}
