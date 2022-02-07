using System;
using System.IO;

namespace BCSAR.CSAR
{
    public class header
    {
        public char[] magic; //MAGIC "CSAR"
        public short endian; //0xFEFF = Big Endian, 0xFFFE = Little Endian
        public short CSARheader_size;
        public int version;
        public int CSARtotal_size;
        public int partition_num;

        public int STRG_id; // Always 0x2000
        public static int STRG_pointer { get; set; }
        public int STRG_size;

        public int INFO_id; // Always 0x2001
        public int INFO_pointer;
        public int INFO_size;

        public int FILE_id; // Always 0x2002
        public int FILE_pointer;
        public int FILE_size;

        public int partition4_pointer; //usually not applicable
        public int partition4__size;

        public header(BinaryReader br)
        {
            magic = br.ReadChars(4);
            endian = br.ReadInt16();
            CSARheader_size = br.ReadInt16();
            version = br.ReadInt32();
            CSARtotal_size = br.ReadInt32();
            partition_num = br.ReadInt32();

            STRG_id = br.ReadInt32();
            STRG_pointer = br.ReadInt32();
            STRG_size = br.ReadInt32();

            INFO_id = br.ReadInt32();
            INFO_pointer = br.ReadInt32();
            INFO_size = br.ReadInt32();

            FILE_id = br.ReadInt32();
            FILE_pointer = br.ReadInt32();
            FILE_size = br.ReadInt32();

            partition4_pointer = br.ReadInt32();
            partition4__size = br.ReadInt32();
        }
    }
}

