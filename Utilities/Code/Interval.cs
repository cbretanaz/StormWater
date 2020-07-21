using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CoP.Enterprise
{
    [Serializable]
    public struct Interval: IComparable, IXmlSerializable
    {
        #region explanation
        /*  private integer field value represents Interval start 
            as count of ticks from Midnight 1 Jan 2000 */
        #endregion explanation

        #region constants
        private static readonly long baseTiks =
            new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
        private static readonly TimeSpan OneHour = 
            TimeSpan.FromTicks(TimeSpan.TicksPerHour);
        public static Interval NULL;
        #endregion constants

        #region private fields
        private long tiks; // number of ticks since midnight 1 Jan 2000 utc
        private TimeSpan len;
        private readonly bool isDef;
        #endregion private fields
        
        #region ctor / factories
        private Interval(long ticks) : this(ticks, IntervalLength.Hour) { }
        private Interval(long ticks, IntervalLength itvlLength) //: this()
        {
            tiks = ticks;
            len = TimeSpan.FromTicks((int)itvlLength*TimeSpan.TicksPerMinute) ;
            isDef = true;
        }
        private Interval(long ticks, TimeSpan timSpan)// : this()
        {
            tiks = ticks;
            len = timSpan;
            isDef = true;
        }
        #region factories
        public static Interval Make(string startIntervalUTC)
        { return Make(startIntervalUTC, OneHour); }
        public static Interval Make(string startIntervalUTC, IntervalLength intervalLength)
        { return Make(startIntervalUTC, TimeSpan.FromTicks((int)intervalLength * TimeSpan.TicksPerMinute)); }
        public static Interval Make(string startIntervalUTC, TimeSpan timSpan)
        {
            DateTime strtIntvlUTC;
            if (!DateTime.TryParse(startIntervalUTC, out strtIntvlUTC))
                throw new ArgumentException(string.Format(
                        "Cannot parse {0} as a DateTime.", startIntervalUTC),
                    "startIntervalUTC");
            return new Interval(strtIntvlUTC.Ticks - baseTiks, timSpan);
        }
        public static Interval Make(DateTime startIntervalDateTime)
        { return Make(startIntervalDateTime, OneHour); }
        public static Interval Make(DateTime startIntervalDateTime, TimeSpan timSpan)
        {
            var inKind=startIntervalDateTime.Kind;
            var startUtc = 
                inKind == DateTimeKind.Utc ?  startIntervalDateTime:
                inKind == DateTimeKind.Local? startIntervalDateTime.ToUniversalTime():
                                              startIntervalDateTime.FromPacificTime();
            return new Interval(startUtc.Ticks - baseTiks, timSpan);
        }
        public static Interval Make(int intervalId)
        { return new Interval(intervalId * TimeSpan.TicksPerHour, TimeSpan.FromHours(1)); }
        public static Interval Make(int intervalId, int hours)
        { return new Interval(intervalId * TimeSpan.TicksPerHour, TimeSpan.FromHours(hours)); }
        public static Interval Make(int intervalId, TimeSpan timSpan)
        { return new Interval(intervalId * TimeSpan.TicksPerHour, timSpan); }
        public static Interval Parse(string intervalStartHour, TimeSpan timSpan)
        { return Make(intervalStartHour, timSpan); }
        #endregion factories
        #endregion ctor / factories

        #region properties
        public bool IsNull { get { return !isDef; } }
        public bool HasValue { get { return isDef; } }
        public long Ticks
        { 
            get  { if (IsNull) throw new 
                InvalidOperationException(
                "Interval is not defined"); 
                return tiks; }
        }
        /// <summary>
        /// Length of this interval in HOurs, minutes, seconds
        /// </summary>
        public TimeSpan Length 
        {
            get{ if (IsNull) throw new 
                    InvalidOperationException(
                    "Interval is not defined");
                return len;
            }
        }
        public long IntervalId { get { return Ticks/TimeSpan.TicksPerHour;}}

        #region DateTime properties
        [XmlAttribute(DataType = "string", AttributeName = "startUtc")]
        public string startUtc
        { get { return new DateTime(baseTiks + tiks, DateTimeKind.Utc).ToString("dd-MM-yyyy"); } }
        public DateTime StartUtc
        { get { return new DateTime(baseTiks + tiks, DateTimeKind.Utc); } }
        public DateTime EndUtc
        { get { return StartUtc + Length; } }
        public DateTime StartPvt
        { get { return StartUtc.ToPacificTime(); } }
        public DateTime EndPvt
        { get { return EndUtc.ToPacificTime(); } }
        public int Year { get { return StartPvt.Year; } }
        public int Month { get { return StartPvt.Month; } }
        public int Day { get { return StartPvt.Day; } }
        public DayOfWeek DayOfWeek { get { return StartPvt.DayOfWeek; } }
        public int ClockHour { get { return StartPvt.Hour; } }
        public int HourEnding { get { return EndPvt.Hour; } } 
        public string HE { get { return (EndPvt.Hour == 0? 24:EndPvt.Hour).ToString("'HE'00"); } }
        public int HourIndex { get { return (StartUtc - StartPvt.Date.ToPacificTime()).Hours; } }
        public HourOfDay HourOfDay
        {
            get
            {
                return ClockHour == 2 ?
                    (HourIndex == 3 ? HourOfDay.H2X : HourOfDay.H02) :
                    ((HourOfDay)ClockHour);
            }
        }
        #endregion DateTime properties
        #endregion properties

        #region IComparable Members
        public int CompareTo(object obj)
        {
            if (!(obj is Interval))
                throw new ArgumentException(string.Format(
                    "Object {0} is not an Interval.", obj));
            var othItvl = (Interval)obj;
            if (IsNull || othItvl.IsNull)
                throw new ArgumentNullException();
            return tiks == othItvl.tiks?
                Length.CompareTo(othItvl.Length):
                tiks.CompareTo(othItvl.tiks);
        }
        #endregion

        #region IXmlSerializable Members
        XmlSchema IXmlSerializable.GetSchema() { return null; }
        void IXmlSerializable.WriteXml(XmlWriter w)
        {
            w.WriteAttributeString("intervalId", IntervalId.ToString("0"));
            w.WriteAttributeString("start", 
                StartUtc.ToString("yyyy-MM-dd'T'HH:mm'Z'") );
            w.WriteAttributeString("lengthHrs", 
                len.TotalHours.ToString("0.00"));
        }
        void IXmlSerializable.ReadXml(XmlReader rdr)
        {
            DateTime srtUtc;
            if(!DateTime.TryParse(rdr.GetAttribute("start"), out srtUtc))
                throw new XmlException(
                    "Could not deserialize Interval Utc Start Datetime.");

            tiks = srtUtc.Ticks - baseTiks;
            double hrs;
            if(!double.TryParse(rdr.GetAttribute("lengthHrs"), out hrs))
                throw new XmlException(
                    "Could not deserialize Interval Length Timespan.");
            len = TimeSpan.FromHours(hrs);
        }
        #endregion IXmlSerializable Members

        #region operator overrides
        public static Interval operator +(Interval intvl, TimeSpan timSpan)
        { return new Interval(intvl.tiks + timSpan.Ticks, intvl.Length); }
        public static Interval operator -(Interval intvl, TimeSpan timSpan)
        { return intvl + (-timSpan); }
        public static Interval operator +(Interval intvl, int intervals)
        { return new Interval(intvl.tiks + intervals * intvl.Length.Ticks, intvl.Length); }
        public static Interval operator -(Interval intvl, int intervals)
        { return intvl + (-intervals); }
        public static Interval operator ++(Interval intvl) { return intvl + 1; }
        public static Interval operator --(Interval intvl) { return intvl - 1; }
        public static bool operator ==(Interval x, Interval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return  x.tiks == y.tiks && x.Length == y.Length;
        }
        public static bool operator !=(Interval x, Interval y)
        { return !(x == y); }
        public static bool operator >(Interval x, Interval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.tiks == y.tiks? x.Length > y.Length: x.tiks > y.tiks;
        }
        public static bool operator <(Interval x, Interval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.tiks == y.tiks? x.Length < y.Length: x.tiks < y.tiks;
        }
        public static bool operator >=(Interval x, Interval y)
        { return !(x < y); }
        public static bool operator <=(Interval x, Interval y)
        { return !(x > y); }
        public override bool Equals(object o)
        {
            if (!(o is Interval))
                throw new ArgumentException(string.Format(
                    "Object {0} is not an Interval.", o));
            return (this == (Interval)o);
        } 
        public override int GetHashCode()
        { return isDef? (int)((tiks + Length.Ticks) % int.MaxValue) : 0; }
        #endregion operator overrides

        #region other overrides
        public override string ToString()
        { 
            return !isDef ? "Null":
            string.Format("{0:d MMM yyyy HH:mm} Utc:  {1}:{2}", 
                StartUtc, Length.Hours, Length.Minutes % 60);
        }
        public string AbbrevDisplay
        {
            get
            {
                return !isDef ? "Null" :
                string.Format("{0:HHmmddMMMyyyy}Z: {1}:{2}",
                    StartUtc, Length.Hours, Length.Minutes % 60);
            }
        }
        #endregion other overrides
    }

    [Serializable]
    public struct CalendarMonth : IComparable, IComparable<CalendarMonth>
    {
        #region explanation
        /*  
              private integer field value stores calendar month
              bits 0-3 store the month; 
              bits 4-11 store the year,
                  as number of years since BASEYEAR (1900)
              YYYYYYYY MMMM
         * */
        #endregion explanation

        #region static / instance constants
        private const int BASEYEAR = 1900;
        private const int
            YEARMASK = 0xFFF0,
           MONTHMASK = 0x0F;
        public static CalendarMonth Null;
        public static readonly CalendarMonth MinCalendarMonth = new CalendarMonth("Jan 1900");
        public static readonly CalendarMonth MaxCalendarMonth = new CalendarMonth("Dec 2999");
        #endregion constants

        #region private fields
        private readonly int value;
        private readonly bool isDef;
        #endregion private fields

        #region properties
        public static CalendarMonth ThisMonth { get { return new CalendarMonth(DateTime.Today); } }
        public static CalendarMonth LastMonth { get { return ThisMonth - 1; } }
        public static CalendarMonth NextMonth { get { return ThisMonth + 1; } }
        public int Year { get { return BASEYEAR + ((value & YEARMASK) >> 4); } }
        public int Month { get { return (value & MONTHMASK); } }
        public DateTime StartTime
        { get { return new DateTime(Year, Month, 1); } }
        public DateTime StartTimeUtc
        { get { return StartTime.FromPacificTime(); } }
        public DateTime EndDateTime
        { get { return StartTime.AddMonths(1); } }
        public DateTime EndTimeUtc
        { get { return EndDateTime.FromPacificTime(); } }
        public bool IsNull { get { return !isDef; } }
        public bool HasValue { get { return isDef; } }
        #endregion properties

        #region ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDatePvt">Local Prevailing Time</param>
        public CalendarMonth(string sDatePvt)
        {
            DateTime date;
            string[] dtParts = sDatePvt.Split(' ');
            int yr;
            if (dtParts.Length == 2 && dtParts[1].Length == 2 && Int32.TryParse(dtParts[1], out yr))
                sDatePvt = dtParts[0] + (yr < 40 ? " 20" : " 19") + dtParts[1];
            if (!DateTime.TryParse(sDatePvt, out date))
                throw new CoPException(string.Format(
                    "Unable to parse {0} as a calendar month.", sDatePvt));
            // -------------------------------------
            value = GetValue(date);
            isDef = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datePvt">Local Prevailing Time</param>
        public CalendarMonth(DateTime datePvt) : this(datePvt, DateTimeKind.Local) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date">Any datetime within the month</param>
        /// <param name="kind">enumeration to indicate whether the date time in date is local or utc</param>
        public CalendarMonth(DateTime date, DateTimeKind kind)
        {
            value = GetValue(kind == DateTimeKind.Utc ? date.ToPacificTime() : date);
            isDef = true;
        }
        private static int GetValue(DateTime datePvt)
        { return datePvt.Month + ((datePvt.Year - BASEYEAR) << 4); }

        public static CalendarMonth Parse(string datePvt)
        { return new CalendarMonth(datePvt); }
        #endregion ctor

        #region IComparable/IComparable<CalendarMonth> Members
        public int CompareTo(object obj)
        {
            if (!(obj is CalendarMonth))
                throw new ArgumentException(string.Format(
                    "Object {0} is not a CalendarMonth.", obj));
            return CompareTo((CalendarMonth)obj);
        }
        public int CompareTo(CalendarMonth other)
        {
            if (IsNull || other.IsNull)
                throw new ArgumentNullException();
            return value.CompareTo(other.value);
        } 
        #endregion

        #region operator overrides
        public static CalendarMonth operator +(CalendarMonth calMon, int months)
        { return new CalendarMonth(calMon.StartTime.AddMonths(months), DateTimeKind.Local); }
        public static CalendarMonth operator  -(CalendarMonth calMon, int months)
        { return calMon + (-months); }
        public static CalendarMonth operator ++(CalendarMonth calMon) { return calMon + 1; }
        public static CalendarMonth operator --(CalendarMonth calMon) { return calMon - 1; }
        public static bool operator ==(CalendarMonth x, CalendarMonth y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return (x.value == y.value);
        }
        public static bool operator !=(CalendarMonth x, CalendarMonth y)
        { return !(x == y); }
        public static bool operator > (CalendarMonth x, CalendarMonth y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.value > y.value;
        }
        public static bool operator < (CalendarMonth x, CalendarMonth y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.value < y.value;
        }
        public static bool operator >=(CalendarMonth x, CalendarMonth y)
        { return !(x < y); }
        public static bool operator <=(CalendarMonth x, CalendarMonth y)
        { return !(x > y); }
        public override bool Equals(object o)
        {
            try { return (this == (CalendarMonth)o); }
            catch { return false; }
        }
        public override int GetHashCode() { return isDef ? value : 0; }
        #endregion operator overrides

        #region other overrides
        public override string ToString()
        { return isDef ? (new DateTime(Year, Month, 1)).ToString("MMMM yyyy") : "CalendarMonth Null"; }
        public string AbbrevDisplay
        { get { return isDef ? (new DateTime(Year, Month, 1)).ToString("MMMyy") : "Null"; } }
        #endregion other overrides

        public static DateTime GetStartTimeLocal(DateTime anyTimeUtc)
        {
            var localDate = anyTimeUtc.ToPacificTime().Date;
            return localDate.AddDays(1 - localDate.Day);
        }
        public CalendarMonth AddMonths(int months)
        { return new CalendarMonth(StartTime.AddMonths(months), DateTimeKind.Local); }
        public static int HoursInTheTimeSpan(DateTime fromDate, DateTime toDate)
        {
            var hours = (int)(toDate.Date - fromDate.Date).TotalHours;
            if (fromDate.IsDaylightSavingTime() || toDate.IsDaylightSavingTime()
                 && fromDate.IsDaylightSavingTime() != toDate.IsDaylightSavingTime())
            {
                if (fromDate.IsDaylightSavingTime())
                    hours += 1;
                else
                    hours -= 1;
            }
            return hours;
        }
    }

    [Serializable]
    public struct CalendarDay : IComparable, IComparable<CalendarDay>
    {
        #region explanation
        /*  private integer field value stores calendar date
            exactly the same as SQL Server Stores date portion of datetime
            --- as a 32 bit signed integer representing the number of days 
                since 1 Jan 1900, with value = 0 representing 1 jan 1900
         * **********************************************************/
        #endregion explanation 
    
        #region private fields
        private readonly int value;
        private readonly bool isDef;
        #endregion private fields

        #region constants
        private static readonly DateTime BASEDate = new DateTime(1900, 1, 1 );
        public static CalendarDay Null;
        public static readonly CalendarDay MinCalendarMonth = new CalendarDay("Jan 1900");
        public static readonly CalendarDay MaxCalendarMonth = new CalendarDay("Dec 2999");
        #endregion constants

        #region ctor / factorys
        public static CalendarDay Parse(string datePvt)
        { return new CalendarDay(datePvt); }
        public static CalendarDay Make(DateTime localDate)
        { return new CalendarDay(GetValue(localDate)); }
        public static CalendarDay Make(DateTime date, DateTimeKind kind)
        {
            return new CalendarDay(
              GetValue(kind == DateTimeKind.Utc ?
              date.ToPacificTime() : date));
        }
        public static CalendarDay Make(int calDayId)
        { return new CalendarDay(calDayId); }
        private CalendarDay(string sDatePvt)
        {
            DateTime date;
            if (DateTime.TryParse(sDatePvt, out date))
            {
                value = GetValue(date);
                isDef = true;
            }
            throw new CoPException(string.Format(
                "Unable to parse {0} as a calendar month.", sDatePvt));
        }
        private static int GetValue(DateTime datePvt)
        { return (int)Math.Floor((datePvt - BASEDate).TotalDays); }
        private CalendarDay(int calDayId)
        {
            value = calDayId;
            isDef = true;
        }
        #endregion ctor / factorys

        #region properties
        #region static CalendarDay Factory Propertys
        public static CalendarDay Today { get { return Make(DateTime.Today); } }
        public static CalendarDay Tomorrow { get { return Today + 1; } }
        public static CalendarDay Yesterday { get { return Today - 1; } }
        public static CalendarDay FirstOfMonth 
        { get { return Today + (1 - DateTime.Today.Day); } }
        public static CalendarDay LastOfMonth
        {
            get
            {
                return Make(DateTime.Today.AddDays(1 -
                    DateTime.Today.Day).AddMonths(1).AddDays(-1));
            }
        }
        #endregion static CalendarDay Factory Propertys
        public bool IsNull { get { return !isDef; } }
        public bool HasValue { get { return isDef; } }
        public DateTime Date
        { get { return BASEDate.AddDays(value); } }
        public int Year { get { return Date.Year; } }
        public int Month { get { return Date.Month; } }
        public DayOfWeek WeekDay { get { return Date.DayOfWeek; } }
        public int Day { get { return Date.Day; } }
        public DateTime StartTimeUtc
        { get { return BASEDate.FromPacificTime(); } }
        public DateTime EndTimeUtc
        { get { return BASEDate.AddDays(1).FromPacificTime(); } }
        #endregion properties

        #region IComparable/IComparable<CalendarDay> Members
        public int CompareTo(object obj)
        {
            if (!(obj is CalendarDay))
                throw new ArgumentException(string.Format(
                    "Object {0} is not a CalendarDay.", obj));
            return CompareTo((CalendarDay)obj);
        }
        public int CompareTo(CalendarDay other)
        {
            if (IsNull || other.IsNull)
                throw new ArgumentNullException();
            return value.CompareTo(other.value);
        } 
        #endregion   
    
        #region operator overrides
        public static CalendarDay operator +(CalendarDay calDay, int days)
        { return new CalendarDay(calDay.value + days); }
        public static CalendarDay operator -(CalendarDay calDay, int days)
        { return calDay + (-days); }
        public static CalendarDay operator ++(CalendarDay calDay) { return calDay + 1; }
        public static CalendarDay operator --(CalendarDay calDay) { return calDay - 1; }
        public static bool operator ==(CalendarDay x, CalendarDay y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return (x.value == y.value);
        }
        public static bool operator !=(CalendarDay x, CalendarDay y)
        { return !(x == y); }
        public static bool operator >(CalendarDay x, CalendarDay y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.value > y.value;
        }
        public static bool operator <(CalendarDay x, CalendarDay y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.value < y.value;
        }
        public static bool operator >=(CalendarDay x, CalendarDay y)
        { return !(x < y); }
        public static bool operator <=(CalendarDay x, CalendarDay y)
        { return !(x > y); }
        public override bool Equals(object o)
        {
            try { return (this == (CalendarDay)o); }
            catch { return false; }
        }
        public override int GetHashCode()
        { return isDef ? value : 0; }
        #endregion operator overrides

        #region other overrides
        public override string ToString()
        { return isDef ? Date.ToString("dddd, d MMMM yyyy") : "CalendarDay Null"; }
        public string AbbrevDisplay
        { get { return isDef ? Date.ToString("d MMM yy") : "Null"; } }
        #endregion other overrides
    }

    [Serializable]
    public enum IntervalLength 
     {FiveMin = 5, SixMin=6, TenMin=10, 
      TwelveMin=12, FifteenMin=15, TwentyMin=20,
      HalfHour=30, Hour=60, TwoHour=120, ThreeHour=180,
      FourHour=240, SixHour=360, SevenHour=420,
      EightHour = 480, NineHour = 540, 
      HalfDay=720, FullDay=1440}

    [Serializable]
    public class Intervals: List<Interval>
    {
        public Intervals() { }
        public Intervals(Interval firstInterval, 
            TimeSpan periodLength, bool useExtendedIntervals):
            this(firstInterval.StartUtc, periodLength, 
                useExtendedIntervals) { }
        public Intervals(DateTime firstIntervalStartUtc, 
            TimeSpan periodLength, bool useExtendedIntervals)
        {
            var fIU = firstIntervalStartUtc;
            var itvlUtc = new DateTime(fIU.Year, fIU.Month, fIU.Day,
                            fIU.Hour, fIU.Minute, 0, DateTimeKind.Utc);
            var itvlPST = itvlUtc.ToPacificTime();
            var startHr = itvlPST.Hour;
            var singleHours = useExtendedIntervals?
                                48 - ((startHr + 2) % 8):
                                periodLength.TotalHours;
            var endHr = itvlUtc.AddHours(singleHours).ToPacificTime().Hour;
            var dstIndex = (endHr + 2) % 8;
            singleHours +=
                dstIndex == 7 ? 1 :
                dstIndex == 1 ? -1 : 0;
            // -----------------------------
            var lastItvlUtc = itvlUtc.AddHours(singleHours);
            while (itvlUtc < lastItvlUtc)
            {
                Add(Interval.Make(itvlUtc, TimeSpan.FromHours(1)));
                itvlUtc = itvlUtc.AddHours(1f);
            }
            if (!useExtendedIntervals) return;
            // --------------------------------------
            var numEightHrIntvls = 24 - ((startHr + 2) % 24) / 8;
            for (var i = 0; i < numEightHrIntvls; i++)
            {
                dstIndex = (itvlUtc.AddHours(8).ToPacificTime().Hour + 2) % 8;
                var intvlHrs =
                    dstIndex == 7 ? 9:
                    dstIndex == 1 ? 7 : 8;     
                Add(Interval.Make(itvlUtc, TimeSpan.FromHours(intvlHrs)));
                itvlUtc = itvlUtc.AddHours(intvlHrs);
            }
        }
        public Interval FirstInterval
        {
            get
            {
                var first = Interval.Make(DateTime.MaxValue);
                foreach (var itvl in this.Where
                            (itvl => itvl < first))
                    first = itvl;
                return first;
            }
        }
        public Interval LastInterval
        {
            get
            {
                var last = Interval.Make(DateTime.MinValue);
                foreach (var itvl in this.Where
                            (itvl => itvl > last))
                    last = itvl;
                return last;
            }
        }
    }

    public static class IntervalExtensions
    {
        public static Interval NextInterval(this Intervals itvls, Interval itvl)
        {
            var candItvl = Interval.NULL;
            foreach(var itv in itvls)
                if (itv.StartUtc > itvl.StartUtc)
                    if (candItvl.IsNull || itv.StartUtc < candItvl.StartUtc)
                        candItvl = itv;
            return candItvl;
        }
        public static Interval PreviousInterval(this Intervals itvls, Interval itvl)
        {
            var candItvl = Interval.NULL;
            foreach (var itv in itvls)
                if (itv.StartUtc < itvl.StartUtc)
                    if (candItvl.IsNull || itv.StartUtc > candItvl.StartUtc)
                        candItvl = itv;
            return candItvl;
        }
    }
}
