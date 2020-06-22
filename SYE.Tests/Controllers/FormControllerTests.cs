using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using SYE.Controllers;
using SYE.Models;
using SYE.Services;
using SYE.ViewModels;
using Xunit;
using Microsoft.AspNetCore.Http;
using SYE.Helpers;
using SYE.Models.Response;

namespace SYE.Tests.Controllers
{
    /// <summary>
    /// this test class is to ensure that the controller is talking to the service layer correctly.
    /// 
    /// </summary>    
    public class FormControllerTests
    {
        SiteTextStrings textStrings = new SiteTextStrings()
        {
            ReviewPage = "check-your-answers",
            ReviewPageId = "CheckYourAnswers",
            BackLinkText = "test back link",
            SiteTitleSuffix = " - test suffix",
            DefaultServiceName = "default service name"
        };

        //[Fact]
        //public void Index_Should_Return_Data()
        //{
        //    const string id = "123";
        //    const string pageId = "my-page-id";
        //    //arrange
        //    var questions = new List<QuestionVM>
        //    {
        //        new QuestionVM
        //        {
        //            Answer = "qwe", QuestionId = "1",
        //            AnswerLogic = new List<AnswerLogicVM>
        //                {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
        //        },
        //        new QuestionVM
        //        {
        //            Answer = "qwe", QuestionId = "2",
        //            AnswerLogic = new List<AnswerLogicVM>
        //                {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
        //        },
        //        new QuestionVM
        //        {
        //            Answer = "qwe", QuestionId = "3",
        //            AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
        //        },
        //    };
        //    var returnPage = new PageVM { PageId = id, NextPageId = "my-next-page", Questions = questions, PreviousPages = new List<PreviousPageVM>{new PreviousPageVM{Answer = "123", PageId = pageId, QuestionId = "123"}} };
        //    var mockValidation = new Mock<IGdsValidation>();
        //    var mockSession = new Mock<ISessionService>();
        //    var mockUrlHelper = new Mock<IUrlHelper>();
        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockPageHelper = new Mock<IPageHelper>();
        //    mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

        //    mockSession.Setup(x => x.GetPageById(id, false)).Returns(returnPage).Verifiable();
        //    mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "" }).Verifiable();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(new FormVM());            

        //    ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123" };
        //    IOptions<ApplicationSettings> options = Options.Create(appSettings);

        //    var sut = new FormController(mockValidation.Object, mockSession.Object, options, mockLogger.Object, mockPageHelper.Object);
        //    sut.Url = mockUrlHelper.Object;
        //    //act
        //    var result = sut.Index(pageId, id);

        //    //assert
        //    var viewResult = result as ViewResult;
        //    var model = viewResult.ViewData.Model as PageVM;
        //    model.PageId.Should().Be(id);
        //    //mockSession.Verify();
        //}

        [Fact]
        public void Index_Should_Return_561_Error()
        {
            const string id = "123";
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();

            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = null }).Verifiable();

            ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123" };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            var sut = new FormController(mockValidation.Object, mockSession.Object, options, mockLogger.Object, new Mock<IPageHelper>().Object) {ControllerContext = controllerContext};

            //act
            var result = sut.Index(id);

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(561);
            mockSession.Verify();
        }

        //[Fact]
        //public void Index_Should_Return_562_Error()
        //{
        //    const string id = "123";
        //    //arrange
        //    //Controller needs a controller context
        //    var httpContext = new DefaultHttpContext();
        //    var controllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext,
        //    };
        //    PageVM returnPage = null;
        //    var mockValidation = new Mock<IGdsValidation>();
        //    var mockSession = new Mock<ISessionService>();
        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockPageHelper = new Mock<IPageHelper>();
        //    mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

        //    mockSession.Setup(x => x.GetPageById(id, false)).Returns(returnPage).Verifiable();
        //    mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "" }).Verifiable();

        //    var mockSettings = new Mock<IOptions<ApplicationSettings>>();
        //    var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings.Object, mockLogger.Object, mockPageHelper.Object) { ControllerContext = controllerContext };

        //    //act
        //    var result = sut.Index("", id);

        //    //assert
        //    var statusResult = result as StatusResult;
        //    statusResult.StatusCode.Should().Be(562);
        //    //mockSession.Verify();
        //}

        [Fact]
        public void Index_Should_Return_563_Error()
        {
            const string id = "123";
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(id, false)).Throws(new Exception());

            var mockSettings = new Mock<IOptions<ApplicationSettings>>();
            var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings.Object, mockLogger.Object, mockPageHelper.Object) { ControllerContext = controllerContext };

            //act
            var result = sut.Index(new CurrentPageVM { PageId = id });

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(563);
            mockSession.Verify();
        }

        [Fact]
        public void Index_Should_Return_565_Error()
        {
            const string id = "123";
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            PageVM returnPage = new PageVM();
            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(It.IsAny<string>(), It.IsAny<bool>())).Returns(returnPage).Verifiable();
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "" }).Verifiable();

            var mockSettings = new Mock<IOptions<ApplicationSettings>>();
            var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings.Object, mockLogger.Object, mockPageHelper.Object) { ControllerContext = controllerContext };

            //act
            var result = sut.Index(new CurrentPageVM { PageId = id });

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(565);
            mockSession.Verify();
        }

        [Fact]
        public void Index_Should_Return_566_Error()
        {
            const string id = "123";
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            PageVM returnPage = new PageVM();
            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(id, false)).Throws(new Exception());
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "" }).Verifiable();

            ApplicationSettings appSettings = new ApplicationSettings() { SiteTextStrings = textStrings };
            IOptions<ApplicationSettings> mockSettings = Options.Create(appSettings);

            var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings, mockLogger.Object, mockPageHelper.Object) { ControllerContext = controllerContext };

            //act
            var result = sut.Index("", id);

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(562);
            //mockSession.Verify();
        }

        //[Fact]
        //public void Index_Should_Return_Internal_Error()
        //{
        //    const string id = "123";
        //    var mockValidation = new Mock<IGdsValidation>();
        //    var mockSession = new Mock<ISessionService>();
        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    mockSession.Setup(x => x.GetPageById(id, false)).Throws(new Exception()).Verifiable();
        //    mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "" }).Verifiable();
        //    var mockPageHelper = new Mock<IPageHelper>();
        //    mockPageHelper.Setup(x => x.CheckPageHistory(It.IsAny<PageVM>(), It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

        //    var mockSettings = new Mock<IOptions<ApplicationSettings>>();
        //    var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings.Object, mockLogger.Object, mockPageHelper.Object);
        //    // Act
        //    Action action = () => sut.Index("", id);
        //    // Assert
        //    action.Should().Throw<Exception>().Where(x => x.Data["GFCError"].ToString() == "Unexpected error loading form: Id='" + id + "'");
        //    //mockSession.Verify();
        //}

        [Fact]
        public void Index_Post_Should_Return_Not_Found()
        {
            //arrange
            const string id = "123";
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            PageVM returnPage = null;
            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(id, false)).Returns(returnPage).Verifiable();
            var mockSettings = new Mock<IOptions<ApplicationSettings>>();
            var sut = new FormController(mockValidation.Object, mockSession.Object, mockSettings.Object, mockLogger.Object, mockPageHelper.Object) { ControllerContext = controllerContext };
            //act
            var result = sut.Index(new CurrentPageVM { PageId = id });
            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(564);
            mockSession.Verify();
        }

        [Fact]
        public void Index_Post_Should_Return_Errors()
        {
            const string id = "123";
            //arrange

            var returnPage = new PageVM { PageId = id, PreviousPages = new List<PreviousPageVM>() };
            var questions = new List<QuestionVM>
            {
                new QuestionVM {Validation = new ValidationVM {IsErrored = true, ErrorMessage = "blah blah"}},
                new QuestionVM {Validation = new ValidationVM {IsErrored = true, ErrorMessage = "blah blah"}}
            };
            returnPage.Questions = questions;

            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(id, false)).Returns(returnPage).Verifiable();
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "the service" }).Verifiable();

            ApplicationSettings appSettings = new ApplicationSettings() { 
                FormStartPage = id, 
                ServiceNotFoundPage = "test1", 
                DefaultBackLink = "test2",
                SiteTextStrings = textStrings
            };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            var mockUrlHelper = new Mock<IUrlHelper>();
            var sut = new FormController(mockValidation.Object, mockSession.Object, options, mockLogger.Object, mockPageHelper.Object);
            sut.Url = mockUrlHelper.Object;
            //act
            var result = sut.Index(new CurrentPageVM { PageId = id });

            //assert
            var viewResult = result as ViewResult;
            var model = viewResult.ViewData.Model as PageVM;
            model.PageId.Should().Be(id);
            model.Questions.Count(x => x.Validation.IsErrored).Should().Be(2);
            mockSession.Verify();
        }

        [Theory]
        [InlineData("CheckYourAnswers")]
        [InlineData(null)]
        public void Index_Post_Should_Redirect(string controllerName)
        {
            const string id = "123";
            //arrange

            var returnPage = new PageVM { PageId = id, NextPageId = controllerName, PreviousPages = new List<PreviousPageVM>() };

            var mockValidation = new Mock<IGdsValidation>();
            var mockSession = new Mock<ISessionService>();
            var mockLogger = new Mock<ILogger<FormController>>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockPageHelper.Setup(x => x.CheckPageHistory(returnPage, It.IsAny<string>(), false, mockSession.Object, It.IsAny<string>())).Returns(true);

            mockSession.Setup(x => x.GetPageById(id, false)).Returns(returnPage).Verifiable();
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "test service name" }).Verifiable();

            ApplicationSettings appSettings = new ApplicationSettings() 
            { 
                FormStartPage = id,
                ServiceNotFoundPage = "test1",
                DefaultBackLink = "test2",
                SiteTextStrings = textStrings
            };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            var mockUrlHelper = new Mock<IUrlHelper>();
            var sut = new FormController(mockValidation.Object, mockSession.Object, options, mockLogger.Object, mockPageHelper.Object);
            sut.Url = mockUrlHelper.Object;

            //act
            var result = sut.Index(new CurrentPageVM { PageId = id });

            //assert
            var redirectesult = result as RedirectToActionResult;
            redirectesult.ControllerName.Should().Be(controllerName);
            redirectesult.ActionName.Should().Be("Index");
            mockSession.Verify();
        }

    }
}
