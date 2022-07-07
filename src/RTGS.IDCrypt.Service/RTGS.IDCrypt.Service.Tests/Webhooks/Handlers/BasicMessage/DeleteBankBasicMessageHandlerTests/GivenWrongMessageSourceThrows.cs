using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteBankBasicMessageHandlerTests;

public class GivenWrongMessageSourceThrows : IAsyncLifetime
{
	private const string RtgsGlobalIdToDelete = "rtgs-global-id";
	private const string MessageSource = "Bob";

	private readonly Mock<IBankConnectionService> _mockBankConnectionService = new();
	private readonly Mock<IRtgsConnectionService> _mockRtgsConnectionService = new();
	private DeleteBankBasicMessageHandler _handler;
	private string _message;

	public Task InitializeAsync()
	{
		_handler = new DeleteBankBasicMessageHandler(_mockBankConnectionService.Object, _mockRtgsConnectionService.Object);

		var messageContent = new DeleteBankRequest(RtgsGlobalIdToDelete);
		_message = JsonSerializer.Serialize(new BasicMessageContent<DeleteBankRequest>
		{
			MessageContent = messageContent,
			Source = MessageSource
		});

		return Task.CompletedTask;
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task ThenExceptionThrown() =>
		await FluentActions
			.Awaiting(() => _handler.HandleAsync(_message, "connection-id"))
			.Should()
			.ThrowAsync<InvalidMessageSourceException>();
}
