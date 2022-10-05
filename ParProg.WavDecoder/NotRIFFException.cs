using System.Runtime.Serialization;

namespace ParProg.WavDecoder;

public class NotRIFFException : Exception
{
    public NotRIFFException()
    {
    }

    protected NotRIFFException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public NotRIFFException(string? message) : base(message)
    {
    }

    public NotRIFFException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}