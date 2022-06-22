using System.Net.Http;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;

public record MockHttpRequest(HttpMethod Method, string Path);
