using RTGS.IDCryptSDK;
using RTGS.IDCryptSDK.Extensions;
using RTGS.Service.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IStorageTableResolver, StorageTableResolver>();

builder.Services.AddIdCryptSdk(new IdCryptSdkConfiguration(
	new Uri(builder.Configuration["AgentApiAddress"]),
	builder.Configuration["AgentApiKey"],
	new Uri(builder.Configuration["AgentServiceEndpointAddress"])));

builder.Configuration.AddInMemoryCollection(new[]
{
	new KeyValuePair<string, string>("BankPartnerConnectionsTableName", "BankPartnerConnections")
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
