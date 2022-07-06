using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;
using RTGSIDCryptWorker.Contracts;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteBankBasicMessageHandlerTests;

public class GivenConnectionServicesAvailable : IAsyncLifetime
{
	private const string RtgsGlobalIdToDelete = "rtgs-global-id";
	private const string MessageSource = "RTGS";

	private readonly Mock<IBankConnectionService> _mockBankConnectionService = new();
	private readonly Mock<IRtgsConnectionService> _mockRtgsConnectionService = new();


	public async Task InitializeAsync()
	{
		_mockBankConnectionService
			.Setup(c => c.DeleteBankAsync(RtgsGlobalIdToDelete, It.IsAny<CancellationToken>()))
			.Verifiable();
		
		_mockRtgsConnectionService
			.Setup(r=>r.DeleteBankAsync(RtgsGlobalIdToDelete, It.IsAny<CancellationToken>()))
			.Verifiable();
		
		var handler = new DeleteBankBasicMessageHandler(_mockBankConnectionService.Object, _mockRtgsConnectionService.Object);

		var messageContent = new DeleteBankRequest(RtgsGlobalIdToDelete);
		var message = JsonSerializer.Serialize(new BasicMessageContent<DeleteBankRequest>
		{
			MessageContent = messageContent, Source = MessageSource
		});

		await handler.HandleAsync(message, "connection-1", default);
	}


	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDeleteBankPartnerConnectionssCalled() => _mockBankConnectionService.Verify();

	[Fact]
	public void ThenDeleteRtgsConnectionsCalled() => _mockRtgsConnectionService.Verify();
}
