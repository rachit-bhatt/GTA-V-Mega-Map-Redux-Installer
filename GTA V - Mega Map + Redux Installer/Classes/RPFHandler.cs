using System.IO;
using RPF = GTA_V___Mega_Map___Redux_Installer.Libraries;

namespace GTA_V___Mega_Map___Redux_Installer.Classes
{
    public class RPFHandler
    {
        public async void ExtractRPFArchive(string rpfFilePath, string tempFolderPath)
        {
            try
            {
                // Ensure the output directory exists
                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                #region Load File in Memory

                FileStream rpfStream = File.Open(rpfFilePath, FileMode.Open);
                byte[] temp = new byte[20];
                rpfStream.Read(temp, 0, 20);
                rpfStream.Close();

                RPF.Header header = new RPF.Header(new MemoryStream(temp));

                rpfStream = File.Open(rpfFilePath, FileMode.Open);

                temp = new byte[header.GetTocSize()];
                rpfStream.Seek(2048, SeekOrigin.Begin);
                rpfStream.Read(temp, 0, header.GetTocSize());
                rpfStream.Close();

                RPF.Toc toc = new RPF.Toc(new MemoryStream(temp), new MemoryStream(temp), header.GetCount());

                #endregion

                // if (toc.FileSystemEntriesList[0].GetType() == typeof(RPF.Directory))
                {
                    #region Extract File to Temp Location

                    RPF.File file = toc.FileSystemEntriesList[0] as RPF.File;

                    RPF.FileExtractor.ExtractFile(rpfFilePath, Path.Combine(tempFolderPath, file.Name), file.Offset, file.Size, file.CompressedSize);

                    #endregion
                }

                // Open the RPF file
                // using (RPF.FileExtractor rpfFile = new RPF.FileExtractor.ExtractFile(rpfFilePath))
                // {
                //     // Iterate through all entries in the RPF file
                //     foreach (var entry in rpfFile.Entries)
                //     {
                //         // Check if entry is a file
                //         if (entry is RPF.FileExtractor fileEntry)
                //         {
                //             // Extract the file entry to the temporary folder
                //             string outputPath = Path.Combine(tempFolderPath, fileEntry.Name);
                //             using (FileStream outputStream = File.Create(outputPath))
                //             {
                //                 fileEntry.ExtractFile(outputStream);
                //             }
                //         }
                //     }
                // }

                // MessageBox.Show("Extraction completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting RPF archive: { ex.Message }", "Error");
            }
        }

        private void OpenRPFFile(string rpfFilePath)
        {
            FileStream rpfStream = File.Open(rpfFilePath, FileMode.Open);
            byte[] temp = new byte[20];
            rpfStream.Read(temp, 0, 20);
            rpfStream.Close();

            RPF.Header header = new RPF.Header(new MemoryStream(temp));

            rpfStream = File.Open(rpfFilePath, FileMode.Open);

            temp = new byte[header.GetTocSize()];
            rpfStream.Seek(2048, SeekOrigin.Begin);
            rpfStream.Read(temp, 0, header.GetTocSize());
            rpfStream.Close();

            RPF.Toc toc = new RPF.Toc(new MemoryStream(temp), new MemoryStream(temp), header.GetCount());

            if (toc.FileSystemEntriesList[0].GetType() == typeof(RPF.Directory))
            {
                MessageBox.Show("GG.");
            }
        }

        // public void CreateRPFArchive(string tempFolderPath, string outputRpfFilePath)
        // {
        //     try
        //     {
        //         // Create a new RPF file
        //         using (RPF.RpfFile rpfFile = new RPF.RpfFile())
        //         {
        //             // Iterate through files in temporary folder
        //             string[] files = Directory.GetFiles(tempFolderPath);
        //             foreach (var filePath in files)
        //             {
        //                 // Add each file to the RPF file
        //                 string fileName = Path.GetFileName(filePath);
        //                 using (FileStream fileStream = File.OpenRead(filePath))
        //                 {
        //                     RPF.RpfEntryFile entry = new RPF.RpfEntryFile(fileName, fileStream);
        //                     rpfFile.Entries.Add(entry);
        //                 }
        //             }

        //             // Save the RPF file to the output path
        //             rpfFile.Save(outputRpfFilePath);
        //         }

        //         MessageBox.Show("RPF creation completed successfully.");
        //     }
        //     catch (Exception ex)
        //     {
        //         MessageBox.Show($"Error creating RPF archive: {ex.Message}");
        //     }
        // }
    }
}