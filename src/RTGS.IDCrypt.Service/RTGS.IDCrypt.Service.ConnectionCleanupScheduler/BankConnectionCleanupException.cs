using System.Runtime.Serialization;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler;

[Serializable]
public class BankConnectionCleanupException : Exception
{
	public BankConnectionCleanupException()
	{
	}

	public BankConnectionCleanupException(string message) : base(message)
	{
	}

	public BankConnectionCleanupException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected BankConnectionCleanupException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
