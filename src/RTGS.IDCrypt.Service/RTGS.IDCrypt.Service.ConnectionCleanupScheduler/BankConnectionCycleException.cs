using System.Runtime.Serialization;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler;

[Serializable]
public class BankConnectionCycleException : Exception
{
	public BankConnectionCycleException()
	{
	}

	public BankConnectionCycleException(string message) : base(message)
	{
	}

	public BankConnectionCycleException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected BankConnectionCycleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
