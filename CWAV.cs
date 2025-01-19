using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BCSAR.CSAR;
using Newtonsoft.Json;

namespace BCWAV
{
    public class cwav
    {
        public cwav_reference[] channelinfo;  // Array of references for channelinfo
        public cwav_reference[] sampleref;    // Array of references for sampleref
        public cwav_reference[] codecref;     // Array of references for codecref
        public INFOBlock Info;
        public DATABlock Data;
        public adpcm[] adpcmInfo;

        public struct header
        {
            public char[] magic; //"CWAV"
            public short endianess;
            public short headerSize;
            public int version;
            public int fileSize;
            public int numBlocks;
        }

        public class INFOReference
        {
            public int ID { get; set; }
            public int offset;
            public int size;
        }

        public struct DATAReference
        {
            public int ID;
            public int offset;
            public int size;
        }

        public class INFOBlock
        {
            public byte[] magic; //"INFO"
            public int size;
            public byte encoding;
            public bool isLoop { get; set; }
            public short padding;
            public int samplerate;
            public int loopstart, loopend;
            public int unk1, channelnum;
        }
        
        public struct cwav_reference
        {
            public short id;
            public short padding;
            public int offset;
        }
        
        public class adpcm
        {
            public short[] coefficients;
            public short predScale; //Predictor scale.
            public short yn1; //History sample 1.
            public short yn2; //History sample 2.
            public short loopPredScale; //Loop predictor scale.
            public short loopYn1; //Loop History sample.
            public short loopYn2; //Loop History sample 2.
        }

        public class DATABlock
        {
            public char[] magic; //"DATA"
            public int size;
            public byte[] data;
        }

        //For Calculating Alignment
        private int paddingCalc(int position, int alignment)
        {
            return (alignment - (position % alignment)) % alignment;
        }

        enum encoding
        {
            PCM8 = 0,
            PCM16 = 1,
            DSP_ADPCM = 2, //4 bits per sample
            IMA_ADPCM = 3
        }



        public cwav(BinaryReader br)
        {
            header header = new header();
            header.magic = br.ReadChars(4);
            header.endianess = br.ReadInt16();
            header.headerSize = br.ReadInt16();
            header.version = br.ReadInt32();
            header.fileSize = br.ReadInt32();
            header.numBlocks = br.ReadInt32();

            INFOReference INFOReference = new INFOReference();
            INFOReference.ID = br.ReadInt32();
            INFOReference.offset = br.ReadInt32();
            INFOReference.size = br.ReadInt32();

            DATAReference DATAReference = new DATAReference();
            DATAReference.ID = br.ReadInt32();
            DATAReference.offset = br.ReadInt32();
            DATAReference.size = br.ReadInt32();

            br.ReadBytes(20); //Padding

            Info = new INFOBlock();
            Info.magic = br.ReadBytes(4);
            Info.size = br.ReadInt32();
            Info.encoding = br.ReadByte();
            Info.isLoop = br.ReadBoolean();
            Info.padding = br.ReadInt16();
            Info.samplerate = br.ReadInt32();
            Info.loopstart = br.ReadInt32();
            Info.loopend = br.ReadInt32(); //=num_samples;
            Info.unk1 = br.ReadInt32();
            Info.channelnum = br.ReadInt32();

            int channelNum = Info.channelnum;
            channelinfo = new cwav_reference[Info.channelnum];
            sampleref = new cwav_reference[Info.channelnum];
            codecref = new cwav_reference[Info.channelnum];
            adpcmInfo = new adpcm[Info.channelnum];

            for (int i = 0; i < Info.channelnum; i++)
            {
                // Read the channel reference (Channel info)
                channelinfo[i] = new cwav_reference();
                channelinfo[i].id = br.ReadInt16();  // 2 bytes: Read Channel ID
                channelinfo[i].padding = br.ReadInt16();  // 2 bytes: Padding
                channelinfo[i].offset = br.ReadInt32() + 0x1C+INFOReference.offset;  // 4 bytes: Read Channel offset
            }

            for (int i = 0; i < Info.channelnum; i++)
            {
                sampleref[i] = new cwav_reference();
                sampleref[i].id = br.ReadInt16();  // 0x1F00
                sampleref[i].padding = br.ReadInt16(); 
                sampleref[i].offset = br.ReadInt32()+DATAReference.offset+8;  //Sample offset
          
                codecref[i] = new cwav_reference();
                codecref[i].id = br.ReadInt16();  // 2 bytes: Sample ID
                codecref[i].padding = br.ReadInt16();  // 2 bytes: Padding
                codecref[i].offset = br.ReadInt32() + channelinfo[i].offset;  // DSP Offset
                br.ReadInt32(); //Reserved
            }

     


            for (int i = 0; i < Info.channelnum; i++)
            {
                adpcmInfo[i] = new adpcm();
                adpcmInfo[i].coefficients = new short[16];
                
                for (int j = 0; j < 16; j++)
                {
                    adpcmInfo[i].coefficients[j] = br.ReadInt16(); // Read each coefficient
                }

                adpcmInfo[i].predScale = br.ReadInt16(); //Predictor scale.
                adpcmInfo[i].yn1 = br.ReadInt16(); //History sample 1.
                adpcmInfo[i].yn2 = br.ReadInt16(); //History sample 2.
                adpcmInfo[i].loopPredScale = br.ReadInt16();  //Loop predictor scale.
                adpcmInfo[i].loopYn1 = br.ReadInt16();  //Loop History sample.
                adpcmInfo[i].loopYn2 = br.ReadInt16();  //Loop History sample 2.
                br.ReadBytes(2);
              
            }

            br.ReadBytes(paddingCalc((int)br.BaseStream.Position, 0x20));

            Data = new DATABlock();
            Data.magic = br.ReadChars(4);
            Data.size = br.ReadInt32();
            Data.data = br.ReadBytes(Data.size - 0x8);

        }
        public static void printChannelInfo(cwav cwavFile)
        {
            for (int i = 0; i < cwavFile.Info.channelnum; i++)
            {
                Console.WriteLine($"Channel {i}:");
                Console.WriteLine($" > Channel ref idtype:  0x{cwavFile.channelinfo[i].id:X4}");
                Console.WriteLine($" > Channel ref offset:  0x{cwavFile.channelinfo[i].offset:X8}");
                Console.WriteLine($" > Sample ref idtype:   0x{cwavFile.sampleref[i].id:X4}");
                Console.WriteLine($" > Sample ref offset:   0x{cwavFile.sampleref[i].offset:X8}");
                Console.WriteLine($" > Codec ref idtype:    0x{cwavFile.codecref[i].id:X4}");
                Console.WriteLine($" > Codec ref offset:    0x{cwavFile.codecref[i].offset:X8}");

                Console.WriteLine($"Channel {i} coefficients:");
                for (int j = 0; j < cwavFile.adpcmInfo[i].coefficients.Length; j++)
                {
                    Console.WriteLine($"Coefficient {j}: {cwavFile.adpcmInfo[i].coefficients[j]:X8}");
                }
                if (cwavFile.Info.isLoop)
                {
                    Console.WriteLine($"Loop Yn1 : {cwavFile.adpcmInfo[i].loopYn1:X8}");
                    Console.WriteLine($"Loop Yn2 : {cwavFile.adpcmInfo[i].loopYn1:X8}");
                    Console.WriteLine($"Loop PredScale : {cwavFile.adpcmInfo[i].loopPredScale:X8}");
                }
                else
                {
                    Console.WriteLine($"Yn1 : {cwavFile.adpcmInfo[i].yn1:X8}");
                    Console.WriteLine($"Yn2 : {cwavFile.adpcmInfo[i].yn2:X8}");
                    Console.WriteLine($"PredScale : {cwavFile.adpcmInfo[i].predScale:X8}");

                }
            }
        }
    }


}






