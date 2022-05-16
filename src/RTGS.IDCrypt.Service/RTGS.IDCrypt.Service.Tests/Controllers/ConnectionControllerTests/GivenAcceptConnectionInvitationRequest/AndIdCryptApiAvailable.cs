using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly ConnectionController _connectionController;
	private readonly Mock<TableClient> _tableClientMock;
	private readonly Mock<IStorageTableResolver> _storageTableResolver;

	private IActionResult _response;

	public AndIdCryptApiAvailable()
	{
		_connectionsClientMock = new Mock<IConnectionsClient>();

		var connectionResponse = new ConnectionResponse
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "invitation"
		};

		var expectedRequest = new ReceiveAndAcceptInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedRequest);

			return true;
		};

		_connectionsClientMock
			.Setup(connectionsClient => connectionsClient.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionResponse)
			.Verifiable();

		var expectedPendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = connectionResponse.ConnectionId,
			RowKey = connectionResponse.Alias,
			ConnectionId = connectionResponse.ConnectionId,
			Alias = connectionResponse.Alias
		};

		Func<PendingBankPartnerConnection, bool> connectionMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedPendingConnection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_tableClientMock = new Mock<TableClient>();
		_tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.Is<PendingBankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_storageTableResolver = new Mock<IStorageTableResolver>();
		_storageTableResolver
			.Setup(resolver => resolver.GetTable("pendingBankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		var logger = new FakeLogger<ConnectionController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			PendingBankPartnerConnectionsTableName = "pendingBankPartnerConnections"
		});

		_connectionController = new ConnectionController(
			logger,
			_connectionsClientMock.Object,
			Mock.Of<IWalletClient>(),
			Mock.Of<IAliasProvider>(),
			_storageTableResolver.Object,
			options);
	}

	public async Task InitializeAsync()
	{
		var request = new AcceptConnectionInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			AgentPublicDid = "agent-public-did"
		};

		_response = await _connectionController.Accept(request, default);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnAccepted() =>
		_response.Should().BeOfType<AcceptedResult>();

	[Fact]
	public void WhenPosting_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() =>
		_connectionsClientMock.Verify();

	[Fact]
	public void ThenExpectedTableIsResolved() =>
		_storageTableResolver.Verify();

	[Fact]
	public void ThenConnectionIsWritten() =>
		_tableClientMock.Verify();
}
