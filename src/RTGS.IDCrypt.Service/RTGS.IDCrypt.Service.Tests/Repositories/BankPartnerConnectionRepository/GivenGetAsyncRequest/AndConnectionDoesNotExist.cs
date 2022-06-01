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

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenGetAsyncRequest;

public class AndConnectionDoesNotExist
{
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly Mock<IStorageTableResolver> _storageTableResolverMock;
	private readonly Mock<TableClient> _tableClientMock;
	private readonly FakeLogger<Service.Repositories.BankPartnerConnectionRepository> _logger;

	private const string ConnectionId = "non-existent-connection-id";

	public AndConnectionDoesNotExist()
	{
		var bankPartnerConnectionMock = new Mock<AsyncPageable<BankPartnerConnection>>();

		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(new List<BankPartnerConnection>().ToAsyncEnumerable().GetAsyncEnumerator());

		_tableClientMock = new Mock<TableClient>();

		Func<Expression<Func<BankPartnerConnection, bool>>, bool> expressionMatches = actualExpression =>
		{
			Expression<Func<BankPartnerConnection, bool>> expectedExpression = bankPartnerConnection =>
				bankPartnerConnection.ConnectionId == ConnectionId;

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

		_logger = new FakeLogger<Service.Repositories.BankPartnerConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository = new Service.Repositories.BankPartnerConnectionRepository(
			_storageTableResolverMock.Object,
			options,
			_logger,
			Mock.Of<IDateTimeProvider>());
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() => await FluentActions
		.Awaiting(() => _bankPartnerConnectionRepository.GetAsync(ConnectionId))
		.Should()
		.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _bankPartnerConnectionRepository.GetAsync("non-existent-connection-id"))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error]
			.Should().BeEquivalentTo($"Connection with ID {ConnectionId} not found");
	}
}
