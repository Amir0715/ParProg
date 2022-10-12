﻿using System.Text;

namespace ParProg.WavDecoder.Chunks;

public class DataChunk : IChunk
{
    public string chunkId { get; }
    public int chunkSize { get; }
    
    public byte[] data { get; }

    public DataChunk(FileStream fileStream)
    {
        var binaryReader = new BinaryReader(fileStream);
        chunkId = "data";
        chunkSize = binaryReader.ReadInt32();
        data = binaryReader.ReadBytes(chunkSize);
    }

    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"ChunkId: {chunkId}");
        stringBuilder.AppendLine($"ChunkSize: {chunkSize}");
        stringBuilder.Append($"DataLenght: {data.Length}");
        return stringBuilder.ToString();
    }
}