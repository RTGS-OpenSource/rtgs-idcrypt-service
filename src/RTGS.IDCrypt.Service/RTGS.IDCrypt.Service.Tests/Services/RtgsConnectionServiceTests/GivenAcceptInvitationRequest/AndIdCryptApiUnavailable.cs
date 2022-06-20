using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.RtgsConnectionServiceTests.GivenAcceptInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly RtgsConnectionService _rtgsConnectionService;
	private readonly RtgsConnectionInvitation _request;
	private readonly FakeLogger<RtgsConnectionService> _logger = new();

	public AndIdCryptApiUnavailable()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		var connectionsClientMock = new Mock<IConnectionsClient>();

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

		connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.IsAny<ReceiveAndAcceptInvitationRequest>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_rtgsConnectionService = new RtgsConnectionService(
			connectionsClientMock.Object,
			_logger,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>(),
			coreOptions);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _rtgsConnectionService.AcceptInvitationAsync(_request))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _rtgsConnectionService.AcceptInvitationAsync(_request))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when accepting RTGS invitation");
	}
}
