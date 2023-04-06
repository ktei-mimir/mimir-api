namespace Mimir.Domain.Helpers;

public static class StringUtils
{
    public static string TakeMax(this string str, int max)
    {
        return str[..Math.Min(str.Length, 10)];
    }
}