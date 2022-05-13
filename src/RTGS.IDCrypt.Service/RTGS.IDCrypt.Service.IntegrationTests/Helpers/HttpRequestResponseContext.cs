namespace RTGS.IDCrypt.Service.IntegrationTests.Helpers;

public class HttpRequestResponseContext
{
	private readonly string _responseContent;
	private readonly Func<string> _getResponseContent;

	public HttpRequestResponseContext(string requestPath, string responseContent)
	{
		RequestPath = requestPath;
		_responseContent = responseContent;
	}

	public HttpRequestResponseContext(string requestPath, Func<string> getResponseContent)
	{
		RequestPath = requestPath;
		_getResponseContent = getResponseContent;
	}

	public string RequestPath { get; }

	private string GetResponseContentFromResponseContent() => _responseContent;

	public Func<string> ResponseContent => _getResponseContent ?? GetResponseContentFromResponseContent;
}
