using System.Collections;
using System.Collections.Generic;
using System;

public static class StringUtils
{

	public static string FormatSeconds(float seconds)
    {
        TimeSpan interval = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                   interval.Days * 24 + interval.Hours,
                   interval.Minutes,
                   interval.Seconds);
    }
}
