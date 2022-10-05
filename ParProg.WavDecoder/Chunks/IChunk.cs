namespace ParProg.WavDecoder.Chunks;

public interface IChunk
{
    public string chunkId { get; }
    public int chunkSize { get; }
    
    public string Describe();
}