namespace RTGS.IDCrypt.Service.Helpers;

/// <summary>
/// https://www.amazon.co.uk/Unit-Testing-Principles-Practices-Patterns/dp/1617296279/ref=sr_1_1?keywords=unit+testing+principles%2C+practices%2C+and+patterns&qid=1652188336&sprefix=unit+testing+%2Caps%2C54&sr=8-1
/// page # 272
/// </summary>
public static class DateTimeOffsetServer
{
	private static Func<DateTimeOffset> _func;

	public static DateTimeOffset Now => _func();

	public static void Init(Func<DateTimeOffset> func) => _func = func;
}
