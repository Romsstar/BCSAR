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

        public List<INFOBlock> InfoList = new List<INFOBlock>();
        public List<DATABlock> DataBlockList = new List<DATABlock>();
        public List<adpcm> ADPCMList = new List<adpcm>();
        public struct header
        {
            public char[] magic; //"CWAV"
            public short endianess;
            public short headerSize;
            public int version;
            public int fileSize;
            public int numBlocks;
        }

        public struct INFOReference
        {
            public int ID;
            public int offset;
            public int size;
        }

        public struct DATAReference
        {
            public int ID;
            public int offset;
            public int size;
        }

        public struct INFOBlock
        {
            public byte[] magic; //"INFO"
            public int size;
            public byte encoding;
            public bool loop;
            public short padding;
            public int samplerate;
            public int loopstart, loopend;
            public int unk1, channelnum;

        }

        public struct sampleInfo
        {
            public int id;
            public int offset;
        }

        public struct adpcminfo
        {
            public int sampledata;
            public int sampleoffset;
            public int dspID;
            public int dspOff;
            public int unk3;
        }
        public struct adpcm
        {
            public short[] coefficients;
            public short[] destTable;
            public short predScale; //Predictor scale.
            public short yn1; //History sample 1.
            public short yn2; //History sample 2.
            public short loopPredScale; //Loop predictor scale.
            public short loopYn1; //Loop History sample.
            public short loopYn2; //Loop History sample 2.
            public byte[] padding; //Padding.
        }

        public struct DATABlock
        {
            public char[] magic; //"DATA"
            public int size;
            public byte[] data;
        }

        //For Calculating Alignment
        private byte paddingCalc(int pos, byte align)
        {
            byte tmp = (byte)(align - 1);
            align -= (byte)(pos & tmp);
            return (byte)(tmp & align);
        }

        enum encoding
        {
            PCM8 = 0,
            PCM16 = 1,
            DSP_ADPCM = 2, //4 bits per sample
            IMA_ADPCM = 3
        }

        //extend Binary Readers Functionality to Read Short Array
        public short[] ReadInt16Array(BinaryReader br, uint nToRead)
        {
            short[] array = new short[nToRead];
            for (int i = 0; i < nToRead; i++)
            {
            array[i] = br.ReadInt16();
            }
            return array;
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

            INFOBlock Info = new INFOBlock();
            Info.magic = br.ReadBytes(4);
            Info.size = br.ReadInt32();
            Info.encoding = br.ReadByte();
            Info.loop = br.ReadBoolean();
            Info.padding = br.ReadInt16();
            Info.samplerate = br.ReadInt32();
            Info.loopstart = br.ReadInt32();
            Info.loopend = br.ReadInt32(); //=num_samples;
            Info.unk1 = br.ReadInt32();
            Info.channelnum = br.ReadInt32();
            InfoList.Add(Info);

            for (int i = 0; i < Info.channelnum; i++)
            {
                sampleInfo sampleInfo = new sampleInfo();
                sampleInfo.id = br.ReadInt32(); //0x7100
                sampleInfo.offset = br.ReadInt32();
            }
            for (int i = 0; i < Info.channelnum; i++)
            {
                adpcminfo adpcminfo = new adpcminfo();
                adpcminfo.sampledata = br.ReadInt32(); //0x1F00
                adpcminfo.sampleoffset = br.ReadInt32();
                adpcminfo.dspID = br.ReadInt32();
                adpcminfo.dspOff = br.ReadInt32();
                adpcminfo.unk3 = br.ReadInt32();
            }
            for (int i = 0; i < Info.channelnum; i++)
            {
                adpcm adpcm = new adpcm();
                adpcm.coefficients = ReadInt16Array(br, 16);
                adpcm.predScale = br.ReadInt16(); //Predictor scale.
                adpcm.yn1 = br.ReadInt16(); //History sample 1.
                adpcm.yn2 = br.ReadInt16(); //History sample 2.
                adpcm.loopPredScale = br.ReadInt16();  //Loop predictor scale.
                adpcm.loopYn1 = br.ReadInt16();  //Loop History sample.
                adpcm.loopYn2 = br.ReadInt16();  //Loop History sample 2.
                adpcm.padding = br.ReadBytes(4);
                ADPCMList.Add(adpcm);
            }
            br.ReadBytes(paddingCalc((int)br.BaseStream.Position, 0x20));

            DATABlock datablock = new DATABlock();
            datablock.magic = br.ReadChars(4);
            datablock.size = br.ReadInt32();
            datablock.data = br.ReadBytes(datablock.size - 0x8);
            DataBlockList.Add(datablock);
     
        }
    }


}







