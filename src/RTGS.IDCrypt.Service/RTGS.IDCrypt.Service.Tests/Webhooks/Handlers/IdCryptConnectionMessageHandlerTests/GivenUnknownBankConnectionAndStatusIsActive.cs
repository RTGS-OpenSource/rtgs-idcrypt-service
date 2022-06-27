using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Webhooks.Handlers;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.Proof;
using RTGS.IDCryptSDK.Proof.Models;

namespace RTGS.IDCrypt.Service.Tests.Webhooks.Handlers.IdCryptConnectionMessageHandlerTests;

public class GivenUnknownBankConnectionAndStatusIsActive
{
	private readonly Mock<IProofClient> _proofClientMock;
	private readonly IdCryptConnectionMessageHandler _handler;
	private string _message;

	public GivenUnknownBankConnectionAndStatusIsActive()
	{
		_proofClientMock = new Mock<IProofClient>();

		var logger = new FakeLogger<IdCryptConnectionMessageHandler>();

		var bankPartnerConnectionsRepository = new Mock<IBankPartnerConnectionRepository>();

		bankPartnerConnectionsRepository
			.Setup(repo => repo.ActiveConnectionForBankExists("alias", It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_handler = new IdCryptConnectionMessageHandler(
			logger,
			_proofClientMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			bankPartnerConnectionsRepository.Object);

		var activeBankConnection = new IdCryptConnection
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "active",
			TheirLabel = "RTGS_Bank_Agent_Test"
		};

		_message = JsonSerializer.Serialize(activeBankConnection);
	}

	[Fact]
	public async Task ThenThrow() =>
		await FluentActions
			.Awaiting(() => _handler.HandleAsync(_message, default))
			.Should().ThrowAsync<Exception>();

	[Fact]
	public async Task ThenDoNotRequestProof()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _handler.HandleAsync(_message, default))
			.Should().ThrowAsync<Exception>();

		_proofClientMock.Verify(client =>
				client.SendProofRequestAsync(It.IsAny<SendProofRequestRequest>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
