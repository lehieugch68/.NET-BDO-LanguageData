using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace BDOLanguageDataTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "BDO LanguageData Tool by LeHieu";
            Console.WriteLine("Author: LeHieu\nGithub: http://github.com/lehieugch68 \n");
            string file = string.Join("", args);
            if (file != null)
            {
                string ext = Path.GetExtension(file);
                if (ext == ".loc")
                {
                    decrypt(decompress(file), file);
                    Console.WriteLine("Successful!");
                }
                else if (ext == ".tsv")
                {
                    compress(encrypt(file), file);
                    Console.WriteLine("Successful!");
                }
                else
                {
                    Console.WriteLine("Invalid file!");
                }
            }
            Console.ReadLine();
        }

        public static MemoryStream decompress(string file)
        {
            MemoryStream stream = new MemoryStream();
            using (var input = File.OpenRead(file))
            {
                input.Seek(6, SeekOrigin.Current);
                using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress, true))
                {
                    deflateStream.CopyTo(stream);
                }
            }
            return stream;
        }

        public static void compress(MemoryStream stream, string file)
        {
            string directory = Path.GetDirectoryName(file);
            string filename = Path.GetFileNameWithoutExtension(file);
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
            FileStream writeStream = new FileStream($"{directory}\\{filename}.loc", FileMode.Create);
            BinaryWriter writeBinary = new BinaryWriter(writeStream);
            writeBinary.Write(size);
            writeBinary.Write(bos.ToArray());
            writeBinary.Close();
        }

        public static void decrypt(MemoryStream stream, string file)
        {
            string directory = Path.GetDirectoryName(file);
            string filename = Path.GetFileNameWithoutExtension(file);
            stream.Position = 0;
            using (var reader = new BinaryReader(stream))
            {
                using (var output = new StreamWriter($"{directory}\\{filename}.tsv", false, Encoding.Unicode))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        UInt32 strSize = reader.ReadUInt32();
                        UInt32 strType = reader.ReadUInt32();
                        UInt32 strID1 = reader.ReadUInt32();
                        UInt16 strID2 = reader.ReadUInt16();
                        byte strID3 = reader.ReadByte();
                        byte strID4 = reader.ReadByte();
                        string str = Encoding.Unicode.GetString(reader.ReadBytes(Convert.ToInt32(strSize * 2))).Replace("\n", "<lf>");
                        reader.ReadBytes(4);
                        output.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", strType, strID1, strID2, strID3, strID4, str);
                    }
                }
            }
        }

        public static MemoryStream encrypt(string file)
        {
            MemoryStream stream = new MemoryStream();
            using (var reader = new StreamReader(file))
            {
                BinaryWriter writeBinary = new BinaryWriter(stream);
                byte[] zeroes = { (byte)0, (byte)0, (byte)0, (byte)0 };
                while (!reader.EndOfStream)
                {
                    string[] content = reader.ReadLine().Split(new string[] { "\t" }, StringSplitOptions.None);
                    byte[] strType = BitConverter.GetBytes(Convert.ToUInt32(content[0]));
                    byte[] strID1 = BitConverter.GetBytes(Convert.ToUInt32(content[1]));
                    byte[] strID2 = BitConverter.GetBytes(Convert.ToUInt16(content[2]));
                    byte strID3 = Convert.ToByte(content[3]);
                    byte strID4 = Convert.ToByte(content[4]);
                    string str = content[5].Replace("<lf>", "\n");
                    byte[] strBytes = Encoding.Unicode.GetBytes(str);
                    byte[] strSize = BitConverter.GetBytes(str.Length);
                    writeBinary.Write(strSize);
                    writeBinary.Write(strType);
                    writeBinary.Write(strID1);
                    writeBinary.Write(strID2);
                    writeBinary.Write(strID3);
                    writeBinary.Write(strID4);
                    writeBinary.Write(strBytes);
                    writeBinary.Write(zeroes);
                }
            }
            return stream;
        }
    }
}
