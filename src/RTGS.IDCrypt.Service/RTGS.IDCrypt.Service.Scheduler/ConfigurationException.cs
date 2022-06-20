namespace RTGS.IDCrypt.Service.Scheduler;

[Serializable]
public class ConfigurationException : Exception
{
	public ConfigurationException() : base()
	{
	}

	public ConfigurationException(string message) : base(message)
	{
	}

	public ConfigurationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
