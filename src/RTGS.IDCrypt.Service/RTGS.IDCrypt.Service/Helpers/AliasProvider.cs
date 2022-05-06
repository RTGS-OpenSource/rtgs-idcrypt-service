namespace RTGS.IDCrypt.Service.Helpers;

public class AliasProvider : IAliasProvider
{
	public string Provide() =>
		Guid.NewGuid().ToString();
}
