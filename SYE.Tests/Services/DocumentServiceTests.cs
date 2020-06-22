using System;
using System.IO;
using FluentAssertions;
using SYE.Tests.TestHelpers;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Services
{
    public class DocumentServiceTests
    {
        private string _dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Resources\\";
        private string _fileNameNoContactNewDateFormat = "submission-schema-no-contact-new-date-format.json";
        private string _fileNameNoContact = "submission-schema-no-contact.json";
        private string _fileNameContactDetails = "submission-schema-contact-details.json";
        private string _fileNameContactDetailsNoLocation = "submission-schema-contact-details-no-location.json";
        private string _fileNameNoContactDetailsNoLocation = "submission-schema-no-contact-details-no-location.json";
        private string _fileNameNoEmail = "submission-schema-no-email.json";

        public DocumentServiceTests()
        {
            FileHelper.DeleteFilesWithExtension(_dir, "docx");//remove any residual files
        }
        [Fact]
        public void CreateDocumentNoContactDetailsNewDateFormatTest()
        {
            var path = _dir + "NoContactDetailsNewDateFormat.docx";

            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameNoContactNewDateFormat);

            var base64Documentresult = sut.CreateSubmissionDocument(json);
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();

        }

        [Fact]
        public void CreateDocumentNoContactDetailsTest()
        {     
            var path = _dir + "NoContactDetails.docx";

            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameNoContact);

            var base64Documentresult = sut.CreateSubmissionDocument(json);
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();

        }
        [Fact]
        public void CreateDocumentWithContactDetailsTest()
        {
            var path = _dir + "ContactDetails.docx";
            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameContactDetails);
            //act
            var base64Documentresult = sut.CreateSubmissionDocument(json);
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();
        }
        [Fact]
        public void CreateDocumentWithContactDetailsNoLocationTest()
        {
            var path = _dir + "ContactDetailsNoLocation.docx";
            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameContactDetailsNoLocation);
            //act
            var base64Documentresult = sut.CreateSubmissionDocument(json);
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();
        }
        [Fact]
        public void CreateDocumentWithNoContactDetailsNoLocationTest()
        {
            var path = _dir + "NoContactDetailsNoLocation.docx";
            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameNoContactDetailsNoLocation);
            //act
            var base64Documentresult = sut.CreateSubmissionDocument(json);
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();
        }
        [Fact]
        public void CreateDocumentWithNoEmailTest()
        {
            var path = _dir + "ContactDetailsNoEmail.docx";
            var sut = new DocumentService(null);
            var json = GetJsonString(_fileNameNoEmail);
            //act
            var base64Documentresult = sut.CreateSubmissionDocument(json);
            //assert
            base64Documentresult.Should().NotBeNullOrWhiteSpace();
            FileHelper.GenerateWordDocument(base64Documentresult, path);
            FileHelper.FileExists(path).Should().BeTrue();
        }

        /// <summary>
        /// this method reads a json file from the folder and returns the next page
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="path"></param>
        /// <param name="locationName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Please refactor this function (and all tests consuming this method) so method accepts whole form schema and returns required page.
        /// If we need to load form from database/cache/session/file-system it has to be done as a seperate function
        /// </remarks>   
        private string GetJsonString(string fileName)
        {
            var file = string.Empty;
            var path = _dir + fileName;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }

            using (var r = new StreamReader(path))
            {
                file = r.ReadToEnd();
            }

            return file;
        }
    }
}
