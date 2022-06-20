namespace RTGS.IDCrypt.Service.Scheduler;

[Serializable]
public class BankConnectionCycleException : Exception
{
	public BankConnectionCycleException() : base()
	{
	}

	public BankConnectionCycleException(string message) : base(message)
	{
	}

	public BankConnectionCycleException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
