using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SYE.Controllers;
using GDSHelpers.Models.FormSchema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using SYE.Models.Response;
using SYE.Services;
using NSubstitute;
using NSubstitute.Extensions;
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

                new KeyValuePair<string, string>("EmailNotification:FeedbackEmailExternal:EmailTemplateId", "mockExternalEmailTemplateId")
            }
            };
        }

        [Fact]
        public void ReportaProblemShouldReturn555StatusCode()
        {
            //arrange
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var mockService = new Mock<IServiceProvider>();
            //act
            var sut = new HelpController(mockService.Object);
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
            //Controller needs a controller context
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var mockService = new Mock<IServiceProvider>();
            //act
            var sut = new HelpController(mockService.Object);
            sut.ControllerContext = controllerContext;
            var response = sut.SubmitFeedback("urlReferer");
            //assert
            var result = response as StatusResult;
            result.StatusCode.Should().Be(556);
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

        //[Fact]
        //public async Task ReportaProblemSendsInternalEmails()
        //{
        //    //arrange
        //    var urlReferer = "urlReferer";
        //    
        //    var pageVM = new PageVM();
        //
        //    var mockNotificationService = new Mock<INotificationService>();
        //    mockNotificationService
        //        .Setup(x => x.NotifyByEmailAsync(It.IsAny<string>(),
        //            It.IsAny<string>(),
        //            It.IsAny<Dictionary<string, dynamic>>(),
        //            null,
        //            null))
        //        .Returns(Task.CompletedTask)
        //        .Verifiable();
        //
        //    var mockConfiguration = new ConfigurationBuilder()
        //        .Add(configData)
        //        .Build();
        //
        //    var mockServiceProvider = new Mock<IServiceProvider>();
        //    mockServiceProvider
        //        .Setup(x => x.GetService(typeof(INotificationService)))
        //        .Returns(mockNotificationService.Object);
        //    mockServiceProvider
        //        .Setup(x => x.GetService(typeof(IConfiguration)))
        //        .Returns(mockConfiguration);
        //
        //    //act
        //    var sut = new HelpController(mockServiceProvider.Object);
        //    await sut.SendEmailNotificationInternalAsync(pageVM, urlReferer);
        //
        //    //assert
        //    Mock.Verify();
        //}
    }
}
