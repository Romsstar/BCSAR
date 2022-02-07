using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCSAR.CSAR;
using BCSAR.STRG;
using Newtonsoft.Json;

namespace BCSAR
{
    class Program
    {
        public static string dir = "extracted//";
        public static string input = "";

 
        static void Main(string[] args)
        {

            {
           
                if (args.Length == 0)
                {
                    Console.WriteLine("Input file:");
                    input = Console.ReadLine();
                }
                if (args.Length > 0)
                    input = args[0];
                dir += (Path.GetFileName(input)) + "\\";

                using (FileStream fs = new FileStream(input, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        header header = new header(br);
                        strg strg = new strg(br);
                        File.WriteAllText("header.json", JsonConvert.SerializeObject(header, Formatting.Indented));
                        File.WriteAllText("offsets+size.json", JsonConvert.SerializeObject(strg.tableRecordsList, Formatting.Indented));
                        File.WriteAllText("filenames.json", JsonConvert.SerializeObject(strg.stringEntries, Formatting.Indented));

                    }

                }
                Console.ReadLine();
            }
        }
    }
}     
    

