using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyPrivateSignatureRequest;

public class AndNoMatchingBankPartnerConnectionExists
{
	private readonly FakeLogger<MessageController> _logger = new();
	private readonly MessageController _controller;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;

	public AndNoMatchingBankPartnerConnectionExists()
	{
		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		bankPartnerConnectionsMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
			.GetEnumerator());

		tableClientMock.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(
				It.IsAny<string>(),
				It.IsAny<int?>(),
				It.IsAny<IEnumerable<string>>(),
				It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionsMock.Object);

		storageTableResolverMock
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_controller = new MessageController(
			_logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object,
			Mock.Of<IDateTimeProvider>(),
			Mock.Of<IWalletClient>());
	}

	[Theory]
	[InlineData("rtgs-global-id-1", "alias-3")]
	[InlineData("rtgs-global-id-2", "alias-1")]
	[InlineData("rtgs-global-id-3", "alias-5")]
	public async Task ThenMessageIsNotVerified(string rtgsGlobalId, string alias)
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		var response = await _controller.Verify(new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = rtgsGlobalId,
			Message = message,
			PrivateSignature = "signature",
			Alias = alias
		});

		using var _ = new AssertionScope();

		_jsonSignaturesClientMock.Verify(client =>
				client.VerifyPrivateSignatureAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()),
			Times.Never);

		response.Should().BeOfType<NotFoundResult>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			$"No bank partner connection found for RTGS Global ID {rtgsGlobalId} " +
			$"and Alias {alias}"
		});
	}
}
