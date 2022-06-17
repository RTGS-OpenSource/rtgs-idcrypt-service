using Microsoft.Extensions.DependencyInjection;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;
internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTestIdCryptHttpClient(
		this IServiceCollection services,
		MultiMessageStatusCodeHttpHandler multiMessageStatusCodeHttpHandler)
	{
		services
			.AddSingleton(multiMessageStatusCodeHttpHandler)
			.AddHttpClient("IdCryptServiceClient")
			.AddHttpMessageHandler<MultiMessageStatusCodeHttpHandler>();

		return services;
	}
}
