using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Core
{
    public static class ModMetadataReader
    {
        public static byte[] StrByteBuffer = new byte[4096];

        public static Metadata LoadMetadata(string filename)
        {
            Metadata header = (Metadata)null;
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException ex)
            {
                return (Metadata)null;
            }
            BinaryReader file = new BinaryReader((Stream)fileStream);
            try
            {
                if (file.ReadInt32() > 15)
                    header = MountMetadata(file);
            }
            catch (EndOfStreamException ex)
            {
            }
            file.Close();
            fileStream.Close();
            return header;
        }

        public static Metadata MountMetadata(BinaryReader file)
        {
            Metadata header = new Metadata();
            header.Version = file.ReadInt32();
            header.Author = Read(file);
            header.Description = Read(file);
            header.Dependencies = new List<string>((IEnumerable<string>)Read(file).Split(',').Where(m => !Constants.BaseMods.Contains(m.ToLower())));
            header.Referenced = new List<string>((IEnumerable<string>)Read(file).Split(',').Where(m => !Constants.BaseMods.Contains(m.ToLower())));
            if (header.Dependencies.Count == 1 && header.Dependencies[0] == "")
                header.Dependencies.Clear();
            if (header.Referenced.Count == 1 && header.Referenced[0] == "")
                header.Referenced.Clear();
            return header;
        }

        public static string Read(BinaryReader file)
        {
            int count = file.ReadInt32();
            if (count <= 0)
                return string.Empty;
            if (count > StrByteBuffer.Length)
                Array.Resize<byte>(ref StrByteBuffer, StrByteBuffer.Length * 2);
            file.Read(StrByteBuffer, 0, count);
            return Encoding.UTF8.GetString(StrByteBuffer, 0, count);
        }
    }
}