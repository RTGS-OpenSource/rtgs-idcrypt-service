﻿using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.VerifyControllerTests.TestData;

public static class VerifyPublicSignature
{
	public const string Path = "/json-signatures/verify/public-did";

	private static VerifyOwnMessageResponse ExpectedResponse => new()
	{
		Verified = true
	};

	private static string SerialisedResponse =>
		$@"{{
			""verified"": {ExpectedResponse.Verified.ToString().ToLowerInvariant()}
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}