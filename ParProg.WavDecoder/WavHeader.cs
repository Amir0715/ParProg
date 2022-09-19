using System.Text;

namespace ParProg.WavDecoder;

public class WavHeader
{
    public int chunkId { get; }
    public int chunkSize { get; }
    public int format { get; }
    public int subchunk1Id { get; }
    public int subchunk1Size { get; }
    public short audioFormat { get; }
    public short numChannels { get; }
    public int sampleRate { get; }
    public int byteRate { get; }
    public short blockAlign { get; }
    public short bitsPerSample { get; }
    public int subchunk2Id { get; }
    public int subchunk2Size { get; }

    public WavHeader(FileStream fileStream)
    {
        var binaryReader = new BinaryReader(fileStream);

        chunkId = binaryReader.ReadInt32();
        chunkSize = binaryReader.ReadInt32();
        format = binaryReader.ReadInt32();
        subchunk1Id = binaryReader.ReadInt32();
        subchunk1Size = binaryReader.ReadInt32();

        audioFormat = binaryReader.ReadInt16();
        numChannels = binaryReader.ReadInt16();

        sampleRate = binaryReader.ReadInt32();
        byteRate = binaryReader.ReadInt32();

        blockAlign = binaryReader.ReadInt16();
        bitsPerSample = binaryReader.ReadInt16();

        subchunk2Id = binaryReader.ReadInt32();
        subchunk2Size = binaryReader.ReadInt32();
    }

    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"ChunkId: {chunkId}");
        stringBuilder.AppendLine($"ChunkSize: {chunkSize}");
        stringBuilder.AppendLine($"Format: {format}");
        stringBuilder.AppendLine($"Subchunk1Id: {subchunk1Id}");
        stringBuilder.AppendLine($"Subchunk1Size: {subchunk1Size}");
        stringBuilder.AppendLine($"AudioFormat: {audioFormat}");
        stringBuilder.AppendLine($"NumChannels: {numChannels}");
        stringBuilder.AppendLine($"SampleRate: {sampleRate}");
        stringBuilder.AppendLine($"ByteRate: {byteRate}");
        stringBuilder.AppendLine($"BlockAlign: {blockAlign}");
        stringBuilder.AppendLine($"BitsPerSample: {bitsPerSample}");
        stringBuilder.AppendLine($"Subchunk2Id: {subchunk2Id}");
        stringBuilder.AppendLine($"Subchunk2Size: {subchunk2Size}");
        return stringBuilder.ToString();
    }
}