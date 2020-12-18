using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BDOLanguageDataTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "BDO LanguageData Tool by LeHieu";
            foreach (string file in args)
            {
                string ext = Path.GetExtension(file).ToLower();
                switch (ext)
                {
                    case ".loc":
                        LOC.Decrypt(file);
                        break;
                    case ".csv":
                        LOC.Encrypt(file);
                        break;
                    default:
                        break;
                }
            }
            Console.ReadLine();
        }
    }
}
