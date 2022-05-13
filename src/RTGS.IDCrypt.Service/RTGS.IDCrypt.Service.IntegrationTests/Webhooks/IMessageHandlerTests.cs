using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RTGS.IDCrypt.Service.Webhooks.Handlers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks;

public class IMessageHandlerTests : WebApplicationFactory<Program>
{
	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.ConfigureHostConfiguration(config =>
		{
			var testConfig = new ConfigurationBuilder()
				.AddJsonFile("testsettings.json")
				.AddEnvironmentVariables()
				.Build();

			config.AddConfiguration(testConfig);
		});

		return base.CreateHost(builder);
	}

	[Fact]
	public void MessageHandlersShouldBeUnique()
	{
		var messageHandlers = Services.GetServices<IMessageHandler>();

		messageHandlers
			.GroupBy(messageHandler => messageHandler.MessageType)
			.Where(group => group.Count() > 1)
			.Should().BeEmpty();
	}
}
