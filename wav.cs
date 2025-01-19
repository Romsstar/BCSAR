using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using BCWAV;
using NAudio.Wave;
using static dspadpcm_decode;

public class WavDecoder
{

    public static void DecodeSingleBcwav(byte[] fileData, string outputDirectory, string bcwarName)
    {
        using (var memoryStream = new MemoryStream(fileData))
        using (var br = new BinaryReader(memoryStream))
        {
            // Parse the BCWAV file
            cwav cwavFile = new cwav(br);

            int totalSamples = cwavFile.Info.isLoop ? (cwavFile.Info.loopend) : cwavFile.Data.data.Length * 14 / 8;

            // Storage for interleaved PCM samples
            short[] pcmData = DecodeDSP(cwavFile.Data.data, cwavFile, totalSamples);

            // Ensure the output filename only has .wav as its extension
            string wavFilename = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(bcwarName)) + ".wav";

            SaveInterleavedToWav(pcmData, cwavFile.Info.samplerate, totalSamples, cwavFile.Info.channelnum, wavFilename);
        }
    }

    

    public static void BatchDecode(List<(byte[] FileData, string Filename)> bcwavFiles, string outputDirectory, string bcwar)
    {
        // Group files by base filename (without extensions) to identify duplicates
        var groupedFiles = bcwavFiles
            .GroupBy(file => Path.GetFileNameWithoutExtension(bcwar))
            .ToDictionary(group => group.Key, group => group.Count());

        Parallel.ForEach(bcwavFiles, fileEntry =>
        {
            // Get the base filename without extension
            string baseFilename = Path.GetFileNameWithoutExtension(bcwar);

            // Determine if a suffix should be appended
            string filenamePart = (groupedFiles[baseFilename] > 1)
                ? "_" + fileEntry.Filename
                : "";

            // Decode the file
            DecodeSingleBcwav(fileEntry.FileData, outputDirectory, bcwar + filenamePart);
        });
    }






    private static void SaveInterleavedToWav(short[] pcmData, int sampleRate, int numSamples, int numChannels, string outputFile)
    {
        using (var writer = new WaveFileWriter(outputFile, new WaveFormat(sampleRate, 16, numChannels)))
        {
            for (int i = 0; i < numSamples * numChannels; i++)
            {
                float normalizedSample = pcmData[i] / 32768.0f;   // Normalize PCM data to float and write the sample
                writer.WriteSample(normalizedSample);
            }
        }
    }
    
}




