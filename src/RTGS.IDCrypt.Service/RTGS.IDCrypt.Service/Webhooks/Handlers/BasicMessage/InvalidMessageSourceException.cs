using System.Runtime.Serialization;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public class InvalidMessageSourceException : Exception
{
	public InvalidMessageSourceException()
	{
	}

	public InvalidMessageSourceException(string message) : base(message)
	{
	}

	public InvalidMessageSourceException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected InvalidMessageSourceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
