using System;
using System.IO;
using System.IO.Compression;

namespace PDFProcessor.Helpers
{
    public static class ZipHelper
    {
        public static string Unzip(string zipFilePath)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            ZipFile.ExtractToDirectory(zipFilePath, tempDir);

            return tempDir;
        }
    }
}
