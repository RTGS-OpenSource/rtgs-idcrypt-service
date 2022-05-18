using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.ConnectionInvitationBasicMessageHandlerTests;

public class GivenConnectionServiceThrows
{
	[Fact]
	public async Task ThenThrows()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.IsAny<ConnectionInvitation>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var handler = new ConnectionInvitationBasicMessageHandler(connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new ConnectionInvitation());

		await FluentActions
			.Awaiting(() => handler.HandleAsync(message))
			.Should()
			.ThrowAsync<Exception>();
	}
}
