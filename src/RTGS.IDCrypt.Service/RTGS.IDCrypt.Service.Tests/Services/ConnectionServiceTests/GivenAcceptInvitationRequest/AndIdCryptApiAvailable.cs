using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenAcceptInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Models.ConnectionInvitation _request;
	private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();

	public AndIdCryptApiAvailable()
	{
		var connectionResponse = new ConnectionResponse
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "invitation"
		};

		_request = new Models.ConnectionInvitation
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			InvitationUrl = "invitation-url",
			Did = "did",
			ImageUrl = "image-url",
			PublicDid = "public-did"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(_request, options =>
			{
				options.Excluding(connection => connection.PublicDid);
				options.Excluding(connection => connection.ImageUrl);
				options.Excluding(connection => connection.Did);
				options.Excluding(connection => connection.InvitationUrl);

				return options;
			});

			return true;
		};

		_connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionResponse)
			.Verifiable();

		var expectedPendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = connectionResponse.ConnectionId,
			RowKey = connectionResponse.Alias,
			ConnectionId = connectionResponse.ConnectionId,
			Alias = connectionResponse.Alias,
			PublicDid = _request.PublicDid,
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

		_connectionRepositoryMock
			.Setup(service => service.SavePendingBankPartnerConnectionAsync(
				It.Is<PendingBankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			_connectionRepositoryMock.Object,
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>()
			);
	}

	public async Task InitializeAsync() =>
		await _connectionService.AcceptInvitationAsync(_request);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallSavePendingBankPartnerConnectionAsyncWithExpected() => _connectionRepositoryMock.Verify();
}
