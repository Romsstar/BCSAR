using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BCSAR.CSAR;
using BCSAR.STRG;
using BCWAV;
using Newtonsoft.Json;

public class warc
{
    public int warcnum;

    public List<warctable> warctableList = new List<warctable>();
    public List<filetable> filetableList = new List<filetable>();
    public List<TableEntry> Entries { get; private set; }

    public struct warctable
    {
        public int id;
        public int offset;
    }

    public struct filetable
    {
        public int fileid;
        public int unk1;
        public int unk2;
        public int nameid;
    }

    public class TableEntry
    {
        public string Type { get; set; } // The type of the entry (e.g., "BCWAR")
        public int FileID { get; set; } // The file ID
        public string Name { get; set; } // The name of the entry
        public int NameID { get; set; } // The ID for the name (used for STRG lookup)
        public long Offset { get; set; } // The offset of the entry in the file
    }

    public warc(BinaryReader br, strg strgData, FileTable fileTable, string outputDirectory)
    {
        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);

        long baseOffset = br.BaseStream.Position; // Store Base Offset for correct offset calculation

        // Read the number of WARC entries
        warcnum = br.ReadInt32();

        // Track current position in the WARC table
        long currentTableOffset = baseOffset + 4; // +4 to account for the warcnum already read

        for (int i = 0; i < warcnum; i++)
        {
            // Seek to the current WARC table entry
            br.BaseStream.Seek(currentTableOffset, SeekOrigin.Begin);

            // Parse WARC table entry
            warctable entry = new warctable
            {
                id = br.ReadInt32(),
                offset = br.ReadInt32() // Relative offset
            };
            warctableList.Add(entry);

            // Calculate absolute offset for the entry's data
            long absoluteOffset = baseOffset + entry.offset;

            // Seek to the entry's data location
            br.BaseStream.Seek(absoluteOffset, SeekOrigin.Begin);

            // Read the file ID and other data at the entry's data location
            filetable fileentry = new filetable
            {
                fileid = br.ReadInt32(),
                unk1 = br.ReadInt32(),
                unk2 = br.ReadInt32(),
                nameid = br.ReadInt32(),
            };
            filetableList.Add(fileentry);

            // Resolve filename from STRG section
            string name = (fileentry.nameid != 0xFFFFFFFF && fileentry.nameid < strgData.stringEntriesList.Count)
                ? strgData.stringEntriesList[fileentry.nameid].filename
                : $"WAR_{i:D8}";

            // Move to the next WARC table entry (each entry is 8 bytes: 4 bytes for ID and 4 bytes for offset)
            currentTableOffset += 8;
        }
    }



    public void ExtractWARCs(BinaryReader br, strg strgData, FileTable fileTable, string outputDirectory)
    {
        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);
        for (int i = 0; i < warctableList.Count; i++)
        {
            var fileEntry = fileTable.Entries.FirstOrDefault(f => f.FileID == filetableList[i].fileid);


            if (fileEntry == null)
            {
                Console.WriteLine($"Warning: No matching FileTable entry for WARC ID {filetableList[i].fileid}");
                continue;
            }

            // Resolve filename from STRG or generate a default one
            string name = (filetableList[i].nameid != 0xFFFFFFFF && filetableList[i].nameid < strgData.stringEntriesList.Count)
                ? strgData.stringEntriesList[filetableList[i].nameid].filename
                : $"WAR_{i:D8}";

            // Calculate absolute offset to the WARC data
            long absoluteOffset = header.FILE_pointer + fileEntry.offset + 8;

            br.BaseStream.Seek(absoluteOffset, SeekOrigin.Begin);

            // Read the WARC data
            byte[] warcData = br.ReadBytes((int)fileEntry.size);

            // Extract BCWAVs from the WARC in memory
            using (var memoryStream = new MemoryStream(warcData))
            using (var warcReader = new BinaryReader(memoryStream))
            {
                string bcwavOutputDir = Path.Combine(outputDirectory, $"{name}");
                Directory.CreateDirectory(bcwavOutputDir);

                var warcExtractor = new BCWARExtractor(warcReader, bcwavOutputDir);
                List<(byte[] FileData, string Filename)> extractedFiles = warcExtractor.GetExtractedFiles();

                string BCWAVName = Path.GetFileNameWithoutExtension(name).Replace("WA_", "");
                WavDecoder.BatchDecode(extractedFiles, bcwavOutputDir, BCWAVName);
                

            }
        }
    }

}






