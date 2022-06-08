﻿using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenGetInvitedPartnerIdsRequest;

public class AndTableStorageAvailable : IAsyncLifetime
{
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private IEnumerable<string> _ids;
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;

	public AndTableStorageAvailable()
	{
		var connection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-1",
			ConnectionId = "connection-id-1",
			CreatedAt = DateTime.Parse("2022-01-01"),
			Status = "Active",
			Role = "Invitee"
		};

		var bankPartnerConnectionMock = new Mock<AsyncPageable<BankPartnerConnection>>();
		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(new List<BankPartnerConnection> { connection }.ToAsyncEnumerable().GetAsyncEnumerator());

		_tableClientMock = new Mock<TableClient>();

		Func<Expression<Func<BankPartnerConnection, bool>>, bool> expressionMatches = actualExpression =>
		{
			Expression<Func<BankPartnerConnection, bool>> expectedExpression = bankPartnerConnection =>
				bankPartnerConnection.Status == "Active" && bankPartnerConnection.Role == "Invitee";

			actualExpression.Should().BeEquivalentTo(expectedExpression);

			return true;
		};

		_tableClientMock.Setup(tableClient =>
				tableClient.QueryAsync(
					It.Is<Expression<Func<BankPartnerConnection, bool>>>(expression => expressionMatches(expression)),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionMock.Object);

		_storageTableResolverMock = new Mock<IStorageTableResolver>();
		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.BankPartnerConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository = new Service.Repositories.BankPartnerConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			logger,
			Mock.Of<IDateTimeProvider>());
	}

	public async Task InitializeAsync() => _ids = await _bankPartnerConnectionRepository.GetInvitedPartnerIdsAsync();

	[Fact]
	public void ThenExpectedTableIsResolved() => _storageTableResolverMock.Verify();

	[Fact]
	public void ThenCorrectQuery() => _tableClientMock.Verify();

	[Fact]
	public void ThenExpectedResult() => _ids.Should().BeEquivalentTo("rtgs-global-id-1");

	public Task DisposeAsync() => Task.CompletedTask;
}