using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GDSHelpers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SYE.Controllers;
using SYE.Helpers;
using SYE.Models;
using SYE.Models.Response;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Controllers
{
    public class CheckYourAnswersControllerTests
    {
        [Fact]
        public void Index_Should_Return_570_Error()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
            var mockSession = new Mock<ISessionService>();
            var mockSubmissionService = new Mock<ISubmissionService>();
            var mockConfigurationService = new Mock<IConfiguration>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockDocumentService = new Mock<IDocumentService>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

            var sut = new CheckYourAnswersController(mockServiceProvider.Object);
            sut.ControllerContext = controllerContext;
            //act
            var result = sut.Index();

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(570);
        }

        [Fact]
        public void Index_Should_Return_571_Error()
        {
            //arrange
            FormVM formVm =new FormVM();
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
            var mockSession = new Mock<ISessionService>();
            var mockSubmissionService = new Mock<ISubmissionService>();
            var mockConfigurationService = new Mock<IConfiguration>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockDocumentService = new Mock<IDocumentService>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);

            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM {LocationName = null});
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

            var sut = new CheckYourAnswersController(mockServiceProvider.Object);
            sut.ControllerContext = controllerContext;
            //act
            var result = sut.Index();

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(571);
        }

        [Fact]
        public void Index_Should_Return_575_Error()
        {
            //arrange
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "CheckYourAnswers",
                        PreviousPages = new List<PreviousPageVM>
                        {
                            new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                            new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                            new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
                        }
                    }
                }
            };
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
            var mockSession = new Mock<ISessionService>();
            var mockSubmissionService = new Mock<ISubmissionService>();
            var mockConfigurationService = new Mock<IConfiguration>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockDocumentService = new Mock<IDocumentService>();
            var mockPageHelper = new Mock<IPageHelper>();

            mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);
            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "location" });
            mockSession.Setup(x => x.GetNavOrder()).Returns(new List<string>());
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

            var sut = new CheckYourAnswersController(mockServiceProvider.Object);
            sut.ControllerContext = controllerContext;
            //act
            var result = sut.Index();

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(575);
        }

        [Fact]
        public void Index_Should_Return_573_Error()
        {
            //arrange
            FormVM formVm = null;
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
            var mockSession = new Mock<ISessionService>();
            var mockSubmissionService = new Mock<ISubmissionService>();
            var mockConfigurationService = new Mock<IConfiguration>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockDocumentService = new Mock<IDocumentService>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);

            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

            var sut = new CheckYourAnswersController(mockServiceProvider.Object);
            sut.ControllerContext = controllerContext;
            //act
            var result = sut.Index(new CheckYourAnswersVm());

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(573);
        }

        [Fact]
        public void Index_Should_Return_574_Error()
        {
            //arrange
            FormVM formVm = new FormVM();
            //Task<int> reference = 123;
            int reference = 0;
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
            var mockSession = new Mock<ISessionService>();
            var mockSubmissionService = new Mock<ISubmissionService>();
            var mockConfigurationService = new Mock<IConfiguration>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockDocumentService = new Mock<IDocumentService>();
            var mockPageHelper = new Mock<IPageHelper>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);

            mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            //mockSubmissionService.Setup(x => x.GenerateSnowmakerUserRefAsync()).ReturnsAsync(reference);
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

            var sut = new CheckYourAnswersController(mockServiceProvider.Object);
            sut.ControllerContext = controllerContext;
            //act
            var result = sut.Index(new CheckYourAnswersVm());

            //assert
            var statusResult = result as StatusResult;
            statusResult.StatusCode.Should().Be(574);
        }

        //[Fact]
        //public void Index_Should_Return_500_Error()
        //{
        //    //arrange
        //    FormVM formVm = new FormVM();
        //    int reference = 123;
        //    //Controller needs a controller context
        //    var httpContext = new DefaultHttpContext();
        //    var controllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext,
        //    };
        //    var mockServiceProvider = new Mock<IServiceProvider>();
        //    var mockLogger = new Mock<ILogger<CheckYourAnswersController>>();
        //    var mockSession = new Mock<ISessionService>();
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockConfigurationService = new Mock<IConfiguration>();
        //    var mockNotificationService = new Mock<INotificationService>();
        //    var mockDocumentService = new Mock<IDocumentService>();
        //    var mockPageHelper = new Mock<IPageHelper>();
        //    mockServiceProvider.Setup(x => x.GetService(typeof(IPageHelper))).Returns(mockPageHelper.Object);

        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
        //    //mockSubmissionService.Setup(x => x.GenerateSnowmakerUserRefAsync()).ReturnsAsync(reference);
        //    //            mockSession.Setup(x => x.GetUserSession()).Returns(new UserSessionVM { LocationName = "location" });
        //    //            mockSession.Setup(x => x.GetNavOrder()).Returns(new List<string>());
        //    mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<CheckYourAnswersController>))).Returns(mockLogger.Object);
        //    mockServiceProvider.Setup(x => x.GetService(typeof(ISessionService))).Returns(mockSession.Object);
        //    mockServiceProvider.Setup(x => x.GetService(typeof(ISubmissionService))).Returns(mockSubmissionService.Object);
        //    mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfigurationService.Object);
        //    mockServiceProvider.Setup(x => x.GetService(typeof(INotificationService))).Returns(mockNotificationService.Object);
        //    mockServiceProvider.Setup(x => x.GetService(typeof(IDocumentService))).Returns(mockDocumentService.Object);

        //    var sut = new CheckYourAnswersController(mockServiceProvider.Object);
        //    sut.ControllerContext = controllerContext;
        //    // Act
        //    Action action = () => sut.Index(new CheckYourAnswersVm());
        //    // Assert
        //    action.Should().Throw<Exception>().Where(x => x.Data["GFCError"].ToString() == "Unexpected error submitting feedback!");
        //    mockSession.Verify();
        //}

        //[Fact]
        //public void Index_Should_Return_Data()
        //{
        //    const string id = "123";
        //    //arrange
        //    var returnForm = new FormVM{Id = id};
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(returnForm).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index();

        //    //assert
        //    var viewResult = result as ViewResult;
        //    var model = viewResult.ViewData.Model as CheckYourAnswersVm;
        //    model.FormVm.Id.Should().Be(id);
        //    mockSession.Verify();
        //}

        //[Fact]
        //public void Index_Should_Return_NotFound()
        //{
        //    //arrange
        //    FormVM returnForm = null;
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(returnForm).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index();

        //    //assert
        //    var statusResult = result as StatusCodeResult;
        //    statusResult.StatusCode.Should().Be(404);
        //    mockSession.Verify();
        //}

        //[Fact]
        //public void Index_Should_Return_Internal_Error()
        //{
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Throws(new Exception()).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index();

        //    //assert
        //    var statusResult = result as StatusCodeResult;
        //    statusResult.StatusCode.Should().Be(500);
        //    mockSession.Verify();
        //}

        //[Fact(Skip = "Failing. We may need to put GenerateSubmission into the submission service")]
        //public void Index_Post_Should_Return_Data()
        //{
        //    const string id = "123";
        //    //arrange
        //    var returnForm = new FormVM { Id = id };
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(returnForm).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index(new CheckYourAnswersVm());

        //    //assert
        //    var viewResult = result as ViewResult;
        //    var model = viewResult.ViewData.Model as CheckYourAnswersVm;
        //    model.FormVm.Id.Should().Be(id);
        //    mockSession.Verify();
        //}

        //[Fact]
        //public void Index_Post_Should_Return_Not_Found()
        //{
        //    const string id = "123";
        //    //arrange
        //    FormVM returnForm = null;
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Returns(returnForm).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index(new CheckYourAnswersVm());

        //    //assert
        //    var statusResult = result as StatusCodeResult;
        //    statusResult.StatusCode.Should().Be(404);
        //    mockSession.Verify();
        //}

        //[Fact]
        //public void Index_Post_Should_Return_Internal_Error()
        //{
        //    const string id = "123";
        //    //arrange
        //    FormVM returnForm = null;
        //    var mockSubmissionService = new Mock<ISubmissionService>();
        //    var mockSession = new Mock<ISessionService>();
        //    mockSession.Setup(x => x.GetFormVmFromSession()).Throws(new Exception()).Verifiable();
        //    var sut = new CheckYourAnswersController(mockSession.Object, mockSubmissionService.Object);

        //    //act
        //    var result = sut.Index(new CheckYourAnswersVm());

        //    //assert
        //    var statusResult = result as StatusCodeResult;
        //    statusResult.StatusCode.Should().Be(500);
        //    mockSession.Verify();
        //}

    }
}
