using System.Collections.Generic;

public class BCWAR
{
    public int FileId { get; set; }
    public int WaveCount { get; set; }
    public string Name { get; set; }
    public List<WaveEntry> Waves { get; set; } = new List<WaveEntry>();
}

public class WaveEntry
{
    public int Offset { get; set; }
    public int Size { get; set; }
    public string Name { get; set; }
}