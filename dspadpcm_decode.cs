using BCWAV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class dspadpcm_decode
    {
        public static short[] DecodeDSP(byte[] dspData, cwav cwav, int totalSamples)
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



        public static short DecodeDSPNibble(int nibble, short scale, short coef1, short coef2, ref short yn1, ref short yn2)
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
    }

