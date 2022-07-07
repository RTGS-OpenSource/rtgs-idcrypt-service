using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.BankConnectionInvitationBasicMessageHandlerTests;

public class GivenConnectionServiceAvailable : IAsyncLifetime
{
	private readonly Mock<IBankConnectionService> _bankConnectionServiceMock = new();

	public async Task InitializeAsync()
	{
		var connectionInvitation = new BankConnectionInvitation
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

		Func<BankConnectionInvitation, bool> invitationMatches = invitation =>
		{
			invitation.Should().BeEquivalentTo(connectionInvitation);
			return true;
		};

		_bankConnectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.Is<BankConnectionInvitation>(invitation => invitationMatches(invitation)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new BankConnectionInvitationBasicMessageHandler(_bankConnectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new BasicMessageContent<BankConnectionInvitation> { MessageContent = connectionInvitation });

		await handler.HandleAsync(message, "connection-id");
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenCallsAcceptInvitation() => _bankConnectionServiceMock.Verify();
}
