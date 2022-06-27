using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests.GivenCycledBankConnectionAndStatusIsActive;

public class AndAgentAvailable : IAsyncLifetime
{
	private readonly Mock<IProofClient> _proofClientMock;
	private readonly IdCryptConnectionMessageHandler _handler;

	public AndAgentAvailable()
	{
		_proofClientMock = new Mock<IProofClient>();

		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		var bankPartnerConnectionsRepository = new Mock<IBankPartnerConnectionRepository>();

		bankPartnerConnectionsRepository
			.Setup(repo => repo.OtherActiveExists("alias", It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_handler = new IdCryptConnectionMessageHandler(
			logger,
			_proofClientMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			bankPartnerConnectionsRepository.Object);
	}

	public async Task InitializeAsync()
	{
		var activeBankConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent_Test"
		};

		var message = JsonSerializer.Serialize(activeBankConnection);

		await _handler.HandleAsync(message, default);
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenDoNotRequestProof() =>
		_proofClientMock.Verify(client =>
				client.SendProofRequestAsync(It.IsAny<SendProofRequestRequest>(), It.IsAny<CancellationToken>()),
			Times.Never);
}
