using RTGS.IDCrypt.Service.Helpers;

namespace RTGS.IDCrypt.Service.Tests.Helpers;

public class AliasProviderTests
{
	[Fact]
	public void Provide_ProvidesNewGuid()
	{
		var aliasProvider = new AliasProvider();

		var alias = aliasProvider.Provide();

		Guid.Parse(alias);
	}
}
