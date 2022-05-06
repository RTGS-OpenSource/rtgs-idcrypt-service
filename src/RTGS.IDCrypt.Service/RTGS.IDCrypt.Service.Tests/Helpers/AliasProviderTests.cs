using System;
using RTGS.IDCrypt.Service.Helpers;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Helpers;

public class AliasProviderTests
{
	[Fact]
	public void Provide_ProvidesNewGuid()
	{
		var guidProvider = new AliasProvider();

		var guid = guidProvider.Provide();

		Guid.Parse(guid);
	}
}
