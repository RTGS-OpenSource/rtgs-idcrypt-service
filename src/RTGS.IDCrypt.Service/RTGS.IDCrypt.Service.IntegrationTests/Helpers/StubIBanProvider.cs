using RTGS.IDCrypt.Service.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Helpers;

public class StubIBanProvider : IIBanProvider
{
	public string CurrentIBan { get; set; } = "GB74BARC20032634763423";

	public string Generate() => CurrentIBan;
}
