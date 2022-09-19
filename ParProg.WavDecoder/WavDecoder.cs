using System.ComponentModel;
using System.Text;

namespace ParProg.WavDecoder;

public class WavDecoder : IDisposable
{
    private FileStream _stream;
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

    public float[] Decode()
    {
        var bytesPerSample = Header.bitsPerSample / 8;
        var buffer = new byte[bytesPerSample];
        var result = new List<float>();
        while (true)
        {
            var readed = _stream.Read(buffer, 0, bytesPerSample);
            if (readed == 0)
                break;
            switch (Header.bitsPerSample)
            {
                case 16: 
                    result.Add(BitConverter.ToInt16(buffer));
                    break;
                case 32: 
                    result.Add(BitConverter.ToInt32(buffer));
                    break;
                case 64: 
                    result.Add(BitConverter.ToInt64(buffer));
                    break;
            }
        }

        return result.ToArray();
    }

    private void ReadHeader(FileStream stream)
    {
        Header = new WavHeader(stream);
    }
    
    public string Describe()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"FileName: {_stream.Name}");
        stringBuilder.AppendLine($"Headers: ");
        stringBuilder.AppendLine(Header.Describe());
        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}