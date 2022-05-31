using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
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
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();

	public AndIdCryptApiAvailable()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

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
			PublicDid = "public-did",
			FromRtgsGlobalId = "rtgs-global-id"
		};

		var expectedReceiveAndAcceptRequest = new ReceiveAndAcceptInvitationRequest
		{
			Alias = _request.Alias,
			Id = _request.Id,
			Label = _request.Label,
			RecipientKeys = _request.RecipientKeys,
			ServiceEndpoint = _request.ServiceEndpoint,
			Type = _request.Type
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = actualReceiveAndAcceptRequest =>
		{
			actualReceiveAndAcceptRequest.Should().BeEquivalentTo(expectedReceiveAndAcceptRequest);

			return true;
		};

		_connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionResponse)
			.Verifiable();

		var expectedConnection = new BankPartnerConnection
		{
			PartitionKey = _request.FromRtgsGlobalId,
			RowKey = connectionResponse.Alias,
			ConnectionId = connectionResponse.ConnectionId,
			Alias = connectionResponse.Alias,
			PublicDid = _request.PublicDid,
			Status = "Pending",
			Role = "Invitee",
		};

		Func<BankPartnerConnection, bool> connectionMatches = actualConnection =>
		{
			actualConnection.Should().BeEquivalentTo(expectedConnection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_bankPartnerConnectionRepositoryMock
			.Setup(service => service.CreateAsync(
				It.Is<BankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions);
	}

	public async Task InitializeAsync() =>
		await _connectionService.AcceptInvitationAsync(_request);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallSaveBankPartnerConnectionAsyncWithExpected() => _bankPartnerConnectionRepositoryMock.Verify();
}
