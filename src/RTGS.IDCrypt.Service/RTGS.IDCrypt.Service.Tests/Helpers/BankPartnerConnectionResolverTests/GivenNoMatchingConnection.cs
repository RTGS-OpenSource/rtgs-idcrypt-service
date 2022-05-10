using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Helpers.BankPartnerConnectionResolverTests;

public class GivenNoMatchingConnection
{
	[Fact]
	public void WhenResolveIsCalled_ThenShouldReturnNull()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>();

		var bankPartnerConnectionsConfig = new BankPartnerConnectionsConfig
		{
			GracePeriod = TimeSpan.FromMinutes(5)
		};
		var sut = new BankPartnerConnectionResolver(Options.Create(bankPartnerConnectionsConfig));

		var connection = sut.Resolve(bankPartnerConnections);

		connection.Should().BeNull();
	}
}
