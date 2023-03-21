using System.Runtime.Serialization;

namespace OPI.WebAPI;

public class DataModelValidationException : Exception
{
    public DataModelValidationException()
    {
    }

    public DataModelValidationException(string? message) : base(message)
    {
    }

    public DataModelValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DataModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}