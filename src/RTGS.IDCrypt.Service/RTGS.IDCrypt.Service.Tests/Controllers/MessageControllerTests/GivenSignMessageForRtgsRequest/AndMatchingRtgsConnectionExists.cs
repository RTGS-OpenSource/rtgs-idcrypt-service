using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageForRtgsRequest;

public class AndMatchingRtgsConnectionExists : IAsyncLifetime
{
	private readonly MessageController _controller;
	private readonly SignMessageForRtgsRequest _signMessageForRtgsRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndMatchingRtgsConnectionExists()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_signMessageForRtgsRequest = new SignMessageForRtgsRequest
		{
			Message = message
		};

		var signDocumentResponse = new SignDocumentResponse
		{
			PairwiseDidSignature = "pairwise-did-signature",
			PublicDidSignature = "public-did-signature"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var connection = new RtgsConnection
		{
			PartitionKey = "alias-2",
			RowKey = "connection-id-2",
			Alias = "alias-2",
			ConnectionId = "connection-id-2",
			CreatedAt = new DateTime(2000, 01, 01).ToUniversalTime(),
			ActivatedAt = new DateTime(2022, 01, 02).ToUniversalTime(),
			Status = "Active",
		};

		_jsonSignaturesClientMock
			.Setup(client => client.SignDocumentAsync(
				_signMessageForRtgsRequest.Message,
				"connection-id-2",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(signDocumentResponse)
			.Verifiable();

		var rtgsConnectionRepositoryMock = new Mock<IRtgsConnectionRepository>();
		rtgsConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(connection);

		var logger = new FakeLogger<MessageController>();

		_controller = new MessageController(
			logger,
			_jsonSignaturesClientMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>(),
			rtgsConnectionRepositoryMock.Object,
			Mock.Of<IWalletClient>());
	}

	public async Task InitializeAsync() =>
		_response = await _controller.SignForRtgs(_signMessageForRtgsRequest, default);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingSignMessageRequest_ThenCallSignMessageWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingSignMessageRequest_ThenReturnOkResponseWithExpectedSignatures()
	{
		var signMessageResponse = new SignMessageResponse
		{
			PairwiseDidSignature = "pairwise-did-signature",
			PublicDidSignature = "public-did-signature",
			Alias = "alias-2"
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(signMessageResponse);
	}
}
