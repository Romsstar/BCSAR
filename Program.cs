using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (args.Length == 0 || args.Contains("--help"))
            {
                Console.WriteLine("Usage: inputFile outputDirectory>");
                Console.WriteLine("Description:");
                Console.WriteLine("Extracts WARC files from the specified BCSAR file and decodes them to WAV format.");
                Console.WriteLine("Parameters:");
                Console.WriteLine("<inputFile>    Path to the input BCSAR file.");
                Console.WriteLine("<outputDirectory>  Directory where extracted files will be saved.");
                Console.WriteLine("Example:");
                Console.WriteLine("BCSAR p10_sound.bcsar extracted");
                return;
            }

        
            try
            {
                if (args[0] == "-bcwav")
                {
                    // Decode single BCWAV
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Error: Missing parameters for -bcwav.");
                        Console.WriteLine("Usage: -bcwav <inputFile> <outputDirectory> [-p]");
                        return;
                    }
                    string bcwavPath = args[1];
                    string outputfolder = args[2];
                    string outputname = args[3];
                    bool printInfo = args.Contains("-p");

                    if (!File.Exists(bcwavPath))
                    {
                        Console.WriteLine($"Error: File not found - {bcwavPath}");
                        return;
                    }

                    byte[] fileData = File.ReadAllBytes(bcwavPath);
         
                    using (var memoryStream = new MemoryStream(fileData))
                    using (var br = new BinaryReader(memoryStream))
                    {
                        cwav cwavFile = new cwav(br); // Parse the CWAV file

                        if (printInfo)
                        {
                            Console.WriteLine("Channel Information:");
                            cwav.printChannelInfo(cwavFile); // Call the printChannelInfo method
                        }

                        WavDecoder.DecodeSingleBcwav(fileData,outputfolder, outputname);
                        Console.WriteLine($"Decoded {bcwavPath} to {outputname}");
                    }
                }
                else
                {
                    string bcsarPath = args[0];
                    string outputDir = args[1];

                    using (FileStream fs = new FileStream(bcsarPath, FileMode.Open))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        //Parse Information Tables
                        header headerData = new header(br); // Parse Header
                        strg strgData = new strg(br); // Parse STRG for filenames
                        info infoData = new info(br); // Parse INFO section

                        // Parse File Table
                        br.BaseStream.Seek(infoData.fileTable, SeekOrigin.Begin);
                        FileTable fileTableData = new FileTable(br, infoData.fileTable, header.FILE_pointer);

                        //Parse Warc
                        br.BaseStream.Seek(infoData.warcTable, SeekOrigin.Begin);
                        warc warcData = new warc(br, strgData, fileTableData);

                        // warcData.ExtractWARCs(br, strgData, fileTableData, outputDir);

                        var extractedFiles = warcData.ExtractWARIO(br, strgData, fileTableData, outputDir);

                        // Call BatchDecode for each WARC
                        foreach (var (files, bcwarName) in extractedFiles)
                        {
                            WavDecoder.BatchDecode(files, outputDir+"\\"+bcwarName, bcwarName);
                        }

                    }
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





