using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.RtgsConnectionInvitationBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock = new();

	public async Task InitializeAsync()
	{
		var connectionInvitation = new RtgsConnectionInvitation
		{
			InvitationUrl = "invitation-url",
			ImageUrl = "image-url",
			Did = "did",
			PublicDid = "public-did",
			Alias = "alias",
			RecipientKeys = new[] { "recipient-key-1" },
			Label = "label",
			ServiceEndpoint = "service-endpoint",
			Id = "id",
			Type = "type"
		};

		Func<RtgsConnectionInvitation, bool> invitationMatches = invitation =>
		{
			invitation.Should().BeEquivalentTo(connectionInvitation);
			return true;
		};

		_connectionServiceMock
			.Setup(service => service.AcceptRtgsInvitationAsync(
				It.Is<RtgsConnectionInvitation>(invitation => invitationMatches(invitation)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new RtgsConnectionInvitationBasicMessageHandler(_connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new BasicMessageContent<RtgsConnectionInvitation> { MessageContent = connectionInvitation });

		await handler.HandleAsync(message, "connection-id");
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsAcceptInvitationAsync() => _connectionServiceMock.Verify();
}
