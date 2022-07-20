using System.Text;
using System.Text.Json;
using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace RTGS.IDCrypt.LoadTest;

public class Program
{
	public static void Main()
	{
		var configuration = FeedData.FromJson<Configuration>("./configuration.json").Single();

		var document = JsonSerializer.SerializeToElement(new Dictionary<string, object>
		{
			{ "creditorAmount", 1833094.22 },
			{ "debtorAgentAccountIban", "debtorAgentAccountIban" },
			{ "debtorAccountIban", "debtorAccountIban" }
		});

		var signBody = JsonSerializer.Serialize(new SignDocumentRequest
		{
			ConnectionId = configuration.SignConnectionId,
			Document = document
		});

		var clientFactory = HttpClientFactory.Create();

		var signStep = Step.Create("sign document",
			clientFactory: clientFactory,
			timeout: TimeSpan.FromSeconds(60),
			execute: context =>
			{
				var request = Http.CreateRequest("POST", configuration.SignUrl)
					.WithHeader("X-API-Key", configuration.SignApiKey)
					.WithBody(new StringContent(signBody, Encoding.UTF8, "application/json"))
					.WithCheck(async response =>
					{
						var json = await response.Content.ReadAsStringAsync();

						return response.IsSuccessStatusCode
							? Response.Ok(
								JsonSerializer.Deserialize<SignDocumentResponse>(json),
								statusCode: (int)response.StatusCode)
							: Response.Fail(statusCode: (int)response.StatusCode);
					});

				return Http.Send(request, context);
			});

		var verifyStep = Step.Create("verify document",
			clientFactory: clientFactory,
			timeout: TimeSpan.FromSeconds(60),
			execute: context =>
			{
				var signatures = context.GetPreviousStepResponse<SignDocumentResponse>();

				var verifyBody = JsonSerializer.Serialize(new VerifyPrivateSignatureRequest<JsonElement>()
				{
					ConnectionId = configuration.VerifyConnectionId,
					Signature = signatures!.PairwiseDidSignature,
					Document = document
				});

				var request = Http.CreateRequest("POST", configuration.VerifyUrl)
					.WithHeader("X-API-Key", configuration.VerifyApiKey)
					.WithBody(new StringContent(verifyBody, Encoding.UTF8, "application/json"));

				return Http.Send(request, context);
			});

		var signAndVerifyScenario = ScenarioBuilder
			.CreateScenario("Signing and Verifying", signStep, verifyStep)
			.WithWarmUpDuration(TimeSpan.FromSeconds(5))
			.WithLoadSimulations(
				Simulation.InjectPerSec(rate: 1, during: TimeSpan.FromSeconds(60)),
				Simulation.InjectPerSec(rate: 3, during: TimeSpan.FromSeconds(60)),
				Simulation.InjectPerSec(rate: 5, during: TimeSpan.FromSeconds(60)),
				Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromSeconds(60)),
				Simulation.InjectPerSec(rate: 20, during: TimeSpan.FromSeconds(60))
			);

		NBomberRunner
			.RegisterScenarios(signAndVerifyScenario)
			.Run();
	}
}
