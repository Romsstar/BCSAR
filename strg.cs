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
        public byte[] padding;
        public List<stringTableRecord> tableRecordsList = new List<stringTableRecord>();
        public List<stringEntry> stringEntriesList = new List<stringEntry>();
        public List<lookupHeader> lookUpheaderList = new List<lookupHeader>();
        public List<lookupTable> lookUpList = new List<lookupTable>();

        public struct stringTableRecord
        {

            public int node_type;
            public int names_offset;
            public int names_length;
        }

        public struct stringEntry
        {
            public string filename;
            public byte separator; //Separator 0x0           
        }

        public struct lookupHeader
        {
            public int rootNode;
            public int nodeCount;
        }

  
        public struct lookupTable
        {

            public short flags;
            public short bit_test;
            public int leftidx;
            public int rightidx;
            public Node Nodedata;
        }

        public struct Node
        {
            public int stringId;
            public int itemId;
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
                br.BaseStream.Position = strgtableOffset + 0x8 + header.STRG_pointer + tableRecordsList[i].names_offset;  //0x40+0x10+8 = originalvalue+strgtableoffset
                name = br.ReadChars(tableRecordsList[i].names_length - 1);
                stringEntry namestr = new stringEntry();
                namestr.filename = new string(name);
                namestr.separator = br.ReadByte();
                stringEntriesList.Add(namestr);
            }
            //Get Lookup Table
            br.BaseStream.Position = lookup_pointer + header.STRG_pointer + 0x8;

            lookupHeader lkheader = new lookupHeader();
            lkheader.rootNode = br.ReadInt32();
            lkheader.nodeCount = br.ReadInt32();
            lookUpheaderList.Add(lkheader);

            lookupTable records = new lookupTable();
            
           for (int i = 0; i < lkheader.nodeCount; i++)
            {
                records.flags = br.ReadInt16();
                records.bit_test = br.ReadInt16();
                records.leftidx = br.ReadInt32();
                records.rightidx = br.ReadInt32();
                records.Nodedata.stringId = br.ReadInt32();
                records.Nodedata.itemId = br.ReadInt32();
                lookUpList.Add(records);
            }
            padding = br.ReadBytes(0xC);//Padding                     

        }
    }
}