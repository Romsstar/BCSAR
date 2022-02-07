using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BCSAR.CSAR;
using Newtonsoft.Json;

namespace BCSAR.STRG
{
    public class strg
    {
     
        public char[] magic; //MAGIC "STRG"
        public int size;
        public int id;
        public int strgtableOffset;
        public int lookup_id;
        public int lookup_pointer;
        public int strgCount;
        public char[] name;
        public List<stringTableRecord> tableRecordsList = new List<stringTableRecord>(); //Records.
        public List<stringEntry> stringEntries = new List<stringEntry>();
       
        public struct stringTableRecord
        {

            public int node_type;
            public int names_offset;
            public int names_length;
        }

        public struct stringEntry
        {
            public string filename; //String data.
            public byte separator; //Separator 0x0           
        }


        public strg(BinaryReader br)
        {
            magic = br.ReadChars(4);
            size = br.ReadInt32();
            id = br.ReadInt32();
            strgtableOffset = br.ReadInt32();
            lookup_id = br.ReadInt32();
            lookup_pointer = br.ReadInt32();
            strgCount = br.ReadInt32();

             for (int i = 0; i < strgCount; i++)
            {
                //Get NodeType,Offsets+Length of String Entry
                br.BaseStream.Position = 0x5C + i * 12;
                stringTableRecord s = new stringTableRecord();
                s.node_type = br.ReadInt32();
                s.names_offset = br.ReadInt32();
                s.names_length = br.ReadInt32();
                tableRecordsList.Add(s); 

                //Get Filename Strings
                br.BaseStream.Position = strgtableOffset + 0x8 +header.STRG_pointer +tableRecordsList[i].names_offset;  //0x40+0x10+8 = originalvalue+strgtableoffset
                name = br.ReadChars(tableRecordsList[i].names_length - 1);
                stringEntry namestr = new stringEntry();
                namestr.filename = new string(name);
                namestr.separator = br.ReadByte();
                stringEntries.Add(namestr);

            }

        }
    }
}
