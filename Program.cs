using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCSAR.CSAR;
using BCSAR.INFO;
using BCSAR.STRG;
using BCWAV;
using Newtonsoft.Json;
using static warc;

namespace BCSAR
{
    class Program
    {


        static void Main(string[] args)
        {
            string bcsarPath = "p10_sound.bcsar"; // Input BCSAR file
            string outputDir = "extracted\\"; // Output directory

            try
            {
                using (FileStream fs = new FileStream(bcsarPath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    header headerData = new header(br); // Parse Header
                    strg strgData = new strg(br); // Parse STRG for filenames
                    info infoData = new info(br); // Parse INFO section

                    // Parse File Table
                    long fileTableOffset = infoData.FILETable + header.INFO_pointer+ 8; // Adjust by header
                    br.BaseStream.Seek(fileTableOffset, SeekOrigin.Begin);
                    FileTable fileTableData = new FileTable(br, fileTableOffset,header.FILE_pointer);

         

                    long warcTableOffset = infoData.warcTable + header.INFO_pointer + 8; // Adjust by header
                    br.BaseStream.Seek(warcTableOffset, SeekOrigin.Begin);
                    warc warcData = new warc(br, strgData, fileTableData, outputDir);
                    warcData.ExtractWARCs(br, strgData, fileTableData, outputDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadLine(); // Pause to view output
        }
    }
}




//wav.convert_to_wav();
//wav.DecodeToWav("output.wav");
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
//                    File.WriteAllText("fileid.json", JsonConvert.SerializeObject(warc.fileidList, Formatting.Indented));
//                }





//            }
//        }
//    }
//}






