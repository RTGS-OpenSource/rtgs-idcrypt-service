using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;

public class TestFixture : IAsyncLifetime
{
	private readonly TestProgram _program;
	private readonly CancellationTokenSource _cancellationTokenSource;

	public TestFixture()
	{
		_program = new TestProgram();
		_cancellationTokenSource = new CancellationTokenSource();
	}

	public MultiMessageStatusCodeHttpHandler IdCryptStatusCodeHttpHandler => _program.IdCryptStatusCodeHttpHandler;
	public IConfigurationRoot Configuration => _program.Configuration;

	public Task InitializeAsync() => Task.CompletedTask;

	public Task DisposeAsync()
	{
		_cancellationTokenSource.Dispose();
		return Task.CompletedTask;
	}

	public async Task RunProgramAsync()
	{
		var exitCode = await _program.Run(Array.Empty<string>(), _cancellationTokenSource.Token);
		exitCode.Should().Be(0, "exit code should be 0, if not something went wrong");
	}
}
