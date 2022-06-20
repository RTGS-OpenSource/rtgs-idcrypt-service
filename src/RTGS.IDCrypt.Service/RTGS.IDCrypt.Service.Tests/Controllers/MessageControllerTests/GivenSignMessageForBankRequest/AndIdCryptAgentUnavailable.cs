using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageForBankRequest;

public class AndIdCryptAgentUnavailable
{
	private readonly MessageController _controller;
	private readonly SignMessageForBankRequest _signMessageForBankRequest;
	private readonly FakeLogger<MessageController> _logger = new();

	public AndIdCryptAgentUnavailable()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_signMessageForBankRequest = new SignMessageForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message
		};

		var referenceDate = new DateTime(2022, 4, 1, 0, 0, 0);
		var dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.SetupGet(provider => provider.UtcNow).Returns(referenceDate);

		var matchingBankPartnerConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = referenceDate.Subtract(TimeSpan.FromDays(1)),
			Status = "Active",
			Role = "Inviter"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		jsonSignaturesClientMock
			.Setup(client => client.SignDocumentAsync(
				It.IsAny<JsonElement>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(_signMessageForBankRequest.RtgsGlobalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(matchingBankPartnerConnection);

		_controller = new MessageController(
			_logger,
			jsonSignaturesClientMock.Object,
			bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _controller.SignForBank(_signMessageForBankRequest, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				"Error occurred when signing JSON document"
			});
	}
}
