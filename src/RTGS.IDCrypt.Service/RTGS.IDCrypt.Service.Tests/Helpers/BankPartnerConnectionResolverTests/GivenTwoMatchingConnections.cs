using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Helpers.BankPartnerConnectionResolverTests;

public class GivenMultipleMatchingConnections
{
	private readonly TimeSpan _gracePeriod = TimeSpan.FromMinutes(5);
	private readonly BankPartnerConnectionResolver _sut;

	public GivenMultipleMatchingConnections()
	{
		DateTimeOffsetServer.Init(() => new DateTimeOffset(2022, 1, 1, 0, 0, 0, new TimeSpan()));

		var bankPartnerConnectionsConfig = new BankPartnerConnectionsConfig
		{
			GracePeriod = TimeSpan.FromMinutes(5)
		};
		_sut = new BankPartnerConnectionResolver(Options.Create(bankPartnerConnectionsConfig));
	}

	[Fact]
	public void WhenAllAreOlderThanGracePeriod_AndResolveIsCalled_ThenShouldReturnLatestBankPartnerConnection()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod)
			},
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-2",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod.Add(TimeSpan.FromMinutes(1)))
			}
		};

		var connection = _sut.Resolve(bankPartnerConnections);

		connection.ConnectionId.Should().Be("connection-id-1");
	}

	[Fact]
	public void WhenOneIsYoungerThanGracePeriod_AndResolveIsCalled_ThenShouldReturnLatestBankPartnerConnectionOlderThanGracePeriod()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod).Add(TimeSpan.FromMinutes(1))
			},
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-2",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod)
			},
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-3",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod.Add(TimeSpan.FromMinutes(1)))
			}
		};

		var connection = _sut.Resolve(bankPartnerConnections);

		connection.ConnectionId.Should().Be("connection-id-2");
	}

	[Fact]
	public void WhenAllAreYoungerThanGracePeriod_AndResolveIsCalled_ThenShouldReturnOldestBankPartnerConnection()
	{
		var bankPartnerConnections = new List<BankPartnerConnection>
		{
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod).Add(TimeSpan.FromMinutes(2))
			},
			new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-2",
				Timestamp = DateTimeOffsetServer.Now.Subtract(_gracePeriod).Add(TimeSpan.FromMinutes(1))
			}
		};

		var connection = _sut.Resolve(bankPartnerConnections);

		connection.ConnectionId.Should().Be("connection-id-2");
	}
}
