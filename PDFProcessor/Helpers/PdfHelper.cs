using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;

namespace PDFProcessor.Helpers
{
    public static class PdfHelper
    {
        public static void MergePdfFiles(List<string> pdfFiles, string outputFilePath)
        {
            using (var outputDocument = new PdfDocument())
            {
                foreach (var pdfFile in pdfFiles)
                {
                    using (var inputDocument = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import))
                    {
                        foreach (var page in inputDocument.Pages)
                        {
                            outputDocument.AddPage(page);
                        }
                    }
                }
                outputDocument.Save(outputFilePath);
            }
        }

        public static void RemoveBookmarks(string inputFilePath, string outputFilePath)
        {
            using (var inputDocument = PdfReader.Open(inputFilePath, PdfDocumentOpenMode.Modify))
            {
                inputDocument.Outlines.Clear();
                inputDocument.Save(outputFilePath);
            }
        }
    }
}
