using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndIdCryptGetPublicDidApiUnavailable
{
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly ConnectionController _connectionController;
	private const string Alias = "alias";

	public AndIdCryptGetPublicDidApiUnavailable()
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		var connectionsClientMock = new Mock<IConnectionsClient>();

		var createInvitationResponse = new CreateInvitationResponse
		{
			ConnectionId = "connection-id",
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint"
			}
		};

		connectionsClientMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				Alias,
				autoAccept,
				multiUse,
				usePublicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(createInvitationResponse)
			.Verifiable();

		var walletClientMock = new Mock<IWalletClient>();

		walletClientMock
			.Setup(walletClient => walletClient.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		var mockAliasProvider = new Mock<IAliasProvider>();

		mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(Alias);

		_logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			_logger,
			connectionsClientMock.Object,
			walletClientMock.Object,
			mockAliasProvider.Object,
			Mock.Of<IStorageTableResolver>());
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent"
			});
	}
}
