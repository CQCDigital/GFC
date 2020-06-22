using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Models.SubmissionSchema;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace SYE.Services
{
    public interface IDocumentService
    {
        string CreateSubmissionDocument(string json);
        string CreateSubmissionDocument(SubmissionVM submissionVm);
    }

    [LifeTime(LifeTime.Scoped)]
    public class DocumentService : IDocumentService
    {
        private readonly IConfiguration _configuration;
        private readonly int FontSizeHeader = 50;
        private readonly int FontSizeNormal = 25;
        private readonly int FontSizeSmall = 15;

        public DocumentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreateSubmissionDocument(string json)
        {
            var submissionVm = JsonConvert.DeserializeObject<SubmissionVM>(json);
            return GenerateDocument(submissionVm);
        }

        public string CreateSubmissionDocument(SubmissionVM submissionVm)
        {
            return GenerateDocument(submissionVm);
        }

        private string GenerateDocument(SubmissionVM submissionVm)
        {
            string convertedDoc = null;
            using (MemoryStream documentStream = new MemoryStream())
            {
                // Create Document
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(documentStream, WordprocessingDocumentType.Document, true))
                {
                    //************************************
                    string notFoundId = "tell-us-which-service-01";
                    var contactIds = new List<string>
                    {
                        "your-contact-details-01",
                        "your-contact-details-02",
                        "your-contact-details-03"
                    };

                    if (_configuration != null)//TODO This is a work around. Fix config setup in tests!!
                    {
                        //get from appsettings
                        notFoundId = _configuration.GetSection("SubmissionDocument").GetValue<string>("NotFoundQuestionId");
                        contactIds = new List<string>
                        {
                            _configuration.GetSection("SubmissionDocument").GetValue<string>("ContactNameQuestionId"),
                            _configuration.GetSection("SubmissionDocument").GetValue<string>("ContactEmailQuestionId"),
                            _configuration.GetSection("SubmissionDocument").GetValue<string>("ContactTelephoneNumberQuestionId")
                        };

                    }
                    //************************************

                    // Add a main document part. 
                    var contactAnswers = submissionVm.Answers
                        .Where(x => contactIds.Contains(x.QuestionId))
                        .OrderBy(x => x.DocumentOrder).ToList();
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                    // Create the document structure and add some text.
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());
                    Paragraph para = body.AppendChild(new Paragraph());
                    //header
                    GetHeader(body, submissionVm);
                    //location
                    GetLocation(body, submissionVm, notFoundId);
                    foreach (var answer in submissionVm.Answers.Where(x => x.DocumentOrder > 1).OrderBy(x => x.DocumentOrder))
                    {
                        if(contactAnswers.Contains(answer))//skip these
                        {
                            if (answer.QuestionId.Equals(contactIds.FirstOrDefault()))//only once
                            {
                                GetContactDetails(body, contactAnswers);
                            }
                        }
                        else
                        {
                            GetAnswer(body, submissionVm, answer.QuestionId);
                        }                        
                    }

                    mainPart.Document.Save();

                    wordDocument.Close();
                }
                //convert to base64
                var documentBytes = documentStream.ToArray();
                convertedDoc = Convert.ToBase64String(documentBytes);
                documentStream.Close();
            }
            return convertedDoc;
        }

        #region Document Loading
        private void GetAnswer(Body body, SubmissionVM submissionVm, string questionId)
        {
            var answer = submissionVm.Answers.FirstOrDefault(x => x.QuestionId == questionId);
            if (answer != null)
            {
                GetDataSection(body, answer.Question, SplitOutNewLines(answer.Answer), true);
            }
        }

        private void GetHeader(Body body, SubmissionVM submissionVm)
        {
            GetDataSection(body, "Response Id :", new List<string> { submissionVm.Id }, false);
            GetDataSection(body, "Channel :", new List<string> { "GFC" }, false);
            GetDataSection(body, "GFC reference number :", new List<string> { submissionVm.SubmissionId }, false);
            GetDataSection(body, "Completed :", new List<string> { GetUkDateStringFromZulu(submissionVm.DateCreated.ToString()) }, false);
        }

        private string GetUkDateStringFromZulu(string dateCreated)
        {
            DateTime ukDate = DateTime.Parse(dateCreated);
            string formattedDate = ukDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return formattedDate + ": " + ukDate.ToShortTimeString();
        }

        /// <summary>
        /// Creates the feedback section
        /// </summary>
        /// <param name="body"></param>
        /// <param name="submissionVm"></param>
        /// <param name="notFoundId"></param>
        private void GetLocation(Body body, SubmissionVM submissionVm, string notFoundId)
        {
            var locationId = string.Empty;
            var providerId = string.Empty;
            var location = string.Empty;
            var locationDescription = string.Empty;
            var locationFound = false;
            var answer = submissionVm.Answers.FirstOrDefault(x => x.QuestionId == notFoundId);//1
            if (answer == null)
            {
                //location has been selected
                locationId = submissionVm.LocationId;
                providerId = submissionVm.ProviderId;
                location = submissionVm.LocationName;
                locationFound = true;
            }
            else
            {
                //location has not been found
                locationId = "none";
                providerId = "none";
                locationDescription = answer.Answer;
            }
            GetDataSection(body, "Location ID :", new List<string> { locationId }, false);
            GetDataSection(body, "Provider ID :", new List<string> { providerId }, false);            
            
            if (locationFound)
            {
                GetDataSection(body, "Location name : ", new List<string> { location }, false);
            }
            else
            {
                GetDataSection(body, "Location description : ", SplitOutNewLines(locationDescription), true);
            }
        }

        /// <summary>
        /// Creates the contact details section
        /// </summary>
        /// <param name="body"></param>
        /// <param name="submissionVm"></param>
        /// <param name="answers"></param>
        private void GetContactDetails(Body body, List<AnswerVM> answers)
        {
            if (answers != null && answers.Count > 0)
            {
                //build data list
                var data = new List<string>();
                foreach (var answer in answers)
                {
                    data.Add(answer.Question + ": " + answer.Answer);
                }

                GetDataSection(body, "Contact Details", data, true);
            }
        }

        private List<string> SplitOutNewLines(string answer)
        {
            //split out the text in case there is a new line
            var answerText = answer.Replace("\r\n", "\n");
            var answers = answerText.Split('\n').ToList();
            return answers;
        }

        #endregion

        #region Document Formatting
        /// <summary>
        /// Creates a line of text for the document
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="bold"></param>
        /// <returns></returns>
        private Run GetText(string text, int size, bool bold = false)
        {
            Run HighLightRun = new Run();
            RunProperties runPro = new RunProperties();
            RunFonts runFont = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" };
            FontSize fontSize = new FontSize() { Val = size.ToString() };

            Text runText = new Text() { Text = text };
            runPro.Append(runFont);
            if (bold)
            {
                runPro.Append(new Bold());
            }
            runPro.Append(fontSize);

            HighLightRun.Append(runPro);
            HighLightRun.Append(runText);
            return HighLightRun;
        }

        /// <summary>
        /// Creates a section consisting of a question with al list of answers
        /// </summary>
        /// <param name="body"></param>
        /// <param name="sideheader"></param>
        /// <param name="data"></param>
        /// <param name="dataOnNewLine"></param>
        /// <param name="indent"></param>
        private void GetDataSection(Body body, string sideheader, List<string> data, bool dataOnNewLine, bool indent = false)
        {
            var para = body.AppendChild(new Paragraph());
            //if (indent)//creates numbered list
            //{
            //    Indentation iUl = new Indentation() { LeftChars = 10, Hanging = "360" };  // correct indentation  

            //    NumberingProperties npUl = new NumberingProperties(
            //        new NumberingLevelReference() { Val = 2 },
            //        new NumberingId() { Val = 1 }
            //    );

            //    ParagraphProperties ppUnordered = new ParagraphProperties(npUl, iUl);
            //    para.ParagraphProperties = new ParagraphProperties(ppUnordered.OuterXml);
            //}
            var line = GetText(sideheader, FontSizeNormal, dataOnNewLine);
            para.AppendChild(line);
            var lineCounter = 0;

            foreach (var text in data.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                lineCounter++;
                if (dataOnNewLine)
                {
                    para = body.AppendChild(new Paragraph());
                }
                line = GetText(text, FontSizeNormal, !dataOnNewLine);
                if (dataOnNewLine)
                {
                    //para.Append(new Run(new TabChar()));//creates tab indent but not on wrapped line!!
                    para.Append(new Run());
                }
                para.AppendChild(line);
            }
            EmptyLine(body, para);
        }

        private void EmptyLine(Body body, Paragraph para)
        {
            para = body.AppendChild(new Paragraph());
            para.AppendChild(GetText(string.Empty, FontSizeSmall));
        }
        #endregion
    }
}
