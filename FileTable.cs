using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;

public class FileTableEntry
{
    public int FileID { get; set; } // Index in the file table
    public ushort header { get; set; } // Header value
    public ushort padding { get; set; } // Header value
    public uint offset { get; set; } // Relative offset to the data
    public uint size { get; set; }   // Size of the data
}

public class FileTable
{
    public List<FileTableEntry> Entries { get; private set; }

    public FileTable(BinaryReader br, long fileTableOffset,long filePartitionOffset)
    {
        Entries = new List<FileTableEntry>();

        // Seek to the file table offset
        br.BaseStream.Seek(fileTableOffset, SeekOrigin.Begin);

        // Read the file count from the table
        int fileCount = br.ReadInt32();
        long currentTableOffset = fileTableOffset + 4; // +4 to skip the file count itself
     
        for (int i = 0; i < fileCount; i++)
        {
            // Seek to the current table entry
            br.BaseStream.Seek(currentTableOffset, SeekOrigin.Begin);

            // Read the offset ID and relative offset
            int offsetId = br.ReadInt32();
            int relativeOffset = br.ReadInt32();

            // Calculate the absolute offset
            long absoluteOffset = fileTableOffset + relativeOffset+12;

         
            // Seek to the actual data for this file
            br.BaseStream.Seek(absoluteOffset, SeekOrigin.Begin);

            // Read Header, Offset, and Size
            FileTableEntry entry = new FileTableEntry
            {
                FileID = i,
                header = br.ReadUInt16(),
                padding = br.ReadUInt16(),
                offset = br.ReadUInt32(),
                size = br.ReadUInt32(),
            };

            // Add the entry to the list
            Entries.Add(entry);

            // Move to the next entry in the file table
            currentTableOffset += 8; 
        }
    }
}
