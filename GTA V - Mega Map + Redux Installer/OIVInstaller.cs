using System.IO;
using System.IO.Compression;
using System.Xml;
using OpenIV.Lib.Rpf;

namespace GTA_V___Mega_Map___Redux_Installer
{
    public class OIVInstaller
    {
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<int> ProgressChanged;

        public async Task InstallOIVPackageAsync(string oivFilePath, string gameDirectory)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            string tempExtractionPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                ZipFile.ExtractToDirectory(oivFilePath, tempExtractionPath);

                string assemblyXmlPath = Path.Combine(tempExtractionPath, "assembly.xml");

                if (!File.Exists(assemblyXmlPath))
                {
                    throw new FileNotFoundException("The 'assembly.xml' file is missing from the OIV package.");
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(assemblyXmlPath);
                XmlNodeList contentFiles = xmlDoc.GetElementsByTagName("archive");

                int totalFiles = contentFiles.Count;
                int processedFiles = 0;

                foreach (XmlNode archiveNode in contentFiles)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    string rpfPath = archiveNode.Attributes["path"].Value;

                    foreach (XmlNode fileNode in archiveNode.ChildNodes)
                    {
                        string relativePath = fileNode.Attributes["source"].Value;
                        string destinationPath = Path.Combine(tempExtractionPath, "content", relativePath.TrimStart('\\'));

                        await Task.Run(() => ModifyRPFFile(gameDirectory, rpfPath, relativePath, destinationPath));

                        processedFiles++;
                        ProgressChanged?.Invoke(this, (processedFiles * 100) / totalFiles);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(tempExtractionPath))
                {
                    Directory.Delete(tempExtractionPath, true);
                }
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void ModifyRPFFile(string gameDirectory, string rpfPath, string relativePath, string sourcePath)
        {
            string rpfFilePath = Path.Combine(gameDirectory, rpfPath);

            if (!File.Exists(rpfFilePath))
            {
                throw new FileNotFoundException($"The RPF file { rpfFilePath } does not exist.");
            }

            // Initialize the RPF file
            using (var rpf = new RpfFile(rpfFilePath, RpfFileMode.Update))
            {
                rpf.Open();

                // Read the file to be added
                byte[] fileData = File.ReadAllBytes(sourcePath);

                // Check if the relative path points to a directory within the RPF
                var parentDir = rpf.GetDirectory(Path.GetDirectoryName(relativePath)) ?? throw new DirectoryNotFoundException($"Directory { Path.GetDirectoryName(relativePath) } not found in RPF file { rpfFilePath }.");

                // Add or replace the file in the RPF
                var existingFile = parentDir.GetFile(Path.GetFileName(relativePath));

                if (existingFile != null)
                {
                    existingFile.Replace(fileData);
                }
                else
                {
                    parentDir.AddFile(Path.GetFileName(relativePath), fileData);
                }

                // Save changes to the RPF file
                rpf.Save();
            }
        }
    }
}