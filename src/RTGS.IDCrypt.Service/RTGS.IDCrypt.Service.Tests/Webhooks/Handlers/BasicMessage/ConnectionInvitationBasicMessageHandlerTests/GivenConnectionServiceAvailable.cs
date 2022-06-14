using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.ConnectionInvitationBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionService> _connectionServiceMock = new();

	public async Task InitializeAsync()
	{
		var connectionInvitation = new ConnectionInvitation
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
			Type = "type",
			FromRtgsGlobalId = "rtgs-global-id"
		};

		Func<ConnectionInvitation, bool> invitationMatches = invitation =>
		{
			invitation.Should().BeEquivalentTo(connectionInvitation);
			return true;
		};

		_connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.Is<ConnectionInvitation>(invitation => invitationMatches(invitation)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new ConnectionInvitationBasicMessageHandler(_connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new BasicMessageContent<ConnectionInvitation> { MessageContent = connectionInvitation });

		await handler.HandleAsync(message);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsAcceptInvitationAsync() => _connectionServiceMock.Verify();
}
