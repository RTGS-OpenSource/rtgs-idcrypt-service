using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RTGS.Service.Dtos;
using Xunit;

namespace RTGS.Service.IntegrationTests;

public class Test
{
	[Fact]
	public async Task HelloWorldTest()
	{
		var application = new MyWebApplicationFactory();

		var client = application.CreateClient();

		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var response = await client.PostAsync(
			"api/signmessage",
			new StringContent(
				JsonSerializer.Serialize(signMessageRequest),
				Encoding.UTF8,
				MediaTypeNames.Application.Json));
	}

	private class MyWebApplicationFactory : WebApplicationFactory<Program>
	{
		protected override IHost CreateHost(IHostBuilder builder)
		{
			builder.ConfigureHostConfiguration(config =>
			{
				var jsonTestConfig = new ConfigurationBuilder()
						.AddJsonFile("testsettings.json")
						.AddEnvironmentVariables()
						.Build();

				config.AddConfiguration(jsonTestConfig);
			});

			return base.CreateHost(builder);
		}
	}
}
