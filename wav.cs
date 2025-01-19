using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using BCSAR;
using BCWAV;
using NAudio.Wave;

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



    private static short[] DecodeDSP(byte[] dspData, cwav cwav, int totalSamples)
    {
        int channelCount = cwav.Info.channelnum;
        short[] pcmSamples = new short[totalSamples * channelCount];

        // Parallelize the decoding across channels
        Parallel.For(0, channelCount, channel =>
        {
            short yn1, yn2;

            int channelOffset = cwav.sampleref[channel].offset;
            yn1 = cwav.Info.isLoop ? cwav.adpcmInfo[channel].loopYn1 : cwav.adpcmInfo[channel].yn1;
            yn2 = cwav.Info.isLoop ? cwav.adpcmInfo[channel].loopYn2 : cwav.adpcmInfo[channel].yn2;

            short[] coefficients = cwav.adpcmInfo[channel].coefficients;

            int pcmIndex = channel;
            for (int i = channelOffset; i < dspData.Length; i += 8)
            {
                byte header = dspData[i];
                int predictorIndex = (header >> 4) & 0x0F;
                short scale = (short)(1 << (header & 0x0F));

                if (predictorIndex < 0 || predictorIndex >= coefficients.Length / 2)
                {
                    throw new Exception($"Invalid predictor index ({predictorIndex}) in DSP data at offset {i}.");
                }

                short coef1 = coefficients[predictorIndex * 2];
                short coef2 = coefficients[predictorIndex * 2 + 1];

                for (int j = 0; j < 14 && pcmIndex + channelCount < pcmSamples.Length; j += 2)
                {
                    if (i + 1 + j / 2 >= dspData.Length)
                    {
                        throw new Exception($"DSP data ended unexpectedly at offset {i + 1 + j / 2}.");
                    }

                    byte nibblePair = dspData[i + 1 + j / 2];
                    pcmSamples[pcmIndex] = DecodeDSPNibble((nibblePair >> 4) & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
                    pcmIndex += channelCount;
                    pcmSamples[pcmIndex] = DecodeDSPNibble(nibblePair & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
                    pcmIndex += channelCount;
                }
            }
        });

        return pcmSamples;
    }



    private static short DecodeDSPNibble(int nibble, short scale, short coef1, short coef2, ref short yn1, ref short yn2)
    {
        // Convert nibble to signed 4-bit value
        if (nibble > 7)
        {
            nibble -= 16;
        }

        // Predict the sample using coefficients and history
        int sample = (nibble * scale) + ((yn1 * coef1) >> 11) + ((yn2 * coef2) >> 11);

        // Clamp to 16-bit range
        sample = Math.Clamp(sample, short.MinValue, short.MaxValue);

        // Update history
        yn2 = yn1;
        yn1 = (short)sample;

        return (short)sample;
    }

    
    private static void SaveInterleavedToWav(short[] pcmData, int sampleRate, int numSamples, int numChannels, string outputFile)
    {
        using (var writer = new WaveFileWriter(outputFile, new WaveFormat(sampleRate, 16, numChannels)))
        {
            for (int i = 0; i < numSamples * numChannels; i++)
            {
                // Normalize PCM data to float and write the sample
                float normalizedSample = pcmData[i] / 32768.0f;
                writer.WriteSample(normalizedSample);
            }
        }
    }
    
}




