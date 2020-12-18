using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Microsoft.VisualBasic.FileIO;

namespace BDOLanguageDataTool
{
    public static class LOC
    {
        private static MemoryStream Decompress(string file)
        {
            MemoryStream result = new MemoryStream();
            FileStream stream = File.OpenRead(file);
            stream.Seek(6, SeekOrigin.Begin);
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
            {
                deflateStream.CopyTo(result);
            }
            stream.Close();
            return result;
        }
        private static MemoryStream Compress(MemoryStream stream)
        {
            stream.Position = 0;
            byte[] input = stream.ToArray();
            byte[] size = BitConverter.GetBytes(Convert.ToUInt32(input.Length));
            Deflater compressor = new Deflater();
            compressor.SetLevel(Deflater.BEST_SPEED);
            compressor.SetInput(input);
            compressor.Finish();
            MemoryStream bos = new MemoryStream(input.Length);
            byte[] buf = new byte[1024];
            while (!compressor.IsFinished)
            {
                int count = compressor.Deflate(buf);
                bos.Write(buf, 0, count);
            }
            MemoryStream result = new MemoryStream();
            BinaryWriter writeBinary = new BinaryWriter(result);
            writeBinary.Write(size);
            writeBinary.Write(bos.ToArray());
            writeBinary.Close();
            return result;
        }
        public static void Decrypt(string file)
        {
            string directory = Path.GetDirectoryName(file);
            string filename = Path.GetFileNameWithoutExtension(file);

            Console.WriteLine($"Decrypting: {filename}");
            MemoryStream stream = Decompress(file);
            stream.Position = 0;
            string csv = Path.Combine(directory, $"{filename}.csv");
            using (var reader = new BinaryReader(stream))
            {
                using (var output = new StreamWriter(csv, false, Encoding.UTF8))
                {
                    output.WriteLine($"\"Type\",\"ID1\",\"ID2\",\"ID3\",\"ID4\",\"Text\"");
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        UInt32 strSize = reader.ReadUInt32();
                        UInt32 strType = reader.ReadUInt32();
                        UInt32 strID1 = reader.ReadUInt32();
                        UInt16 strID2 = reader.ReadUInt16();
                        byte strID3 = reader.ReadByte();
                        byte strID4 = reader.ReadByte();
                        string str = Encoding.Unicode.GetString(reader.ReadBytes(Convert.ToInt32(strSize * 2))).Replace("\n", "{LF}").Replace("\"", "\"\"");
                        reader.ReadBytes(4);
                        output.WriteLine($"\"{strType}\",\"{strID1}\",\"{strID2}\",\"{strID3}\",\"{strID4}\",\"{str}\"");
                    }
                }
            }
            Console.WriteLine($"Decrypted: {Path.GetFileName(csv)}");
            stream.Close();
        }
        public static void Encrypt(string file)
        {
            Console.WriteLine($"Encrypting: {Path.GetFileName(file)}");
            string loc = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.loc");
            MemoryStream stream = new MemoryStream();
            BinaryWriter writeBinary = new BinaryWriter(stream);
            using (TextFieldParser parser = new TextFieldParser(file))
            {
                parser.Delimiters = new string[] { "," };
                parser.TrimWhiteSpace = false;
                parser.ReadFields();
                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null) break;
                    byte[] strType = BitConverter.GetBytes(Convert.ToUInt32(parts[0]));
                    byte[] strID1 = BitConverter.GetBytes(Convert.ToUInt32(parts[1]));
                    byte[] strID2 = BitConverter.GetBytes(Convert.ToUInt16(parts[2]));
                    byte strID3 = Convert.ToByte(parts[3]);
                    byte strID4 = Convert.ToByte(parts[4]);
                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(parts[5].Replace("{LF}", "\n").Replace("\"\"", "\""));
                    byte[] utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
                    byte[] strSize = BitConverter.GetBytes(utf16Bytes.Length);
                    writeBinary.Write(strSize);
                    writeBinary.Write(strType);
                    writeBinary.Write(strID1);
                    writeBinary.Write(strID2);
                    writeBinary.Write(strID3);
                    writeBinary.Write(strID4);
                    writeBinary.Write(utf16Bytes);
                    writeBinary.Write(new byte[4]);
                }
            }
            MemoryStream result = Compress(stream);
            File.WriteAllBytes(loc, result.ToArray());
            Console.WriteLine($"Encrypted: {Path.GetFileName(loc)}");
        }
    }
}
