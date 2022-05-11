using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Helpers.BankPartnerConnectionResolverTests;

public class GivenSingleMatchingConnection
{
	[Fact]
	public void WhenResolveIsCalled_ThenShouldReturnBankPartnerConnection()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Timestamp = DateTime.Now
			}
		};

		var bankPartnerConnectionsConfig = new BankPartnerConnectionsConfig
		{
			MinimumConnectionAge = TimeSpan.FromMinutes(5)
		};

		var dateTimeOffsetProviderMock = new Mock<IDateTimeOffsetProvider>();
		dateTimeOffsetProviderMock.SetupGet(provider => provider.Now).Returns(DateTimeOffset.Now);

		var sut = new BankPartnerConnectionResolver(Options.Create(bankPartnerConnectionsConfig), dateTimeOffsetProviderMock.Object);

		var connection = sut.Resolve(bankPartnerConnections);

		connection.Should().BeEquivalentTo(bankPartnerConnections.Single());
	}
}
