using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenUnknownConnectionTypeAndStatusIsActive : IAsyncLifetime
{
	private readonly Mock<IProofClient> _proofClientMock;
	private readonly Mock<IRtgsConnectionRepository> _rtgsConnectionsRepository;
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionsRepository;
	private readonly IdCryptConnectionMessageHandler _handler;

	public GivenUnknownConnectionTypeAndStatusIsActive()
	{
		_proofClientMock = new Mock<IProofClient>();

		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		_rtgsConnectionsRepository = new Mock<IRtgsConnectionRepository>();

		_bankPartnerConnectionsRepository = new Mock<IBankPartnerConnectionRepository>();

		_handler = new IdCryptConnectionMessageHandler(
			logger,
			_proofClientMock.Object,
			_rtgsConnectionsRepository.Object,
			_bankPartnerConnectionsRepository.Object);
	}

	public async Task InitializeAsync()
	{
		var activeRtgsConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "unknown-label"
		};

		var message = JsonSerializer.Serialize(activeRtgsConnection);

		await _handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDoNotSetRtgsConnectionActive() =>
		_rtgsConnectionsRepository.Verify(repo =>
				repo.ActivateAsync("connection-id", It.IsAny<CancellationToken>()),
			Times.Never);

	[Fact]
	public void ThenDoNotSetBankPartnerConnectionActive() =>
		_bankPartnerConnectionsRepository.Verify(repo =>
				repo.ActivateAsync("connection-id", It.IsAny<CancellationToken>()),
			Times.Never);

	[Fact]
	public void ThenDoNotRequestProof() =>
		_proofClientMock.Verify(client =>
			client.SendProofRequestAsync(It.IsAny<SendProofRequestRequest>(), It.IsAny<CancellationToken>()),
			Times.Never);
}
