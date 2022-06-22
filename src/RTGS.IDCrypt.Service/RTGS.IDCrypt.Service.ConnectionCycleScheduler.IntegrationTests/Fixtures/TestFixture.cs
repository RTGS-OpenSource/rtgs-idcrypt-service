using WireMock.Server;

namespace RTGS.IDCrypt.Service.ConnectionCycleScheduler.IntegrationTests.Fixtures;

public sealed class TestFixture : IDisposable
{
	private const int Port = 7999;

	public TestFixture()
	{
		Server = WireMockServer.Start(Port);
	}

	public static string Url => $"http://localhost:{Port}";

	public WireMockServer Server { get; }

	public void Dispose()
	{
		if (Server is { IsStarted: true })
		{
			Server.Stop();
		}
	}

	public static async Task<int> RunProgramAsync() => await Program.Main(Array.Empty<string>());
}
