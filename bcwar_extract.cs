using System;
using System.Collections.Generic;
using System.IO;

public class bcwar_extract
{
    private readonly string outputDirectory;
    private readonly BinaryReader br;

    private long infoSectionOffset;
    private long fileSectionOffset;
    private List<FileInfoEntry> fileEntries;

    public bcwar_extract(BinaryReader br, string outputDirectory)
    {
        this.br = br;
        this.outputDirectory = outputDirectory;
        Directory.CreateDirectory(outputDirectory);
        fileEntries = new List<FileInfoEntry>();
    }

    private void ParseHeader()
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        string magic = new string(br.ReadChars(4));
        ushort bom = br.ReadUInt16();

        if (magic != "CWAR")
            throw new InvalidDataException($"Invalid magic: {magic}, expected 'CWAR'");

        br.ReadUInt16(); // header length
        br.ReadUInt32(); // version
        br.ReadUInt32(); // file size
        int sectionCount = br.ReadInt32();

        for (int i = 0; i < sectionCount; i++)
        {
            ushort sectionID = br.ReadUInt16();
            br.ReadUInt16(); // padding
            int sectionOffset = br.ReadInt32();
            int sectionSize = br.ReadInt32();

            if (sectionID == 0x6800) // INFO section
            {
                infoSectionOffset = sectionOffset;
            }
            else if (sectionID == 0x6801) // FILE section
            {
                fileSectionOffset = sectionOffset;
            }
        }
    }

    private void ReadInfoSection()
    {
        br.BaseStream.Seek(infoSectionOffset, SeekOrigin.Begin);
        string magic = new string(br.ReadChars(4));
        if (magic != "INFO")
            throw new InvalidDataException($"Invalid INFO magic: {magic}");

        int sectionSize = br.ReadInt32();
        int entryCount = br.ReadInt32();

        for (int i = 0; i < entryCount; i++)
        {
            ushort id = br.ReadUInt16();
            br.ReadUInt16(); // padding
            int pos = br.ReadInt32();
            int size = br.ReadInt32();

            if (id == 0x1f00) // File reference
            {
                fileEntries.Add(new FileInfoEntry
                {
                    Position = pos,
                    Size = size
                });
            }
        }
    }

    public void ExtractBCWAV()
    {
        ParseHeader();
        ReadInfoSection();
        br.BaseStream.Seek(fileSectionOffset, SeekOrigin.Begin);
        string magic = new string(br.ReadChars(4));
        if (magic != "FILE")
            throw new InvalidDataException($"Invalid FILE magic: {magic}");

        int sectionSize = br.ReadInt32();
        long baseOffset = br.BaseStream.Position;

        for (int i = 0; i < fileEntries.Count; i++)
        {
            var fileInfo = fileEntries[i];
            long fileOffset = baseOffset + fileInfo.Position;

            br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
            byte[] fileData = br.ReadBytes(fileInfo.Size);

            // Determine the file extension (placeholder function, you should implement proper detection)
            string ext = DetermineFileExtension(fileData);

            string filename = Path.Combine(outputDirectory, $"{i}{ext}");
            File.WriteAllBytes(filename, fileData);
            
        }
    }

    private List<(byte[] FileData, string Filename)> ExtractFiles()
    {
        br.BaseStream.Seek(fileSectionOffset, SeekOrigin.Begin);
        string magic = new string(br.ReadChars(4));
        if (magic != "FILE")
            throw new InvalidDataException($"Invalid FILE magic: {magic}");

        int sectionSize = br.ReadInt32();
        long baseOffset = br.BaseStream.Position;

        var extractedFiles = new List<(byte[] FileData, string Filename)>();

        for (int i = 0; i < fileEntries.Count; i++)
        {
            var fileInfo = fileEntries[i];
            long fileOffset = baseOffset + fileInfo.Position;

            br.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
            byte[] fileData = br.ReadBytes(fileInfo.Size);

            string ext = DetermineFileExtension(fileData);

            string filename = $"{i}{ext}";

            // Add the file data and filename to the list
            extractedFiles.Add((fileData, filename));
        }

        return extractedFiles;
    }

    public List<(byte[] FileData, string Filename)> GetExtractedFiles()
    {
        ParseHeader();
        ReadInfoSection();
        return ExtractFiles();
    }

    private string DetermineFileExtension(byte[] fileData)
    {
       if (fileData.Length >= 4 && fileData[0] == 'C' && fileData[1] == 'W' && fileData[2] == 'A' && fileData[3] == 'V')
            return ".bcwav";
        return ".bin";
    }

    private class FileInfoEntry
    {
        public int Position { get; set; }
        public int Size { get; set; }
    }
}
