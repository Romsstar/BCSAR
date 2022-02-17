using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCSAR.CSAR;
using BCSAR.STRG;
using BCWAV;
using Newtonsoft.Json;

namespace BCSAR
{
    class Program
    {
        public static string dir = "extracted//";
        public static string input = "";
        public static string extension = "";

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
                wav.convert_to_wav();
                //extension = Path.GetExtension(input);
                //using (FileStream fs = new FileStream(input, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                //{
                //    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                //    {
                //        if (extension == ".bcwav")
                //        {                                        
                //                        wav.convert_to_wav(bw);
                //                }


                //                if (extension == ".bcsar")
                //                {
                //                    header header = new header(br);
                //                    strg strg = new strg(br);
                //                    warc warc = new warc(br);
                //                    File.WriteAllText("header.json", JsonConvert.SerializeObject(header, Formatting.Indented));
                //                    File.WriteAllText(Directory.CreateDirectory("STRG\\") + "offsets+size.json", JsonConvert.SerializeObject(strg.tableRecordsList, Formatting.Indented));
                //                    File.WriteAllText(Directory.CreateDirectory("STRG\\") + "filenames.json", JsonConvert.SerializeObject(strg.stringEntriesList, Formatting.Indented));
                //                    File.WriteAllText(Directory.CreateDirectory("STRG\\") + "lookupheader.json", JsonConvert.SerializeObject(strg.lookUpheaderList, Formatting.Indented));
                //                    File.WriteAllText(Directory.CreateDirectory("STRG\\") + "lookuptable.json", JsonConvert.SerializeObject(strg.lookUpList, Formatting.Indented));
                //                    File.WriteAllText(Directory.CreateDirectory("INFO\\") + "warctable.json", JsonConvert.SerializeObject(warc.warctableList, Formatting.Indented));
                //                    File.WriteAllText("fileid.json", JsonConvert.SerializeObject(warc.fileidList, Formatting.Indented));
                //                }
                                Console.ReadLine();


                            }
                        }
                    }
                }
//            }
//        }
//    }
//}






