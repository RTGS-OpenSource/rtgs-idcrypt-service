using WireMock.Server;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Fixtures;

public class TestFixture : IDisposable
{
	private const int Port = 7999;

	private readonly WireMockServer _server;

	public TestFixture()
	{
		_server = WireMockServer.Start(Port);
	}

	public string Url => $"http://localhost:{Port}";
	public WireMockServer Server => _server;

	public void Dispose()
	{
		if (_server is { IsStarted: true })
		{
			_server.Stop();
		}
	}

	public async Task<int> RunProgramAsync() => await Program.Main(Array.Empty<string>());
}
