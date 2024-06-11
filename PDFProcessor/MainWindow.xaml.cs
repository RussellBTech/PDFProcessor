using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PDFProcessor.Helpers;
using PDFProcessor.Models;
using System.Collections.Generic;

namespace PDFProcessor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private async void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FolderPathTextBox.Text;

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("Please enter a valid folder path.");
                return;
            }

            try
            {
                await Task.Run(() => ProcessFolder(folderPath));
                Console.WriteLine("Processing completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ProcessFolder(string folderPath)
        {
            var outputFolder = Path.Combine(folderPath, "MergedPDFs");
            Directory.CreateDirectory(outputFolder);

            var pdfProcessor = new PdfProcessor();
            var nonPdfFiles = new List<string>();
            var overwrittenPdfFiles = new List<string>();

            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                var dirName = new DirectoryInfo(dir).Name;
                var outputFilePath = Path.Combine(outputFolder, $"{dirName}.pdf");

                Console.WriteLine($"Processing folder: {dirName}");

                if (!dirName.Contains("MergedPDFs"))
                {
                    try
                    {
                        ProcessDirectory(dir, pdfProcessor, outputFilePath, nonPdfFiles, overwrittenPdfFiles);
                        Console.WriteLine($"Merged PDFs for folder: {dirName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process folder {dirName}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Skipping Output Folder");
                }
            }

            foreach (var zipFilePath in Directory.GetFiles(folderPath, "*.zip"))
            {
                var zipFileName = Path.GetFileNameWithoutExtension(zipFilePath);
                var outputFilePath = Path.Combine(outputFolder, $"{zipFileName}.pdf");

                Console.WriteLine($"Processing zip file: {zipFileName}");

                try
                {
                    var tempDir = ZipHelper.Unzip(zipFilePath);
                    ProcessDirectory(tempDir, pdfProcessor, outputFilePath, nonPdfFiles, overwrittenPdfFiles);
                    Directory.Delete(tempDir, true);
                    Console.WriteLine($"Merged PDFs for zip file: {zipFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process zip file {zipFileName}: {ex.Message}");
                }
            }

            foreach (var filePath in Directory.GetFiles(folderPath))
            {
                var fileName = Path.GetFileName(filePath);
                var outputFilePath = Path.Combine(outputFolder, fileName);

                if (Path.GetExtension(filePath).ToLower() == ".pdf")
                {
                    if (File.Exists(outputFilePath))
                    {
                        overwrittenPdfFiles.Add(fileName);
                    }

                    Console.WriteLine($"Processing individual PDF: {fileName}");

                    try
                    {
                        pdfProcessor.RemoveBookmarks(filePath, outputFilePath);
                        Console.WriteLine($"Processed individual PDF: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process PDF {fileName}: {ex.Message}");
                    }
                }
                else
                {
                    var tempExtension = Path.GetExtension(filePath).ToLower();
                    if (tempExtension != ".zip" && !tempExtension.Contains("zip") && !filePath.Contains(".zip"))
                    {
                        nonPdfFiles.Add(fileName);
                    }
                }
            }

            LogNonPdfFiles(nonPdfFiles, outputFolder);
            LogOverwrittenPdfFiles(overwrittenPdfFiles, outputFolder);
        }

        private void ProcessDirectory(string directoryPath, PdfProcessor pdfProcessor, string outputFilePath, List<string> nonPdfFiles, List<string> overwrittenPdfFiles)
        {
            var pdfFiles = new List<string>();

            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                var fileName = Path.GetFileName(filePath);
                if (Path.GetExtension(filePath).ToLower() == ".pdf")
                {
                    pdfFiles.Add(filePath);
                }
                else if (!filePath.Contains(".zip"))
                {
                    nonPdfFiles.Add(fileName);
                }
            }

            if (pdfFiles.Count > 0)
            {
                if (File.Exists(outputFilePath))
                {
                    overwrittenPdfFiles.Add(Path.GetFileName(outputFilePath));
                }
                pdfProcessor.MergePdfFiles(pdfFiles, outputFilePath);
            }

            foreach (var subDir in Directory.GetDirectories(directoryPath))
            {
                var subDirName = new DirectoryInfo(subDir).Name;
                var subDirOutputFilePath = Path.Combine(Path.GetDirectoryName(outputFilePath), $"{subDirName}.pdf");

                ProcessDirectory(subDir, pdfProcessor, subDirOutputFilePath, nonPdfFiles, overwrittenPdfFiles);
            }
        }

        private void LogNonPdfFiles(List<string> nonPdfFiles, string outputFolder)
        {
            var nonPdfFilePath = Path.Combine(outputFolder, "NonPdfFiles.txt");
            File.WriteAllLines(nonPdfFilePath, nonPdfFiles);
        }

        private void LogOverwrittenPdfFiles(List<string> overwrittenPdfFiles, string outputFolder)
        {
            var overwrittenPdfFilePath = Path.Combine(outputFolder, "OverwrittenPdfFiles.txt");
            File.WriteAllLines(overwrittenPdfFilePath, overwrittenPdfFiles);
        }
    }
}
