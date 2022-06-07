using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenDeleteAsyncRequest;

public class AndConnectionDoesNotExist : IAsyncLifetime
{
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly Mock<TableClient> _tableClientMock;
	private const string ConnectionId = "connection-id-999";

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

		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<Service.Repositories.BankPartnerConnectionRepository>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository = new Service.Repositories.BankPartnerConnectionRepository(
			storageTableResolverMock.Object,
			options,
			logger,
			Mock.Of<IDateTimeProvider>());
	}

	public async Task InitializeAsync() => await _bankPartnerConnectionRepository.DeleteAsync(ConnectionId);

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenNoDeleteAttemptIsMade() => _tableClientMock
		.Verify(client => client.DeleteEntityAsync(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<ETag>(),
			It.IsAny<CancellationToken>()), Times.Never);
}
