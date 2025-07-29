using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 日期时间工具类 - 提供日期时间的格式化、转换、计算等功能
/// </summary>
public class DateTimeUtil
{
    /// <summary>
    /// 获取当前UTC时间的时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetCurrentUTCTimestamp()
    {
        // DateTime dateTime = TimeCenter.Instance.UTCNow();
        // long timestamp = (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        // return timestamp;
        return 0;
    }

    /// <summary>
    /// 获取当前UTC时间
    /// </summary>
    /// <returns></returns>
    public static DateTime GetUniversalDataTime()
    {
        // return TimeCenter.Instance.UTCNow();
        return default(DateTime);
    }

    /// <summary>
    /// 将时间戳转换为时间数据对象
    /// 传入时间戳,获得的是本地的时间
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime TimeStampConvertToLocalDateTime(long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        DateTime dateTime = dateTimeOffset.LocalDateTime;

        return dateTime;
    }

    /// <summary>
    /// 将时间戳转换为时间数据对象
    /// 时间戳,获得的是UTC时间
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime TimeStampConvertToUTCDateTime(long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        DateTime dateTime = dateTimeOffset.UtcDateTime;

        return dateTime;
    }

    /// <summary>
    /// 将传入的UTC时间转换为UTC时间戳
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="secont"></param>
    /// <returns></returns>
    public static long TimeConvertToTUCStamp(int year, int month, int day, int hour, int minute, int second)
    {
        DateTime dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        long timestamp = (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        return timestamp;
    }

    /// <summary>
    /// 返回时间的标准日期格式;
    /// </summary>
    /// <returns>yyyy-MM-dd</returns>
    public static string GetDateYmd(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 返回时间的标准日期格式;
    /// </summary>
    /// <returns>yyyy-MM-dd HH:mm:ss</returns>
    public static string GetDateYmdHms(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 返回时间的相对天数标准日期格式;
    /// </summary>
    /// <param name="relativeday">增加的天数</param>
    /// <returns>相对天数</returns>
    public static string GetDateTimeOfDay(DateTime dateTime, int relativeday)
    {
        return dateTime.AddDays(relativeday).ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 将8位日期型整型数据转换为日期字符串数据  例如  20240502 转换后位 2024-05-02
    /// 默认为英文格式;
    /// </summary>
    /// <param name="date">整型日期</param>
    /// <returns></returns>
    public static string FormatDate(int date)
    {
        return FormatDate(date, false);
    }

    /// <summary>
    /// 将8位日期型整型数据转换为日期字符串数据
    /// </summary>
    /// <param name="date">整型日期</param>
    /// <param name="chnType">是否以中文年月日输出</param>
    /// <returns></returns>
    private static string FormatDate(int date, bool chnType)
    {
        string dateStr = date.ToString();
        if (date <= 0 || dateStr.Length != 8) return dateStr;
        if (chnType)
        {
            return dateStr.Substring(0, 4) + "年" + dateStr.Substring(4, 2) + "月" + dateStr.Substring(6) + "日";
        }

        return dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6);
    }

    /// <summary>
    /// 是否常规时间; 格式为"HH:mm"或"HH:mm:ss"
    /// 20:60  false  20:25:26  true
    /// </summary>
    public static bool IsTime(string timeval)
    {
        return Regex.IsMatch(timeval, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
    }

    /// <summary>
    /// 判断某年是否为闰年;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <returns>是否为闰年</returns>;
    public static bool YearIsLeap(int year)
    {
        if (year <= 0001 || year >= 9999)
        {
            return false;
        }

        return DateTime.IsLeapYear(year);
    }

    /// <summary>
    /// 获得某年最后一天的日期;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <returns>最后一天日期</returns>;
    public static DateTime YearOfLastDay(int year)
    {
        if (year <= 0001 || year >= 9999)
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse($"{year}-12-31", out DateTime result) ? result : DateTime.MinValue;
    }

    /// <summary>
    /// 获得某年第一天的日期;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <returns>第一天日期</returns>
    public static DateTime YearOfFirstDay(int year)
    {
        if (year <= 0001 || year >= 9999)
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse($"{year}-01-01", out DateTime result) ? result : DateTime.MinValue;
    }

    /// <summary>
    /// 根据日期获取当前年当前月的总天数;
    /// </summary>
    /// <returns>共有多少天</returns>;
    public static int GetDateCurMonthDays(DateTime dateTime)
    {
        return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
    }

    /// <summary>
    /// 根据日期获取当前年当前月的总天数;
    /// </summary>
    /// <returns>共有多少天</returns>;
    public static int GetDateCurMonthDays(int year, int month)
    {
        return DateTime.DaysInMonth(year, year);
    }

    /// <summary>
    /// 根据日期获取当年当月的第一天日期
    /// </summary>
    /// <returns>第一天日期</returns>
    public static DateTime MonthOfFirstDay(DateTime dateTime)
    {
        return MonthOfFirstDay(dateTime.Year, dateTime.Month);
    }

    /// <summary>
    /// 获得某年某月第一天的日期;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <param name="month">需要计算的月份</param>
    /// <returns>第一天日期</returns>
    public static DateTime MonthOfFirstDay(int year, int month)
    {
        if (year <= 0001 || year >= 9999)
        {
            return DateTime.MinValue;
        }

        if (month < 1 || month > 12)
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse($"{year}-{month}-01", out DateTime result) ? result : DateTime.MinValue;
    }

    /// <summary>
    /// 获得当前年当前月最后一天的日期;
    /// </summary>
    /// <returns>最后一天日期</returns>
    public static DateTime MonthOfLastDay(DateTime dateTime)
    {
        return MonthOfLastDay(dateTime.Year, dateTime.Month);
    }

    /// <summary>
    /// 获得某年某月最后一天的日期;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <param name="month">需要计算的月份</param>
    /// <returns>最后一天日期</returns>;
    public static DateTime MonthOfLastDay(int year, int month)
    {
        if (year <= 0001 || year >= 9999)
        {
            return DateTime.MinValue;
        }

        if (month < 1 || month > 12)
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse($"{year}-{month}-{GetDateCurMonthDays(year, month)}", out DateTime result)
            ? result
            : DateTime.MinValue;
    }

    /// <summary>
    /// 统计一段时间内有多少个星期几;
    /// </summary>
    /// <param name="beginDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="weekNumber">星期几</param>
    /// <returns>多少个星期几</returns>;
    public static int WeekOfTotalWeeks(DateTime beginDate, DateTime endDate, DayOfWeek weekNumber)
    {
        TimeSpan _dayTotal = new TimeSpan(endDate.Ticks - beginDate.Ticks);
        int result = (int)_dayTotal.TotalDays / 7;
        double iLen = _dayTotal.TotalDays % 7;
        for (int i = 0; i <= iLen; i++)
        {
            if (beginDate.AddDays(i).DayOfWeek == weekNumber)
            {
                result++;
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 计算日期前月属于第几季度;
    /// </summary>
    /// <returns>当前年第几季度</returns>
    public static int QuarterOfCurrent(DateTime dateTime)
    {
        return QuarterOfCurrent(dateTime.Month);
    }

    /// <summary>
    /// 计算某个月属于第几季度;
    /// </summary>
    /// <param name="month">需要计算的月份</param>
    /// <returns>某年第几季度</returns>
    public static int QuarterOfCurrent(int month)
    {
        if (month < 1 || month > 12) return -1;
        return (month - 1) / 3 + 1;
    }

    /// <summary>
    /// 获得指定日期所在季度的开始日期和结束日期;
    /// </summary>
    /// <param name="fromDate">需要计算的日期</param>
    /// <param name="quarterBeginDate">开始日期</param>
    /// <param name="quarterEndDate">结束日期</param>
    public static void QuarterOfDate(DateTime fromDate, out DateTime quarterBeginDate, out DateTime quarterEndDate)
    {
        int quarter = QuarterOfCurrent(fromDate.Month);
        QuarterOfDate(fromDate.Year, quarter, out quarterBeginDate, out quarterEndDate);
    }

    /// <summary>
    /// 获得某年第某季度的开始日期和结束日期;
    /// </summary>
    /// <param name="year">需要计算的年份</param>
    /// <param name="quarter">需要计算的季度</param>
    /// <param name="quarterBeginDate">开始日期</param>
    /// <param name="quarterEndDate">结束日期</param>
    public static void QuarterOfDate(int year, int quarter, out DateTime quarterBeginDate,
        out DateTime quarterEndDate)
    {
        quarterBeginDate = DateTime.MinValue;
        quarterEndDate = DateTime.MinValue;
        if (year <= 0001 || year >= 9999 || quarter < 1 || quarter > 4)
        {
            return;
        }

        int month = (quarter - 1) * 3 + 1;
        quarterBeginDate = new DateTime(year, month, 1);
        quarterEndDate = quarterBeginDate.AddMonths(3).AddMilliseconds(-1);
    }

    /// <summary>
    /// 将一天内的秒转换为字符串时间
    /// </summary>
    /// <param name="Seconds">秒</param>
    /// <returns></returns>
    public static string SecondsToString(int Seconds)
    {
        string StrContent = "00:01";

        int hour = Seconds / 3600;
        int min = (Seconds % 3600) / 60;
        int sec = (Seconds % 3600) % 60;

        if (hour > 0)
        {
            StrContent = hour.ToString().PadLeft(2, '0') + ":";
            StrContent += min.ToString().PadLeft(2, '0') + ":";
            StrContent += sec.ToString().PadLeft(2, '0');
        }
        else if (min > 0)
        {
            StrContent = min.ToString().PadLeft(2, '0') + ":";
            StrContent += sec.ToString().PadLeft(2, '0') + "";
        }
        else if (sec > 0)
        {
            StrContent = "00:" + sec.ToString().PadLeft(2, '0') + "";
        }

        return StrContent;
    }

    /// <summary>
    /// 是否是同一天  时间戳
    /// </summary>
    /// <param name="time_seconds1"></param>
    /// <param name="time_seconds2"></param>
    /// <returns></returns>
    public static bool IsMoreThanOneDay(int time_seconds1, int time_seconds2)
    {
        time_seconds1 = time_seconds1 + 60 * 60 * 8;
        time_seconds2 = time_seconds2 + 60 * 60 * 8;
        return (time_seconds1 / (24 * 60 * 60)) == (time_seconds2 / (24 * 60 * 60));
    }

    /// <summary>
    /// 传入一个时间戳，判断和当天时间是否大于一天了
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static bool UtcSameDay(long time)
    {
        // DateTime LastPlayTime = TimeStampConvertToUTCDateTime(time);
        // DateTime curDateTime = TimeCenter.Instance.UTCNow();
        // if (curDateTime.Year == LastPlayTime.Year)
        // {
        //     if (curDateTime.Month == LastPlayTime.Month)
        //     {
        //         if (curDateTime.Day > LastPlayTime.Day)
        //         {
        //             return true;
        //         }
        //     }
        //     else if (curDateTime.Month > LastPlayTime.Month)
        //     {
        //         return true;
        //     }
        // }
        // else if (curDateTime.Year > LastPlayTime.Year)
        // {
        //     return true;
        // }

        return false;
    }

    /// <summary>
    /// 连个日期相差多少天
    /// </summary>
    /// <param name="dateStart">开始时间</param>
    /// <param name="dateEnd">结束时间</param>
    /// <param name="isIgnoreTime">是否忽略时间部分</param>
    /// <returns></returns>
    public static int DateDiffDays(DateTime dateStart, DateTime dateEnd, bool isIgnoreTime)
    {
        // 判断连个日期谁大谁小
        if (DateTime.Compare(dateEnd, dateStart) < 0)
        {
            (dateStart, dateEnd) = (dateEnd, dateStart);
        }

        if (isIgnoreTime)
        {
            TimeSpan timeSpan = dateEnd.Date.Subtract(dateStart.Date);
            return timeSpan.Days;
        }
        else
        {
            TimeSpan sp = dateEnd.Subtract(dateStart);
            return sp.Days;
        }
    }

    /// <summary>
    /// 获取指定年月份的所有日期
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    public static List<DateTime> AllDatesInAMonth(int year, int month)
    {
        List<DateTime> dates = new List<DateTime>();

        // 获取该月的第一天
        DateTime firstDay = new DateTime(year, month, 1);

        // 获取下个月的第一天
        DateTime nextMonth = firstDay.AddMonths(1);

        // 计算该月的天数
        int daysInMonth = (nextMonth - firstDay).Days;

        // 遍历该月的每一天
        for (int day = 1; day <= daysInMonth; day++)
        {
            dates.Add(new DateTime(year, month, day));
        }

        return dates;
    }
}