using System.Net;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;

internal static class IdCryptServiceEndpoints
{
	public const string GetInvitedPartnerIdsPath = "/api/connection/InvitedPartnerIds";
	public const string CyclePath = "/api/connection/cycle";

	public static readonly Dictionary<string, MockHttpResponse> MockHttpResponses = new()
	{
		{
			GetInvitedPartnerIdsPath,
			new MockHttpResponse
			{
				Content = @"[""rtgs-global-id""]",
				HttpStatusCode = HttpStatusCode.OK,
			}
		},
		{
			CyclePath,
			new MockHttpResponse
			{
				Content = string.Empty,
				HttpStatusCode = HttpStatusCode.OK,
			}
		}
	};
}
