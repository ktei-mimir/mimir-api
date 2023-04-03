namespace Mimir.Domain.Helpers;

public static class DateTimeUtils
{
    public static long ToUnixTimeStamp(this DateTime dateTime)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timeSpan = dateTime - unixEpoch;
        return (long)timeSpan.TotalSeconds;
    }
}