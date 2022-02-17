using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCWAV;
using NAudio.Wave;
using VGAudio;
public class wav
{


    public static void convert_to_wav()
    {


        foreach (string file in Directory.GetFiles(@"C:\TestFiles", "*.bcwav"))
        {
            using (var input = new FileStream(file, FileMode.Open))
            using (var br = new BinaryReader(input))
            {
                cwav cwav = new cwav(br);

                for (var i = 0; i < cwav.ADPCMList.Count; i++)
                {
                    short hist1 = cwav.ADPCMList[i].yn1;
                    short hist2 = cwav.ADPCMList[i].yn2;
                    int dstIndex = 0;
                    int srcIndex = 0;                

                }
                //using (var output = File.Open(file + ".wav", FileMode.Create))

                //{


                //    short[] test = VGAudio.Codecs.GcAdpcm.GcAdpcmDecoder.Decode(cwav.DataBlockList[0].data, cwav.ADPCMList[0].coefficients);
                //    byte[] write = Array.ConvertAll(test, b => (byte)b);
                //    File.WriteAllBytes("test.wav",write);
                //    }
                //}


                //    using (var bw = new BinaryWriter(output, Encoding.UTF8, false))
                //    {
                //    bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"))e;
                //    bw.Write(cwav.InfoList[0].samplerate);
                //    bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt"));
                //    bw.Flush();
                //}


            }
  
        }
    }
   
}





