using System;

static class CurrentMillis
{
    private static readonly DateTime Jan1St1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long GetMillis()
    {
        return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
    }

    public static long GetDifference(long value)
    {
        return Math.Abs(value - GetMillis());
    }

    public static long GetDifferenceSeconds(long value)
    {
        return GetDifference(value) / 1000;
    }
}