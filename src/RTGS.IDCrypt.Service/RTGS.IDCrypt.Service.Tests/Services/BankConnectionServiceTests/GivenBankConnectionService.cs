using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.BankConnectionServiceTests;

public class GivenBankConnectionService
{
	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void WhenRtgsGlobalIdIsEmpty_ThenThrow(string rtgsGlobalId) =>
		FluentActions.Invoking(() =>
			new BankConnectionService(
				Mock.Of<IConnectionsClient>(),
				Mock.Of<ILogger<BankConnectionService>>(),
				Mock.Of<IBankPartnerConnectionRepository>(),
				Mock.Of<IAliasProvider>(),
				Mock.Of<IWalletClient>(),
				Options.Create(new CoreConfig
				{
					RtgsGlobalId = rtgsGlobalId
				}),
				Mock.Of<IBasicMessageClient>()))
			.Should().Throw<ArgumentException>()
				.Which.Message.Should().Be("RtgsGlobalId configuration option is not set.");
}
