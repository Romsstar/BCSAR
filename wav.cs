using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCWAV;
using NAudio.Wave;

public class wav
{

    public static void DecodeToWav(List<(byte[] FileData, string Filename)> bcwavFiles, string outputDirectory, string bcwar)
    {
        // Ensure the output directory exists
        Directory.CreateDirectory(outputDirectory);

        foreach (var fileEntry in bcwavFiles)
        {
            using (var memoryStream = new MemoryStream(fileEntry.FileData))
            using (var br = new BinaryReader(memoryStream))
            {
                try
                {
                    // Parse the BCWAV file
                    cwav cwavFile = new cwav(br);

                    if (cwavFile.InfoList.Count == 0 || cwavFile.DataBlockList.Count == 0 ||
                        cwavFile.ADPCMList.Count == 0)
                    {
                        Console.WriteLine($"Skipping incomplete or invalid BCWAV file: {fileEntry.Filename}");
                        continue;
                    }

       
                    // Extract DSP-ADPCM data and metadata
                    var info = cwavFile.InfoList[0];
                    var dataBlock = cwavFile.DataBlockList[0];
                    var adpcm = cwavFile.ADPCMList;

                    int sampleRate = info.samplerate;
                    int numChannels = info.channelnum;
                    bool isLooping = info.loop;
                    int loopStart = info.loopstart;
                    int loopEnd = info.loopend;

                    // Calculate total samples
                    int totalSamples = isLooping
                        ? Math.Min(loopEnd, dataBlock.data.Length * 14 / 8)
                        : dataBlock.data.Length * 14 / 8;

                    // Prepare output buffers
                    List<short[]> pcmChannels = new List<short[]>();

                    // Decode each channel
                    for (int channel = 0; channel < numChannels; channel++)
                    {
                        Console.WriteLine($"Decoding Channel {channel}, Total Samples: {totalSamples}");
                        pcmChannels.Add(DecodeDSP(dataBlock.data, adpcm[channel], totalSamples));
                    }

                    // Generate WAV filename
                    string wavFilename = Path.Combine(outputDirectory,
                        Path.GetFileNameWithoutExtension(bcwar+"_"+fileEntry.Filename) + ".wav");

                    // Save PCM data to WAV file
                    SaveInterleavedToWav(pcmChannels, sampleRate, wavFilename);

                    Console.WriteLine($"Decoded to WAV: {wavFilename}");
                }
            
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing BCWAV file {fileEntry.Filename}: {ex.Message}");
                }
            }
        }
    }




private static short[] DecodeDSP(byte[] dspData, cwav.adpcm adpcm, int totalSamples)
    {
        short[] pcmSamples = new short[totalSamples];
        int pcmIndex = 0;

        // Initialize history and coefficients
        short yn1 = adpcm.yn1; // History sample 1
        short yn2 = adpcm.yn2; // History sample 2
        short[] coefficients = adpcm.coefficients;

        // Decode each DSP frame (8 bytes = 14 samples)
        for (int i = 0; i < dspData.Length; i += 8)
        {
            // Read the header byte
            byte header = dspData[i];
            int predictorIndex = (header >> 4) & 0x0F; // Upper 4 bits: predictor index
            short scale = (short)(1 << (header & 0x0F)); // Lower 4 bits: scale factor

            // Validate predictor index
            if (predictorIndex < 0 || predictorIndex >= coefficients.Length / 2)
            {
                throw new Exception($"Invalid predictor index ({predictorIndex}) in DSP data.");
            }

            // Get the predictor coefficients
            short coef1 = coefficients[predictorIndex * 2];
            short coef2 = coefficients[predictorIndex * 2 + 1];

            for (int j = 0; j < 14 && pcmIndex + 1 < pcmSamples.Length; j += 2)
            {
                byte nibblePair = dspData[i + 1 + j / 2];
                pcmSamples[pcmIndex++] = DecodeDSPNibble((nibblePair >> 4) & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
                pcmSamples[pcmIndex++] = DecodeDSPNibble(nibblePair & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
            }
        }

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


    private static void SaveInterleavedToWav(List<short[]> pcmChannels, int sampleRate, string outputFile)
    {
        int numChannels = pcmChannels.Count;
        int numSamples = pcmChannels[0].Length;


        using (var writer = new WaveFileWriter(outputFile, new WaveFormat(sampleRate, 16, numChannels)))
        {
            for (int i = 0; i < numSamples; i++)
            {
                foreach (var channel in pcmChannels)
                {
                    writer.WriteSample(channel[i] / 32768.0f); // Convert to float
                }
            }
        }
    }



    }

  








