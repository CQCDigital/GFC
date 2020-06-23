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
using Microsoft.Extensions.Configuration;
using SYE.Helpers;
using SYE.Models.Response;

namespace SYE.Tests.Controllers
{
    public class FeedbackControllerTests
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
        //public void Page_Should_Return_Error_If_Last_Page_Not_Submission_Confirmation()
        //{
        //    const string id = "123";
        //    //arrange
        //    //Controller needs a controller context
        //    var httpContext = new DefaultHttpContext();
        //    var controllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext,
        //    };

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockFormService = new Mock<IFormService>();
        //    var mockGdsValidate = new Mock<IGdsValidation>();
        //    var mockConfiguration = new Mock<IConfiguration>();
        //    var mockNotificationService = new Mock<INotificationService>();
        //    var mockSessionService = new Mock<ISessionService>();

        //    mockSessionService.Setup(x => x.GetLastPage()).Returns("not-the-right-page").Verifiable();

        //    ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123" };
        //    IOptions<ApplicationSettings> options = Options.Create(appSettings);

        //    var sut = new FeedbackController(mockLogger.Object, mockFormService.Object, mockGdsValidate.Object, mockConfiguration.Object, mockNotificationService.Object, mockSessionService.Object);

        //    //act
        //    var result = sut.WhatDoYouThink("testReferer");

        //    //assert
        //    var statusResult = result as StatusResult;
        //    statusResult.StatusCode.Should().Be(590);
        //    mockSessionService.Verify();
        //}

        //[Fact]
        //public void Page_Should_Return_Error_If_Last_Page_Is_Null()
        //{
        //    const string id = "123";
        //    //arrange
        //    //Controller needs a controller context
        //    var httpContext = new DefaultHttpContext();
        //    var controllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext,
        //    };

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockFormService = new Mock<IFormService>();
        //    var mockGdsValidate = new Mock<IGdsValidation>();
        //    var mockConfiguration = new Mock<IConfiguration>();
        //    var mockNotificationService = new Mock<INotificationService>();
        //    var mockSessionService = new Mock<ISessionService>();

        //    mockSessionService.Setup(x => x.GetLastPage()).Returns((string)null).Verifiable();

        //    ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123" };
        //    IOptions<ApplicationSettings> options = Options.Create(appSettings);

        //    var sut = new FeedbackController(mockLogger.Object, mockFormService.Object, mockGdsValidate.Object, mockConfiguration.Object, mockNotificationService.Object, mockSessionService.Object);

        //    //act
        //    var result = sut.WhatDoYouThink("testReferer");

        //    //assert
        //    var statusResult = result as StatusResult;
        //    statusResult.StatusCode.Should().Be(590);
        //    mockSessionService.Verify();
        //}

        //[Fact]
        //public void Page_Should_Return_Error_If_PageViewModel_Is_Null()
        //{
        //    //arrange
        //    //Controller needs a controller context
        //    var httpContext = new DefaultHttpContext();
        //    var controllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext,
        //    };

        //    var mockLogger = new Mock<ILogger<FormController>>();
        //    var mockFormService = new Mock<IFormService>();
        //    var mockGdsValidate = new Mock<IGdsValidation>();
        //    var mockConfiguration = new Mock<IConfiguration>();
        //    var mockNotificationService = new Mock<INotificationService>();
        //    var mockSessionService = new Mock<ISessionService>();

        //    mockSessionService.Setup(x => x.GetLastPage()).Returns("you-have-sent-your-feedback").Verifiable(); 
            
        //    mockConfiguration.Setup(x => x.GetSection(It.IsAny<string>()).GetValue<string>(It.IsAny<string>()))
        //        .Returns((string) null).Verifiable();

        //    ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123" };
        //    IOptions<ApplicationSettings> options = Options.Create(appSettings);

        //    var sut = new FeedbackController(mockLogger.Object, mockFormService.Object, mockGdsValidate.Object, mockConfiguration.Object, mockNotificationService.Object, mockSessionService.Object);

        //    //act
        //    var result = sut.WhatDoYouThink("testReferer");

        //    //assert
        //    var statusResult = result as StatusResult;
        //    statusResult.StatusCode.Should().Be(591);
        //    mockSessionService.Verify();
        //}
    }
}