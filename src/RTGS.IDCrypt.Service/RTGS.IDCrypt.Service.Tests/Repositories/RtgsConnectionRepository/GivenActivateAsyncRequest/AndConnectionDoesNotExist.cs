using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.RtgsConnectionRepository.GivenActivateAsyncRequest;

public class AndConnectionDoesNotExist : IAsyncLifetime
{
	private readonly Service.Repositories.RtgsConnectionRepository _rtgsConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private readonly FakeLogger<Service.Repositories.RtgsConnectionRepository> _logger;

	public AndConnectionDoesNotExist()
	{
		var retrievedConnection = new RtgsConnection
		{
			PartitionKey = "alias",
			RowKey = "connection-id",
			ConnectionId = "connection-id",
			Alias = "alias",
			Status = "Pending"
		};

		var rtgsConnectionMock = new Mock<Pageable<RtgsConnection>>();

		rtgsConnectionMock.Setup(rtgsConnections => rtgsConnections.GetEnumerator())
			.Returns(new List<RtgsConnection>().GetEnumerator());

		_tableClientMock = new Mock<TableClient>();

		Func<Expression<Func<RtgsConnection, bool>>, bool> expressionMatches = actualExpression =>
		{
			Expression<Func<RtgsConnection, bool>> expectedExpression = rtgsConnection =>
				rtgsConnection.ConnectionId == "non-existent-connection-id";

			actualExpression.Should().BeEquivalentTo(expectedExpression);

			return true;
		};

		_tableClientMock.Setup(tableClient =>
				tableClient.Query(
					It.Is<Expression<Func<RtgsConnection, bool>>>(expression => expressionMatches(expression)),
					It.IsAny<int?>(),
					It.IsAny<IEnumerable<string>>(),
					It.IsAny<CancellationToken>()))
			.Returns(rtgsConnectionMock.Object);

		_storageTableResolverMock = new Mock<IStorageTableResolver>();

		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("rtgsConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		_logger = new FakeLogger<Service.Repositories.RtgsConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			RtgsConnectionsTableName = "rtgsConnections"
		});

		_rtgsConnectionRepository = new Service.Repositories.RtgsConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			_logger,
			Mock.Of<IDateTimeProvider>());
	}

	public async Task InitializeAsync() => await _rtgsConnectionRepository.ActivateAsync("non-existent-connection-id");

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenNoUpdateAttemptIsMade() => _tableClientMock
		.Verify(client => client.UpdateEntityAsync(
			It.IsAny<BankPartnerConnection>(),
			It.IsAny<ETag>(),
			It.IsAny<TableUpdateMode>(),
			It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void ThenLog() =>
		_logger.Logs[LogLevel.Warning]
			.Should().BeEquivalentTo("Unable to activate connection as the connection was not found");
}
