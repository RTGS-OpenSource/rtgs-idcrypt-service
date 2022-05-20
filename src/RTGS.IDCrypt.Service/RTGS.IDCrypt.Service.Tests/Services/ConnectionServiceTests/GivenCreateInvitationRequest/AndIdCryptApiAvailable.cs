using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenCreateInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly ConnectionService _connectionService;
	private readonly Models.ConnectionInvitation _expectedResponse;
	private readonly string _alias = "alias";
	private Models.ConnectionInvitation _actualResponse;

	public AndIdCryptApiAvailable()
	{
		_expectedResponse = new Models.ConnectionInvitation
		{
			InvitationUrl = "invitation-url",
			ImageUrl = "image-url",
			Did = "did",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key-1" },
			ServiceEndpoint = "service-endpoint",
			Id = "id",
			Type = "type"
		};

		var createConnectionInvitationResponse = new CreateConnectionInvitationResponse
		{
			ConnectionId = "connection-id",
			Alias = "alias",
			InvitationUrl = "invitation-url",
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				ImageUrl = "image-url",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint",
				Did = "did"
			}
		};

		_connectionsClientMock
			.Setup(client => client.CreateConnectionInvitationAsync(
				_alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(createConnectionInvitationResponse)
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		var aliasProviderMock = new Mock<IAliasProvider>();
		aliasProviderMock.Setup(provider => provider.Provide()).Returns(_alias);

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			Mock.Of<IConnectionRepository>(),
			aliasProviderMock.Object,
			Mock.Of<IWalletClient>()
		);
	}

	public async Task InitializeAsync() =>
		_actualResponse = await _connectionService.CreateConnectionInvitationAsync();

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenExpectedResponse() => _actualResponse.Should().BeEquivalentTo(_expectedResponse);

	[Fact]
	public void WhenInvoked_ThenCallReceiveAndAcceptInvitationAsyncWithExpected() => _connectionsClientMock.Verify();
}
