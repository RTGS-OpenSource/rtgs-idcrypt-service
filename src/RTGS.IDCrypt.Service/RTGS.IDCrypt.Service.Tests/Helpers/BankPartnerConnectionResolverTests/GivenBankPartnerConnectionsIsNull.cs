using System;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Helpers.BankPartnerConnectionResolverTests;

public class GivenBankPartnerConnectionsIsNull
{
	[Fact]
	public void WhenResolveIsCalled_ThenShouldReturnNull()
	{
		var bankPartnerConnectionsConfig = new BankPartnerConnectionsConfig
		{
			GracePeriod = TimeSpan.FromMinutes(5)
		};

		var sut = new BankPartnerConnectionResolver(Options.Create(bankPartnerConnectionsConfig));

		FluentActions.Invoking(() => sut.Resolve(null))
			.Should()
			.ThrowExactly<ArgumentNullException>()
			.WithMessage("*bankPartnerConnections*");
	}
}
