using PDFProcessor.Helpers;
using System.Collections.Generic;

namespace PDFProcessor.Models
{
    public class PdfProcessor
    {
        public void MergePdfFiles(List<string> pdfFiles, string outputFilePath)
        {
            PdfHelper.MergePdfFiles(pdfFiles, outputFilePath);
        }

        public void RemoveBookmarks(string inputFilePath, string outputFilePath)
        {
            PdfHelper.RemoveBookmarks(inputFilePath, outputFilePath);
        }
    }
}
