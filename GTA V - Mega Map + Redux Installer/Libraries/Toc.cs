using System;
using System.Collections.Generic;
using System.IO;
using rpf = GTA_V___Mega_Map___Redux_Installer.Libraries;

namespace GTA_V___Mega_Map___Redux_Installer.Libraries
{
    class Toc
    {
        private MemoryStream _namesStream;
        private List<rpf::FileSystemEntry> _fseList = new List<rpf::FileSystemEntry>();

        public List<rpf::FileSystemEntry> FileSystemEntriesList { get => _fseList; }


        public Toc(Stream toc, MemoryStream namesStream, int count)
        {
            // LoadNameSection(toc, count);
            _namesStream = namesStream;
            toc.Seek(0, SeekOrigin.Begin);


            byte[] currentEntry = new byte[16];

            for (int i = 0; i < count; i++)
            {
                toc.Read(currentEntry, 0, 16);
                MemoryStream ms = new MemoryStream(currentEntry);

                //filename offset
                byte[] temp = new byte[4];
                ms.Read(temp, 0, 3);

                int fnoffset = BitConverter.ToInt32(temp);
                _namesStream.Seek(fnoffset, SeekOrigin.Begin);
                String name = "common.rpf";

                byte[] currentLetter = new byte[1];
                do
                {
                    _namesStream.Read(currentLetter, 0, 1);
                    if (currentLetter[0] != 0)
                    {
                        name += System.Text.Encoding.UTF8.GetString(currentLetter);
                    }
                } while (currentLetter[0] != 0);

                //file or folder
                temp = new byte[4];
                ms.Read(temp, 0, 1);


                //is Directory?
                if (temp[0] == 128)
                {
                    //first filesystementry offset
                    temp = new byte[4];
                    ms.Read(temp, 0, 4);
                    int ffseoffset = BitConverter.ToInt32(temp);

                    //count
                    temp = new byte[4];
                    ms.Read(temp, 0, 4);
                    int fsecount = BitConverter.ToInt32(temp);

                    _fseList.Add(new rpf::Directory(name, ffseoffset, fsecount));

                }
                else
                {

                    //offset
                    temp = new byte[4];
                    ms.Read(temp, 0, 4);
                    int offset = BitConverter.ToInt32(temp);

                    //compressed size
                    temp = new byte[4];
                    ms.Read(temp, 0, 4);
                    int csize = BitConverter.ToInt32(temp);

                    //uncompressed size
                    temp = new byte[4];
                    ms.Read(temp, 0, 4);
                    int size = BitConverter.ToInt32(temp);

                    _fseList.Add(new rpf::File(name, offset, csize, size));
                }

                ms.Dispose();

            }
            toc.Close();





        }

        private void LoadNameSection(Stream toc, int count)
        {
            //cut filename section from stream and load it into the namesStream
            byte[] temp = new byte[toc.Length - (count * 16)];
            toc.Seek(count * 16, SeekOrigin.Begin);
            toc.Read(temp, 0, temp.Length);
            _namesStream = new MemoryStream(temp);
        }

    }
}
