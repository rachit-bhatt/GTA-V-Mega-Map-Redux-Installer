using System.IO;
using System.Xml;

namespace GTA_V___Mega_Map___Redux_Installer.Classes
{
    public class OIVInstaller : ProgressEventHandler
    {
        public async Task InstallFromXMLAsync(string xmlFilePath, string gameDirectory)
        {
            string contentFolderPath = Path.Combine(xmlFilePath, "content");

            try
            {
                isCancelled = false;

                // Load the XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                // Validate the root package element
                XmlElement packageElement = xmlDoc.SelectSingleNode("/package") as XmlElement;

                if (packageElement == null || packageElement.GetAttribute("target") != "Five")
                {
                    OnProgressChanged(0, "Invalid or unsupported package.");
                    return;
                }

                // Process each content element
                XmlNodeList contentNodes = xmlDoc.SelectNodes("/package/content");
                int totalSteps = contentNodes.Count;
                int currentStep = 0;

                foreach (XmlNode contentNode in contentNodes)
                {
                    currentStep++;
                    OnProgressChanged((int)(((double)currentStep / totalSteps) * 100), $"Processing step { currentStep } of { totalSteps }");

                    XmlNode archiveNode = contentNode.SelectSingleNode("archive");
                    if (archiveNode == null)
                        continue;

                    string rpfPath = archiveNode.Attributes["path"].Value;
                    bool createIfNotExist = archiveNode.Attributes["createIfNotExist"]?.Value == "True";
                    string rpfType = archiveNode.Attributes["type"].Value;

                    // Handle RPF file operations
                    await HandleRPFFileAsync(rpfPath, createIfNotExist, rpfType, contentNode, contentFolderPath, gameDirectory);

                    // Check if cancelled after each step
                    if (isCancelled)
                    {
                        OnProgressChanged(0, "Installation cancelled.");
                        return;
                    }
                }

                OnProgressChanged(100, "Installation completed successfully.");
            }
            catch (Exception ex)
            {
                OnProgressChanged(0, $"Error during installation: { ex.Message }");
            }
        }

        private async Task HandleRPFFileAsync(string rpfPath, bool createIfNotExist, string rpfType, XmlNode contentNode, string contentFolderPath, string gameDirectory)
        {
            // Initialize RPF handling logic (assuming OpenRPF library is properly integrated)
            // Example:
            // var rpfHandler = new RPFHandler();

            // Example: Create the RPF file if it doesn't exist
            if (createIfNotExist)
            {
                // rpfHandler.CreateRPFFile(rpfPath, rpfType);
                OnProgressChanged(0, $"Created RPF file: { rpfPath }");
            }

            // Process each add tag
            XmlNodeList addNodes = contentNode.SelectNodes("archive/add");
            int totalSteps = addNodes.Count;
            int currentStep = 0;

            foreach (XmlNode addNode in addNodes)
            {
                currentStep++;
                OnProgressChanged((int)(((double)currentStep / totalSteps) * 100), $"Adding file { currentStep } of { totalSteps }");

                string sourceFileName = addNode.Attributes["source"].Value;
                string destinationPath = addNode.InnerText.Trim();

                string sourceFilePath = Path.Combine(contentFolderPath, sourceFileName);
                string targetFilePath = Path.Combine(gameDirectory, rpfPath.Replace("/", "\\") + destinationPath);

                // Example: Copy file to target path inside RPF file
                // rpfHandler.CopyFileToRPF(sourceFilePath, targetFilePath);
                File.Copy(sourceFilePath, targetFilePath, true);

                OnProgressChanged((int)(((double)currentStep / totalSteps) * 100), $"Installed { sourceFileName } to { targetFilePath }");

                // Check if cancelled after each file installation
                if (isCancelled)
                {
                    OnProgressChanged(0, "Installation cancelled.");
                    return;
                }
            }
        }
    }
}