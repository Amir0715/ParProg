using System.Text;

namespace ParProg.WavDecoder.Chunks;

public class FmtChunk : IChunk
{
    public string chunkId { get; }
    public int chunkSize { get; }
    public short audioFormat { get; }
    public short numChannels { get; }
    public int sampleRate { get; }
    public int byteRate { get; }
    public short blockAlign { get; }
    public short bitsPerSample { get; }

    public short extraFormatBytesSize { get; }
    public byte[] extraFormatBytes { get; }

    public FmtChunk(FileStream fileStream)
    {
        var binaryReader = new BinaryReader(fileStream);

        chunkId = "fmt ";
        chunkSize = binaryReader.ReadInt32();

        audioFormat = binaryReader.ReadInt16();
        numChannels = binaryReader.ReadInt16();

        sampleRate = binaryReader.ReadInt32();
        byteRate = binaryReader.ReadInt32();

        blockAlign = binaryReader.ReadInt16();
        bitsPerSample = binaryReader.ReadInt16();
        
        if (chunkSize <= 16) return;
        
        extraFormatBytesSize = binaryReader.ReadInt16();

        if (extraFormatBytesSize > 0)
        {
            extraFormatBytes = binaryReader.ReadBytes(extraFormatBytesSize);
        }
    }

    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"ChunkId: {chunkId}");
        stringBuilder.AppendLine($"ChunkSize: {chunkSize}");
        stringBuilder.AppendLine($"AudioFormat: {audioFormat}");
        stringBuilder.AppendLine($"NumChannels: {numChannels}");
        stringBuilder.AppendLine($"SampleRate: {sampleRate}");
        stringBuilder.AppendLine($"ByteRate: {byteRate}");
        stringBuilder.AppendLine($"BlockAlign: {blockAlign}");
        stringBuilder.Append($"BitsPerSample: {bitsPerSample}");
        return stringBuilder.ToString();
    }
}