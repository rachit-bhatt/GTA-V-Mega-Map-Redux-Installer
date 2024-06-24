using System.IO;
using System.Collections.Generic;

namespace GTA_V___Mega_Map___Redux_Installer.Classes
{
    public class RPFHeader
    {
        public int Magic { get; set; }
        public int EntryCount { get; set; }
        public int NamesLength { get; set; }

        public RPFHeader(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                Magic = reader.ReadInt32();
                EntryCount = reader.ReadInt32();
                NamesLength = reader.ReadInt32();
            }
        }
    }

    public class RPFEntry
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public int NameOffset { get; set; }
    }

    public class RPFToc
    {
        public List<RPFEntry> Entries { get; set; } = new List<RPFEntry>();

        public RPFToc(Stream stream, int entryCount)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                for (int i = 0; i < entryCount; i++)
                {
                    var entry = new RPFEntry
                    {
                        Offset = reader.ReadInt32(),
                        Size = reader.ReadInt32(),
                        NameOffset = reader.ReadInt32()
                    };
                    Entries.Add(entry);
                }
            }
        }
    }

    public class RPFExtractor
    {
        public static void ExtractFile(string rpfPath, string outputDir)
        {
            using (FileStream fs = new FileStream(rpfPath, FileMode.Open, FileAccess.Read))
            {
                RPFHeader header = new RPFHeader(fs);
                long tocPosition = fs.Position;

                RPFToc toc = new RPFToc(fs, header.EntryCount);

                foreach (var entry in toc.Entries)
                {
                    // Save the current position to return to the TOC
                    long currentPos = fs.Position;

                    // Seek to the file data offset
                    fs.Seek(entry.Offset, SeekOrigin.Begin);

                    // Read the file data
                    byte[] data = new byte[entry.Size];
                    fs.Read(data, 0, entry.Size);

                    // Write the extracted file to the output directory
                    string outputPath = Path.Combine(outputDir, entry.NameOffset.ToString());
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllBytes(outputPath, data);

                    // Return to the TOC position
                    fs.Seek(currentPos, SeekOrigin.Begin);
                }
            }
        }
    }
}