namespace RTGS.IDCrypt.Service.Helpers;

public class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
	public DateTimeOffset Now => DateTimeOffset.Now;
}
