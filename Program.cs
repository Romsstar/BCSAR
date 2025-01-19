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
            string outputDir = "extracted//"; // Output directory

            try
            {
                using (FileStream fs = new FileStream(bcsarPath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    header headerData = new header(br); // Parse Header
                    strg strgData = new strg(br); // Parse STRG for filenames
                    info infoData = new info(br); // Parse INFO section

                    // Parse File Table
                    long fileTableOffset = infoData.FILETable + header.INFO_pointer + 8; // Adjust by header
                    br.BaseStream.Seek(fileTableOffset, SeekOrigin.Begin);
                    FileTable fileTableData = new FileTable(br, fileTableOffset, header.FILE_pointer);
                    
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





