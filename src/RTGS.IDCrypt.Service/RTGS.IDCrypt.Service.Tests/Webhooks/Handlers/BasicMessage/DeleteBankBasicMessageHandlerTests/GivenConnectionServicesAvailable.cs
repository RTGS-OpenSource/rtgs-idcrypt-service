using System.Linq.Expressions;
using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.BasicMessage.DeleteBankBasicMessageHandlerTests;

public class GivenConnectionServicesAvailable : IAsyncLifetime
{
	private const string RtgsGlobalIdToDelete = "rtgs-global-id";
	private const string ConnectionId = "connection-1";
	private const string MessageSource = "RTGS";

	private readonly Mock<IBankConnectionService> _mockBankConnectionService = new();
	private readonly Mock<IRtgsConnectionService> _mockRtgsConnectionService = new();
	private readonly Mock<IRtgsConnectionRepository> _mockRtgsConnectionRepository = new();

	public async Task InitializeAsync()
	{
		Expression<Func<RtgsConnection, bool>> connectionExpression =
			connection => connection.ConnectionId == ConnectionId;

		_mockRtgsConnectionRepository.Setup(
			repo => repo.FindAsync(It.Is<Expression<Func<RtgsConnection, bool>>>(expr =>
					LambdaCompare.Eq(expr, connectionExpression)),
				It.IsAny<CancellationToken>())).ReturnsAsync(new List<RtgsConnection> { new() });

		_mockBankConnectionService
			.Setup(service => service.DeleteBankAsync(RtgsGlobalIdToDelete, It.IsAny<CancellationToken>()))
			.Verifiable();

		_mockRtgsConnectionService
			.Setup(service => service.DeleteBankAsync(RtgsGlobalIdToDelete, It.IsAny<CancellationToken>()))
			.Verifiable();

		var handler = new DeleteBankBasicMessageHandler(
			_mockBankConnectionService.Object,
			_mockRtgsConnectionService.Object,
			_mockRtgsConnectionRepository.Object);

		var messageContent = new DeleteBankRequest(RtgsGlobalIdToDelete);
		var message = JsonSerializer.Serialize(new BasicMessageContent<DeleteBankRequest>
		{
			MessageContent = messageContent,
			Source = MessageSource
		});

		await handler.HandleAsync(message, ConnectionId);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDeleteBankPartnerConnectionsCalled() => _mockBankConnectionService.Verify();

	[Fact]
	public void ThenDeleteRtgsConnectionsCalled() => _mockRtgsConnectionService.Verify();
}
