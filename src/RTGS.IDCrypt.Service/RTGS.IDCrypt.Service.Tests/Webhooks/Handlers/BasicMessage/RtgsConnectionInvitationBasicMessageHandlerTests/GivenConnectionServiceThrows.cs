using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models.ConnectionInvitations;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;


namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.RtgsConnectionInvitationBasicMessageHandlerTests;

public class GivenConnectionServiceThrows
{
	[Fact]
	public async Task ThenThrows()
	{
		var rtgsConnectionServiceMock = new Mock<IRtgsConnectionService>();

		rtgsConnectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.IsAny<RtgsConnectionInvitation>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var handler = new RtgsConnectionInvitationBasicMessageHandler(rtgsConnectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new RtgsConnectionInvitation());

		await FluentActions
			.Awaiting(() => handler.HandleAsync(message, "connection-id"))
			.Should()
			.ThrowAsync<Exception>();
	}
}
