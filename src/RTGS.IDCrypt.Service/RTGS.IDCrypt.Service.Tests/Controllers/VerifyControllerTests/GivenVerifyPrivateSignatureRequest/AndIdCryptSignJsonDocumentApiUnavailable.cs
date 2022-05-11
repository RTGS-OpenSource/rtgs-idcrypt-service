using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests.GivenVerifyPrivateSignatureRequest;

public class AndIdCryptSignJsonDocumentApiUnavailable
{
	private readonly VerifyPrivateSignatureRequest _request;
	private readonly VerifyController _verifyController;
	private readonly FakeLogger<VerifyController> _logger;

	public AndIdCryptSignJsonDocumentApiUnavailable()
	{
		_request = new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = "rtgs-global-id-1",
			Alias = "alias-1",
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PrivateSignature = "private-signature"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		jsonSignaturesClientMock
			.Setup(client => client.VerifyJsonDocumentPrivateSignatureAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		bankPartnerConnectionsMock.Setup(
			bankPartnerConnections =>
				bankPartnerConnections.GetEnumerator()).Returns(
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
			.Setup(storageTableResolver =>
				storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		_logger = new FakeLogger<VerifyController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_verifyController = new VerifyController(
			_logger,
			options,
			storageTableResolverMock.Object,
			jsonSignaturesClientMock.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _verifyController.Post(_request))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when sending VerifyPrivateSignature request to ID Crypt Cloud Agent"
		});
	}
}
