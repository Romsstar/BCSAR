using System;
using System.Collections.Generic;
using System.IO;

public class warc
{
    public int warcnum;

    public List<warctable> warctableList = new List<warctable>();
    public List<fileID> fileidList = new List<fileID>();

    public struct warctable
    {
        public int id;
        public int offset;
    }

    public struct fileID
    {
        public int fileid;
        public int waveCount;
    }


    public warc(BinaryReader br)
    {
        br.BaseStream.Position = 0x118584;//warctableoffset
        warcnum = br.ReadInt32();

        for (int i = 0; i < warcnum; i++)
        {
            br.BaseStream.Position = 0x118588 + i * 8;
            warctable entry = new warctable();
            entry.id = br.ReadInt32();
            entry.offset = br.ReadInt32();
            warctableList.Add(entry);

            br.BaseStream.Position = 0x118584 + warctableList[i].offset;
            fileID fileid = new fileID();
            fileid.fileid = br.ReadInt32();
            fileid.waveCount = br.ReadInt32();
            fileidList.Add(fileid);
        }
    }
}
