using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenStatusIsActive : IAsyncLifetime
{
	private readonly IdCryptConnection _activeConnection;
	private readonly Mock<IProofClient> _proofClientMock;
	private readonly IdCryptConnectionMessageHandler _handler;

	public GivenStatusIsActive()
	{
		_activeConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active"
		};

		_proofClientMock = new Mock<IProofClient>();

		var expectedRequest = new SendProofRequestRequest
		{
			ConnectionId = _activeConnection.ConnectionId
		};

		Func<SendProofRequestRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedRequest);

			return true;
		};

		_proofClientMock
			.Setup(client => client.SendProofRequestAsync(
				It.Is<SendProofRequestRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		_handler = new IdCryptConnectionMessageHandler(logger, _proofClientMock.Object);
	}

	public async Task InitializeAsync()
	{
		var message = JsonSerializer.Serialize(_activeConnection);

		await _handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenRequestProofAsyncWithExpected() =>
		_proofClientMock.Verify();
}
