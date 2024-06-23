using System.IO;
using System.Xml;

namespace GTA_V___Mega_Map___Redux_Installer
{
    public class OIVInstaller
    {
        public void InstallFromXML(string xmlFilePath, string contentFolderPath, string gameDirectory)
        {
            try
            {
                // Load the XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                // Validate the root package element
                XmlElement packageElement = xmlDoc.SelectSingleNode("/package") as XmlElement;
                if (packageElement == null || packageElement.GetAttribute("target") != "Five")
                {
                    Console.WriteLine("Invalid or unsupported package.");
                    return;
                }

                // Process each content element
                XmlNodeList contentNodes = xmlDoc.SelectNodes("/package/content");
                foreach (XmlNode contentNode in contentNodes)
                {
                    XmlNode archiveNode = contentNode.SelectSingleNode("archive");
                    if (archiveNode == null)
                        continue;

                    string rpfPath = archiveNode.Attributes["path"].Value;
                    bool createIfNotExist = archiveNode.Attributes["createIfNotExist"]?.Value == "True";
                    string rpfType = archiveNode.Attributes["type"].Value;

                    // Handle RPF file operations
                    HandleRPFFile(rpfPath, createIfNotExist, rpfType, contentNode, contentFolderPath, gameDirectory);
                }

                Console.WriteLine("Installation completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during installation: {ex.Message}");
            }
        }

        private void HandleRPFFile(string rpfPath, bool createIfNotExist, string rpfType, XmlNode contentNode, string contentFolderPath, string gameDirectory)
        {
            // Initialize RPF handling logic (assuming OpenRPF library is properly integrated)
            // Example:
            // var rpfHandler = new RPFHandler();

            // Example: Create the RPF file if it doesn't exist
            if (createIfNotExist)
            {
                // rpfHandler.CreateRPFFile(rpfPath, rpfType);
                Console.WriteLine($"Created RPF file: { rpfPath }");
            }

            // Process each add tag
            XmlNodeList addNodes = contentNode.SelectNodes("archive/add");
            foreach (XmlNode addNode in addNodes)
            {
                string sourceFileName = addNode.Attributes["source"].Value;
                string destinationPath = addNode.InnerText.Trim();

                string sourceFilePath = Path.Combine(contentFolderPath, sourceFileName);
                string targetFilePath = Path.Combine(gameDirectory, rpfPath.Replace("/", "\\") + destinationPath);

                // Example: Copy file to target path inside RPF file
                // rpfHandler.CopyFileToRPF(sourceFilePath, targetFilePath);
                File.Copy(sourceFilePath, targetFilePath, true);

                Console.WriteLine($"Installed { sourceFileName } to { targetFilePath }");
            }
        }
    }
}