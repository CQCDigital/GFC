using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using GDSHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SYE.Controllers;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using SYE.Models.Response;
using SYE.Services;
using NSubstitute;
using NSubstitute.Extensions;
using SYE.Helpers;
using SYE.Models;
using Xunit;

namespace SYE.Tests.Controllers
{
    public class HelpControllerTests
    {
        private MemoryConfigurationSource configData;

        public HelpControllerTests()
        {
            configData = new MemoryConfigurationSource
            {
                InitialData = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:Phase", "mockPhase"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:EmailTemplateId", "mockInternalEmailTemplateId"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:ServiceSupportEmailAddress", "mockServiceSupportEmail"),
                
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:0:Name", "message"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:0:TemplateField", "feedback-message"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:0:FormField", "message"),

                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:1:Name", "name"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:1:TemplateField", "feedback-full-name"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:1:FormField", "full-name"),

                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:2:Name", "email"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:2:TemplateField", "feedback-email-address"),
                new KeyValuePair<string, string>("EmailNotification:FeedbackEmail:FieldMappings:2:FormField", "email-address"),

                new KeyValuePair<string, string>("EmailNotification:FeedbackEmailExternal:EmailTemplateId", "mockExternalEmailTemplateId"),

                new KeyValuePair<string, string>("FormsConfiguration:ServiceFeedbackForm:Name", "form1"),
                new KeyValuePair<string, string>("FormsConfiguration:ServiceFeedbackForm:Version", "version1"),
                new KeyValuePair<string, string>("ApplicationSettings:GFCUrls:StartPage", "start-page"),
                new KeyValuePair<string, string>("ApplicationSettings:GFCUrls:ConfirmationPage", "confirmation-page")
                }
            };
        }

        [Fact]
        public void ReportaProblemShouldReturn555StatusCode()
        {
            //arrange
            var fakeConfiguration = new ConfigurationBuilder()
                .Add(configData)
                .Build();
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var formVm = new FormVM{Pages = new List<PageVM>()};
            var mockLogger = new Mock<ILogger<HelpController>>();
            var mockFormService = new Mock<IFormService>();
            mockFormService.Setup(x => x.FindByNameAndVersion("form1", "version1")).ReturnsAsync(formVm);
            var mockGdsValidation = new Mock<IGdsValidation>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockActionService = new Mock<IActionService>();

            //act
            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, fakeConfiguration, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.Feedback("urlReferer");
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(555);
        }

        [Fact]
        public void ReportaProblemsubmitShouldReturn556StatusCode()
        {
            //arrange
            var fakeConfiguration = new ConfigurationBuilder()
                .Add(configData)
                .Build();

            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var formVm = new FormVM { Pages = new List<PageVM>() };
            var mockLogger = new Mock<ILogger<HelpController>>();
            var mockFormService = new Mock<IFormService>();
            mockFormService.Setup(x => x.FindByNameAndVersion("form1", "version1")).ReturnsAsync(formVm);
            var mockGdsValidation = new Mock<IGdsValidation>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockActionService = new Mock<IActionService>();

            //act
            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, fakeConfiguration, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SubmitFeedback("urlReferer");
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(556);
        }
        [Fact]
        public void ReportaProblemsubmitShouldCreateAction()
        {
            //arrange
            var fakeConfiguration = new ConfigurationBuilder()
                .Add(configData)
                .Build();

            var formData = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "test", "value" } });

            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(x => x.Form).Returns(formData);

            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(x => x.Request).Returns(requestMock.Object);

            //Controller needs a controller context
            var controllerContext = new ControllerContext()
            {
                HttpContext = contextMock.Object//httpContext,
            };

            var formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        Questions = new List<QuestionVM>
                        {
                            new QuestionVM {QuestionId = "email-address"}
                        }
                    }
                }
            };
            var pageVm = new PageVM();
            var mockLogger = new Mock<ILogger<HelpController>>();
            var mockFormService = new Mock<IFormService>();
            mockFormService.Setup(x => x.FindByNameAndVersion("form1", "version1")).ReturnsAsync(formVm);
            mockFormService.Setup(x => x.GetLatestFormByName("form1")).ReturnsAsync(formVm);
            var mockGdsValidation = new Mock<IGdsValidation>();

            mockGdsValidation.Setup(x => x.ValidatePage(It.IsAny<PageVM>(), It.IsAny<IFormCollection>(), It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<HashSet<char>>())).Returns(pageVm);
            var mockNotificationService = new Mock<INotificationService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockActionService = new Mock<IActionService>();
            mockActionService.Setup(x => x.CreateAsync(It.IsAny<UserActionVM>())).Verifiable();

            //act
            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, fakeConfiguration, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SubmitFeedback("urlReferer");
            //assert
            mockActionService.Verify();
        }
        [Theory]
        [InlineData("confirmation-page")]
        [InlineData("")]
        [InlineData(null)]
        public void FeedbackThankYouShouldRedirectToStartPage(string urlReferer)
        {
            //arrange
            var fakeConfiguration = new ConfigurationBuilder()
                .Add(configData)
                .Build();

            var requestMock = new Mock<HttpRequest>();
            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(x => x.Request).Returns(requestMock.Object);

            //Controller needs a controller context
            var controllerContext = new ControllerContext()
            {
                HttpContext = contextMock.Object//httpContext,
            };

            var mockLogger = new Mock<ILogger<HelpController>>();
            var mockFormService = new Mock<IFormService>();
            var mockGdsValidation = new Mock<IGdsValidation>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockActionService = new Mock<IActionService>();

            //act
            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, fakeConfiguration, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.FeedbackThankYou(urlReferer);
            //assert
            var result = response as ViewResult;
            var redirectVm = result?.Model as RedirectVM;
            redirectVm.Url.Should().Be("start-page");
        }

        [Theory]
        [InlineData("any-other-page")]
        [InlineData("junk")]
        [InlineData("eworiweoriweroiu")]
        [InlineData("qweqwe qweqwe")]
        [InlineData("qwe/qwe/qwe/qwe/qwe")]
        public void FeedbackThankYouShouldNotRedirectToStartWhenReturnUrlIsConfirmation(string urlReferer)
        {
            //arrange
            var fakeConfiguration = new ConfigurationBuilder()
                .Add(configData)
                .Build();

            var requestMock = new Mock<HttpRequest>();
            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(x => x.Request).Returns(requestMock.Object);

            //Controller needs a controller context
            var controllerContext = new ControllerContext()
            {
                HttpContext = contextMock.Object//httpContext,
            };

            var mockLogger = new Mock<ILogger<HelpController>>();
            var mockFormService = new Mock<IFormService>();
            var mockGdsValidation = new Mock<IGdsValidation>();
            var mockNotificationService = new Mock<INotificationService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockActionService = new Mock<IActionService>();

            //act
            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, fakeConfiguration, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.FeedbackThankYou(urlReferer);
            //assert
            var result = response as ViewResult;
            var redirectVm = result?.Model as RedirectVM;
            redirectVm.Url.Should().Be(urlReferer);
        }

        //[Fact]
        //public async Task ReportaProblemSendsExternalEmails()
        //{
        //    //arrange
        //    var externalRecipient = "external@recipient.com";

        //    var mockNotificationService = new Mock<INotificationService>();
        //    mockNotificationService
        //        .Setup(x => x.NotifyByEmailAsync(It.IsAny<string>(),
        //            It.IsAny<string>(),
        //            null,
        //            null,
        //            null))
        //        .Returns(Task.CompletedTask)
        //        .Verifiable();

        //    var mockConfiguration = new ConfigurationBuilder()
        //        .Add(configData)
        //        .Build();

        //    var mockServiceProvider = new Mock<IServiceProvider>();
        //    mockServiceProvider
        //        .Setup(x => x.GetService(typeof(INotificationService)))
        //        .Returns(mockNotificationService.Object);
        //    mockServiceProvider
        //        .Setup(x => x.GetService(typeof(IConfiguration)))
        //        .Returns(mockConfiguration);

        //    //act
        //    var sut = new HelpController(mockServiceProvider.Object);
        //    await sut.SendEmailNotificationExternalAsync(externalRecipient);

        //    //assert
        //    Mock.Verify();

        //}

        //        [Fact]
        //        public async Task ReportaProblemSendsInternalEmails()
        //        {
        //            //arrange
        //            var urlReferer = "urlReferer";

        //            var pageVM = new PageVM();

        //            var mockNotificationService = new Mock<INotificationService>();
        //            mockNotificationService
        //                .Setup(x => x.NotifyByEmailAsync(It.IsAny<string>(),
        //                    It.IsAny<string>(),
        //                    It.IsAny<Dictionary<string, dynamic>>(),
        //                    null,
        //                    null))
        //                .Returns(Task.CompletedTask)
        //                .Verifiable();

        //            var mockConfiguration = new ConfigurationBuilder()
        //                .Add(configData)
        //                .Build();

        //            var mockServiceProvider = new Mock<IServiceProvider>();
        //            mockServiceProvider
        //                .Setup(x => x.GetService(typeof(INotificationService)))
        //                .Returns(mockNotificationService.Object);
        //            mockServiceProvider
        //                .Setup(x => x.GetService(typeof(IConfiguration)))
        //                .Returns(mockConfiguration);

        //            var mockLogger = new Mock<ILogger<HelpController>>();
        //            var mockFormService = new Mock<IFormService>();
        //            var mockGdsValidation = new Mock<IGdsValidation>();
        //            var mockConfig = new Mock<IConfiguration>();
        ////            var mockNotificationService = new Mock<INotificationService>();
        //            var mockSessionService = new Mock<ISessionService>();
        //            var mockActionService = new Mock<IActionService>();

        //            //act
        //            var sut = new HelpController(mockLogger.Object, mockFormService.Object, mockGdsValidation.Object, mockConfig.Object, mockNotificationService.Object, mockSessionService.Object, mockActionService.Object);
        //            await sut.SendEmailNotificationInternalAsync(pageVM, urlReferer);

        //            //assert
        //            Mock.Verify();
        //        }
    }
}
