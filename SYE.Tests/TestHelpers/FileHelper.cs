using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SYE.Tests.TestHelpers
{
    public static class FileHelper
    {
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool DeleteFilesWithExtension(string folder, string extension)
        {
            try
            {
                string[] fileNames = Directory.GetFiles(folder);
                foreach (string filePath in fileNames)
                {
                    if (filePath.Contains(extension))
                    {
                        File.Delete(filePath);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                //log error
                return false;
            }
        }

        public static void GenerateWordDocument(string base64WordDocument, string fullPath)
        {
            LoadContent(Convert.FromBase64String(base64WordDocument), fullPath);
        }
        private static void LoadContent(byte[] content, string fullPath)
        {
            Stream documentStream = new MemoryStream();
            documentStream.Write(content, 0, content.Length);

            using (WordprocessingDocument document = WordprocessingDocument.Open(documentStream, true))
            {
                MainDocumentPart mainPart = document.MainDocumentPart;
                mainPart.Document.Save();
                document.SaveAs(fullPath);
            }
        }
    }
}