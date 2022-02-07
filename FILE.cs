using System;

public class FILE
{
    public struct header
    {
        public char[] magic; //MAGIC "FILE"
        public int size;
        public byte[] padding;
    }


}
