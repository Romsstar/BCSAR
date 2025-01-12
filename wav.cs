using System;
using System.Collections.Generic;
using System.IO;
using BCWAV;
using NAudio.Wave;

public class wav
{

    public static void DecodeToWav(string outputFile)
    {

        foreach (string file in Directory.GetFiles(@"C:\TestFiles", "*.bcwav"))
        {
            using (var input = new FileStream(file, FileMode.Open))
            using (var br = new BinaryReader(input))
            {
                cwav cwavFile = new cwav(br);



                if (cwavFile.InfoList.Count == 0 || cwavFile.DataBlockList.Count == 0 || cwavFile.ADPCMList.Count == 0)
                {
                    throw new InvalidOperationException("CWAV file is incomplete or invalid.");
                }




                // Extract DSP-ADPCM data and metadata
                var info = cwavFile.InfoList[0];
                var dataBlock = cwavFile.DataBlockList[0];
                var adpcm = cwavFile.ADPCMList;

                int sampleRate = info.samplerate;
                int numChannels = info.channelnum;
                bool isLooping = info.loop;
                int loopStart = info.loopstart;
                int loopEnd = info.loopend; //Correlates to total number of samples

                // Prepare output buffers
                List<short[]> pcmChannels = new List<short[]>();

                // Decode each channel
                for (int channel = 0; channel < numChannels; channel++)
                {
                    pcmChannels.Add(DecodeDSP(dataBlock.data, adpcm[channel], loopEnd));
                }
                PrintCoefficients(cwavFile.ADPCMList);
                // Save PCM data to WAV file
                SaveInterleavedToWav(pcmChannels, sampleRate, outputFile);
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
            int predictorIndex = (header >> 4) & 0x0F;  // Upper 4 bits: predictor index
            short scale = (short)(1 << (header & 0x0F)); // Lower 4 bits: scale factor

            // Validate predictor index
            if (predictorIndex < 0 || predictorIndex >= coefficients.Length / 2)
            {
                throw new Exception($"Invalid predictor index ({predictorIndex}) in DSP data.");
            }

            // Get the predictor coefficients
            short coef1 = coefficients[predictorIndex * 2];
            short coef2 = coefficients[predictorIndex * 2 + 1];

            // Decode 14 samples from the next 7 bytes
            for (int j = 0; j < 14 && pcmIndex < totalSamples; j += 2)
            {
                // Read the current byte containing two nibbles
                byte nibblePair = dspData[i + 1 + j / 2];

                // Decode the high nibble (first sample)
                short sample1 = DecodeDSPNibble((nibblePair >> 4) & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
                pcmSamples[pcmIndex++] = sample1;

                // Decode the low nibble (second sample)
                short sample2 = DecodeDSPNibble(nibblePair & 0x0F, scale, coef1, coef2, ref yn1, ref yn2);
                pcmSamples[pcmIndex++] = sample2;
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


    private static void PrintCoefficients(List<cwav.adpcm> adpcmList)
    {
        Console.WriteLine("Printing DSP Coefficients (Hexadecimal):");

        for (int channel = 0; channel < adpcmList.Count; channel++)
        {
            Console.WriteLine($"Channel {channel}:");

            short[] coefficients = adpcmList[channel].coefficients;

            for (int i = 0; i < coefficients.Length; i += 2)
            {
                string coef1 = $"0x{coefficients[i] & 0xFFFF:X4}";     // Convert signed to unsigned hex
                string coef2 = $"0x{coefficients[i + 1] & 0xFFFF:X4}"; // Convert signed to unsigned hex
                Console.WriteLine($"Predictor {i / 2}: Coef1 = {coef1}, Coef2 = {coef2}");
            }

            Console.WriteLine($"Initial Predictor Scale: {adpcmList[channel].predScale:X4}");
            Console.WriteLine($"History Sample 1 (yn1): {adpcmList[channel].yn1:X4}");
            Console.WriteLine($"History Sample 2 (yn2): {adpcmList[channel].yn2:X4}");
            Console.WriteLine($"Loop Predictor Scale: {adpcmList[channel].loopPredScale:X4}");
            Console.WriteLine($"Loop History Sample 1 (loopYn1): {adpcmList[channel].loopYn1:X4}");
            Console.WriteLine($"Loop History Sample 2 (loopYn2): {adpcmList[channel].loopYn2:X4}");
            Console.WriteLine(new string('-', 50));
        }
    }
}
  








