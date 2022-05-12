using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Moq;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Storage;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class ThrowingFixture : WebApplicationFactory<Program>
{
	public static SignMessageRequest SignMessageRequest => new(
		"rtgs-global-id",
		@"{ ""Message"": ""I am the walrus"" }");

	protected override IHost CreateHost(IHostBuilder builder)
	{
		var mockStorageTableResolver = new Mock<IStorageTableResolver>();
		mockStorageTableResolver.Setup(resolver => resolver.GetTable(It.IsAny<string>()))
			.Throws(new OperationCanceledException("testing middleware with controller"));

		builder.ConfigureServices(services =>
			services.AddSingleton(mockStorageTableResolver.Object)
		);

		builder.ConfigureHostConfiguration(config =>
		{
			var testConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}
}
