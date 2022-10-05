namespace ParProg.WavDecoder.Chunks;

public class ListChunk : IChunk
{
    public string chunkId { get; }
    public int chunkSize { get; }

    public ListChunk(FileStream fileStream)
    {
        var binaryReader = new BinaryReader(fileStream);
        chunkId = "list";
        chunkSize = binaryReader.ReadInt32();

        binaryReader.ReadBytes(chunkSize);
    }   
    
    public string Describe()
    {
        throw new NotImplementedException();
    }
}