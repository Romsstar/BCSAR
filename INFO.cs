using System;

namespace BCSAR.INFO
{
	public class INFO
	{
		public struct header
		{
			public char[] magic; //MAGIC "INFO"
			public int size;
			public int audiotableID;
			public int audiotablePointer;
			public int tableID;
			public int tablePointer;
			public int bankID;
			public int bankPointer;
			public int warcIC;
			public int warcPointer;
		}
	}
}
