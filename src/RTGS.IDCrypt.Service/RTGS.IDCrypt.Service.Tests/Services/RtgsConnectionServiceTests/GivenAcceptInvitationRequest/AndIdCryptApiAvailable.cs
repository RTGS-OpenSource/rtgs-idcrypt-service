using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.RtgsConnectionServiceTests.GivenAcceptInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly RtgsConnectionService _rtgsConnectionService;
	private readonly RtgsConnectionInvitation _request;
	private readonly Mock<IRtgsConnectionRepository> _rtgsConnectionRepositoryMock = new();

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

		_request = new RtgsConnectionInvitation
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

		var expectedConnection = new RtgsConnection
		{
			PartitionKey = connectionResponse.Alias,
			RowKey = connectionResponse.ConnectionId,
			ConnectionId = connectionResponse.ConnectionId,
			Alias = connectionResponse.Alias,
			Status = "Pending",
		};

		Func<RtgsConnection, bool> connectionMatches = actualConnection =>
		{
			actualConnection.Should().BeEquivalentTo(expectedConnection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_rtgsConnectionRepositoryMock
			.Setup(service => service.CreateAsync(
				It.Is<RtgsConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<RtgsConnectionService>();

		_rtgsConnectionService = new RtgsConnectionService(
			_connectionsClientMock.Object,
			logger,
			_rtgsConnectionRepositoryMock.Object,
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions);
	}

	public async Task InitializeAsync() =>
		await _rtgsConnectionService.AcceptInvitationAsync(_request);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallRepositoryCreateAsyncWithExpected() => _rtgsConnectionRepositoryMock.Verify();
}
