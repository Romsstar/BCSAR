﻿using System;
using System.IO;

namespace BCSAR.INFO
{
    public class info
    {
        //Contains Offsets for the tables
        public char[] magic; //MAGIC "INFO"
        public int size;
        public int audioTableID;
        public int audioTable;
        public int setTableID;
        public int setTable;
        public int bankTableID;
        public int bankTable;
        public int warcID;
        public int warcTable;
        public int groupTableID;
        public int groupTable;
        public int playerTableID;
        public int playerTable;
        public int FILETableID;
        public int FILETable;

        public info(BinaryReader br)
        {
            magic = br.ReadChars(4);
            size = br.ReadInt32();
            audioTableID = br.ReadInt32();
            audioTable = br.ReadInt32();
            setTableID = br.ReadInt32();
            setTable = br.ReadInt32();
            bankTableID = br.ReadInt32();
            bankTable = br.ReadInt32();
            warcID = br.ReadInt32();
            warcTable = br.ReadInt32(); // Convert relative to absolute
            groupTableID = br.ReadInt32();
            groupTable= br.ReadInt32();
            playerTableID= br.ReadInt32();
            playerTable =br.ReadInt32() ;
            FILETableID =br.ReadInt32();
            FILETable=br.ReadInt32();

        }
    }
}
