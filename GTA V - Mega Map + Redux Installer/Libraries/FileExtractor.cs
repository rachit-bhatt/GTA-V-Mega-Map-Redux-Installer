using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace GTA_V___Mega_Map___Redux_Installer.Libraries
{
    abstract class FileExtractor
    {

        public static void ExtractFile(String rpfpath, String expath, int offset, int size, int compressedSize)
        {
            FileInfo fileInfo = new FileInfo(rpfpath);

            using FileStream originalFileStream = fileInfo.OpenRead();
            try
            {
                originalFileStream.Seek(offset, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Error");
            }

            using FileStream decompressedFileStream = System.IO.File.Create(expath);
            if (size == compressedSize)
            {
            }
            else
            {
                // originalFileStream.CopyTo(decompressedFileStream);
                using DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress);
                decompressionStream.CopyTo(decompressedFileStream);
            }
        }

    }
}