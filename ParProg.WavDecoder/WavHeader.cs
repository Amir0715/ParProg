using System.Text;
using ParProg.WavDecoder.Chunks;

namespace ParProg.WavDecoder;

public class WavHeader
{
    public string chunkId { get; } // RIFF
    public int chunkSize { get; } // filezie - 8
    public string format { get; } // "WAVE"
    
    public FmtChunk FmtChunk { get; }
    public DataChunk DataChunk { get; }
    public ListChunk ListChunk { get; }

    public WavHeader(FileStream fileStream)
    {
        var binaryReader = new BinaryReader(fileStream);
        
        chunkId = new string(binaryReader.ReadChars(4));
        if (chunkId != "RIFF")
        {
            throw new NotRIFFException();
        }
        
        chunkSize = binaryReader.ReadInt32();
        format = new string(binaryReader.ReadChars(4));

        while (fileStream.Position != fileStream.Length)
        {
            var subchunkId = new string(binaryReader.ReadChars(4));
            switch (subchunkId.ToLower())
            {
                case "fmt ":
                    FmtChunk = new FmtChunk(fileStream);
                    break;
                case "data":
                    DataChunk = new DataChunk(fileStream);
                    break;
                case "list":
                    ListChunk = new ListChunk(fileStream);
                    break;
                default:
                    break;
            }
        }
    }

    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"ChunkId: {chunkId}");
        stringBuilder.AppendLine($"ChunkSize: {chunkSize}");
        stringBuilder.AppendLine($"Format: {format}");
        stringBuilder.AppendLine(FmtChunk.Describe());
        stringBuilder.AppendLine(DataChunk.Describe());
        return stringBuilder.ToString();
    }
}