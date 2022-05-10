using Microsoft.Extensions.DependencyInjection;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Extensions;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddTestIdCryptHttpClient(
		this IServiceCollection services,
		StatusCodeHttpHandler statusCodeHttpHandler)
	{
		services
			.AddSingleton(statusCodeHttpHandler)
			.AddHttpClient("AgentHttpClient")
			.AddHttpMessageHandler<StatusCodeHttpHandler>();

		return services;
	}
}
