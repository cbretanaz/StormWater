using System;

namespace CoP.Enterprise.Support
{
    public interface IDUIDisplayInterval
    {
        bool IsNull { get; }
        bool HasValue { get; }
        DateTime StartUtc { get; }
        DateTime EndUtc { get; }
        DateTime StartPvt { get; }
        DateTime EndPvt { get; }
        DateTime Date { get; }
        int Year { get; }
        int Month { get; }
        int Day { get; }
        bool IsFallBack { get; }
        bool IsSpringForward { get; }
        DayOfWeek DayOfWeek { get; }
        int ClockHour { get; }
        int HourEnding { get; }
    }
}
