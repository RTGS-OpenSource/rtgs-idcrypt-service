using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCrypt.Service.Webhooks.Models.BasicMessageModels;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteBankPartnerConnectionBasicMessageHandlerTests;

public class GivenConnectionServiceThrows
{
	[Fact]
	public async Task ThenThrows()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.DeleteAsync(
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var handler = new DeleteBankPartnerConnectionBasicMessageHandler(connectionServiceMock.Object);

		var message = JsonSerializer.Serialize(new BasicMessageContent<DeleteBankPartnerConnectionBasicMessage>());

		await FluentActions
			.Awaiting(() => handler.HandleAsync(message, "connection-id"))
			.Should()
			.ThrowAsync<Exception>();
	}
}
