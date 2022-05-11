using System.Net.Mime;
using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RTGS.IDCrypt.Service.Extensions;
using Serilog;
using Serilog.Events;

TelemetryClient telemetryClient = null;

CreateSerilogLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRtgsDependencies(builder.Configuration);

builder.Host.UseSerilog((_, provider, config) =>
{
	telemetryClient = provider.GetRequiredService<TelemetryClient>();
	ConfigureLogging(config).WriteTo.ApplicationInsights(telemetryClient, TelemetryConverter.Traces);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
		if (exceptionHandlerFeature?.Error != null)
		{
			context.Response.ContentType = MediaTypeNames.Application.Json;
			await context.Response.WriteAsync(
				JsonSerializer.Serialize(
					new { error = exceptionHandlerFeature.Error.Message },
					new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
				));
		}
	});
});

app.MapHealthChecks("/healthz");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
	Log.Information("Starting web host");
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
	if (telemetryClient != null)
	{
		telemetryClient.Flush();
		// Flush is not blocking so estimate how long the flush requires.
		await Task.Delay(TimeSpan.FromSeconds(5));
	}
	Log.CloseAndFlush();
}

void CreateSerilogLogger() =>
	Log.Logger = ConfigureLogging(new LoggerConfiguration())
		.WriteTo.ApplicationInsights(TelemetryConverter.Traces)
		.CreateLogger();

LoggerConfiguration ConfigureLogging(LoggerConfiguration loggerConfiguration) =>
	loggerConfiguration
		.Enrich.FromLogContext()
		.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
		.WriteTo.Console();

public partial class Program { }
