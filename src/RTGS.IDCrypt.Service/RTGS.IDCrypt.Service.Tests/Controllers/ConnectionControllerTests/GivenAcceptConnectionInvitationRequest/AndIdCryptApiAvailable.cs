using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock;
	private readonly ConnectionController _connectionController;
	private readonly Mock<IConnectionStorageService> _connectionStorageServiceMock;

	private IActionResult _response;

	public AndIdCryptApiAvailable()
	{
		_connectionServiceMock = new Mock<IConnectionService>();

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

		_connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
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

		_connectionStorageServiceMock = new Mock<IConnectionStorageService>();
		_connectionStorageServiceMock
			.Setup(service => service.SavePendingBankPartnerConnectionAsync(
				It.Is<PendingBankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			logger,
			Mock.Of<IWalletClient>(),
			Mock.Of<IAliasProvider>(),
			_connectionServiceMock.Object,
			_connectionStorageServiceMock.Object);
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
		_connectionServiceMock.Verify();

	[Fact]
	public void ThenConnectionIsWritten() =>
		_connectionStorageServiceMock.Verify();
}
