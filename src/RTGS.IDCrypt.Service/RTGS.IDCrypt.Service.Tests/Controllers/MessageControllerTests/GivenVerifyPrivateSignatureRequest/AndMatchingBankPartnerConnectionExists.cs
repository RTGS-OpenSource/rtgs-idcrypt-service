using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
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

public class AndMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly MessageController _controller;
	private readonly VerifyRequest _verifyRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndMatchingBankPartnerConnectionExists()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_verifyRequest = new VerifyRequest
		{
			RtgsGlobalId = "rtgs-global-id-1",
			Message = message,
			PrivateSignature = "signature",
			Alias = "alias-1"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		_jsonSignaturesClientMock
			.Setup(client => client.VerifyPrivateSignatureAsync(
				_verifyRequest.Message,
				_verifyRequest.PrivateSignature,
				"connection-id-1",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true)
			.Verifiable();

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

		var logger = new FakeLogger<MessageController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_controller = new MessageController(
			logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object,
			Mock.Of<IDateTimeProvider>(),
			Mock.Of<IWalletClient>());
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Verify(_verifyRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenCallVerifyPrivateSignatureWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenReturnOkResponseWithVerifiedTrue()
	{
		var verifyPrivateSignatureResponse = new VerifyResponse
		{
			Verified = true
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(verifyPrivateSignatureResponse);
	}
}
