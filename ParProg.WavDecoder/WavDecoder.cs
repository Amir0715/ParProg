using System.ComponentModel;
using System.Text;

namespace ParProg.WavDecoder;

public class WavDecoder : IDisposable
{
    private FileStream _stream;
    private delegate float Converter(byte[] buffer);

    public WavHeader Header { get; private set; }

    public WavDecoder(FileStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead) throw new ArgumentException($"{nameof(stream)} should be readeble");
        
        ReadHeader(_stream); 
    }

    public WavDecoder(string filePath)
    {
        _stream = File.Open(filePath, FileMode.Open);
        ReadHeader(_stream);
    }

    public List<float> Decode()
    {
        var bytesPerSample = Header.FmtChunk.bitsPerSample / 8;
        var result = new List<float>(Header.DataChunk.chunkSize / bytesPerSample);
        var i = 0;
        Converter converter;
        switch (Header.FmtChunk.bitsPerSample)
        {
            case 16:
                converter = buffer => BitConverter.ToInt16(buffer);
                break;
            case 32:
                converter = buffer => BitConverter.ToInt32(buffer);
                break;
            case 64:
                converter = buffer => BitConverter.ToInt64(buffer);
                break;
            default:
                throw new NotImplementedException();
        }

        while (i < Header.DataChunk.data.Count - bytesPerSample)
        {
            var buffer = Header.DataChunk.data.Skip(i).Take(bytesPerSample).ToArray();
            result.Add(converter(buffer));
            i += bytesPerSample;
        }

        return result;
    }

    private void ReadHeader(FileStream stream)
    {
        Header = new WavHeader(stream);
    }
    
    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"FileName: {_stream.Name}");
        stringBuilder.AppendLine("Headers: ");
        stringBuilder.AppendLine(Header.Describe());
        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}