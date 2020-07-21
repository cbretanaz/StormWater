using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CoP.Enterprise.Support;
using lib = CoP.Enterprise.Utilities;

namespace CoP.Enterprise
{
    [Serializable]
    public struct HourInterval : IComparable, IComparable<HourInterval>, IDUIDisplayInterval
    {
        #region explanation
        /*  private integer field value stores Hourly Interval
            as Number Count from ClockHour 0: =  
            Midnight to 1 AM UTC on 1 Jan 2000 
         * ********************* */
        #endregion explanation

        #region constants

        private static readonly TimeZone tz = TimeZone.CurrentTimeZone;
        private const DateTimeKind utc = DateTimeKind.Utc;
        private static readonly DateTime BaseUtcHour =
            new DateTime(2000, 1, 1, 0, 0, 0, 0, utc);
        public static HourInterval NULL;
        public static HourInterval Base = Make(0);
        public static HourInterval MinValue = Make(int.MinValue);
        public static HourInterval MaxValue = Make(int.MaxValue);
        #endregion constants

        #region private fields
        private int itvlId;
        #endregion private fields

        #region ctor / Factories
        private HourInterval(int hourIndex, bool hasVal): this()
        {
            itvlId = hourIndex;
            HasValue = hasVal;
        }
        [Obsolete("Please use factory method Make()")]
        public HourInterval(int hourIndex) : this()
        {
            itvlId = hourIndex;
            HasValue = true;
        }
        /// <summary>
        /// ctor taking string representation of Utc DateTime.
        /// </summary>
        /// <param name="startIntervalUTC">String representation of Utc dateTime 
        /// of start of one-hour interval</param>
        [Obsolete("Please use factory method Make()")]
        public HourInterval(string startIntervalUTC) : this()
        {
            DateTime strtIntvlUTC;
            if (!DateTime.TryParse(startIntervalUTC, out strtIntvlUTC))
                throw new ArgumentException(string.Format(
                        "Cannot parse {0} as a DateTime.", startIntervalUTC),
                    "startIntervalUTC");
            itvlId = (int)Math.Floor((strtIntvlUTC - BaseUtcHour).TotalHours);
            HasValue = true;
        }
        /// <summary>
        /// constructor: assumes local (PPT)
        /// </summary>
        /// <param name="startIntervalDateTime">Utc DateTime. 
        /// if specifically zoned as Utc, assumes local PPT</param>
        [Obsolete("Please use factory method Make()")]
        public HourInterval(DateTime startIntervalDateTime)
            : this()
        {
            var knd=startIntervalDateTime.Kind;
            var startUtc =
                knd == DateTimeKind.Utc ?  startIntervalDateTime:
                knd == DateTimeKind.Local? startIntervalDateTime.ToUniversalTime():
                                           startIntervalDateTime.FromPacificTime() ;
            itvlId = (int)Math.Floor((startUtc - BaseUtcHour).TotalHours);
            HasValue = true;
        }

        #region factories
        public static HourInterval Make(int intervalId)
        { return new HourInterval(intervalId, true); }
        public static HourInterval Make(string startIntervalUTC)
        { return new HourInterval(getIntervalId(startIntervalUTC), true); }
        public static HourInterval Make(DateTime startIntervalDateTime)
        { return new HourInterval(getIntervalId(startIntervalDateTime), true); }
        #endregion factories

        public static HourInterval Parse(string intervalStartHour )
        { return new HourInterval(getIntervalId(intervalStartHour), true); }
        public static bool TryParse(string startIntervalUTC, out HourInterval hourInterval)
        {
            DateTime strtIntvlUTC;
            if (DateTime.TryParse(startIntervalUTC, out strtIntvlUTC))
            {
                hourInterval = Make(startIntervalUTC);
                return true;
            }
            hourInterval = NULL;
            return false;
        }
        private static int getIntervalId(DateTime startIntervalUTC)
        {
            var knd = startIntervalUTC.Kind;
            var startUtc =
                knd == DateTimeKind.Utc ? startIntervalUTC :
                knd == DateTimeKind.Local ? startIntervalUTC.ToUniversalTime() :
                                            startIntervalUTC.FromPacificTime();
            return (int)Math.Floor((startUtc - BaseUtcHour).TotalHours);
        }
        private static int getIntervalId(string startIntervalUTC)
        {
            DateTime strtIntvlUTC;
            if (!DateTime.TryParse(startIntervalUTC, out strtIntvlUTC))
                throw new ArgumentException(string.Format(
                        "Cannot parse {0} as a DateTime.", startIntervalUTC),
                    "startIntervalUTC");
            return getIntervalId(strtIntvlUTC);
        }
        #endregion ctor / Factories

        #region static Factorys
        public static HourInterval Now
        { get { return Make(DateTime.UtcNow); } }
        public static HourInterval ProtectedInterval
        {
            get
            {
                var utcNow = DateTime.UtcNow;
                return Make(utcNow.AddHours(utcNow.Minute < 50 ? 1 : 2));
            }
        }

        public static HourInterval FirstToday
        { get { return Make(DateTime.Now.Date); } }
        public static HourInterval LastToday
        { get { return Make(DateTime.Now.AddDays(1).Date) - 1; } }
        #endregion static Factorys

        #region properties
        #region static properties
        public static HourInterval FirstTomorrow  { get { return Make(DateTime.Today.AddDays(1)); } }
        public static HourInterval FirstYesterday { get { return Make(DateTime.Today.AddDays(-1)); } }
        #endregion static properties

        public bool IsNull { get { return !HasValue; } }
        public bool HasValue { get; private set; }
        public int IntervalId
        {
            get
            {
                if (IsNull) throw new InvalidOperationException(
                    "HourInterval is not defined" );
                return itvlId;
            }
            set
            {
                itvlId = value;
                HasValue = true;
            }
        }
        public HourInterval Tomorrow  { get { return Make(StartPvt.Date.AddDays(1)); } }
        public HourInterval Yesterday { get { return Make(StartPvt.Date.AddDays(-1)); } }
        public HourInterval NextDay { get { return Tomorrow; } }
        public HourInterval PreviousDay { get { return Yesterday; } }
        public HourInterval Next { get { return this + 1; } }
        public HourInterval Previous { get { return this - 1; } }
        public HourInterval StartOfDay { get { return this - HourIndex; } }
        public HourInterval EndOfDay { get { return (this+23) - (this+24).HourIndex; } }
        public LoadHour Load { get { return IsLLH ? LoadHour.LLH : LoadHour.HLH; } }
        public bool IsHLH { get { return HourEnding > 6 && HourEnding < 23; } }
        public bool IsLLH { get { return HourEnding <= 6 || HourEnding >= 23; } }

        #region DateTime properties
        public DateTime StartUtc
        { get { return BaseUtcHour.AddHours(itvlId); } }
        public DateTime EndUtc
        { get { return StartUtc.AddHours(1); } }
        public DateTime StartPvt
        { get { return StartUtc.ToPacificTime(); } }
        public DateTime EndPvt
        { get { return EndUtc.ToPacificTime(); } }
        public DateTime Date { get { return StartPvt.Date; } }
        public int Year { get { return StartPvt.Year; } }
        public int Month { get { return StartPvt.Month; } }
        public int Day { get { return StartPvt.Day; } }
        public bool IsFallBack { get { return Date.IsDaylightSavingTime() && !Date.AddHours(4).IsDaylightSavingTime(); } }
        public bool IsSpringForward { get { return !Date.IsDaylightSavingTime() && Date.AddHours(4).IsDaylightSavingTime(); } }
        public DayOfWeek DayOfWeek { get { return StartPvt.DayOfWeek; } }
        public int ClockHour { get { return StartPvt.Hour; } }
        public int HourIndex { get { return (int)(StartUtc - StartPvt.Date.FromPacificTime()).TotalHours; } }
        public HourOfDay HourOfDay
        {
            get
            {
                return ClockHour == 2 ?
                    (HourIndex == 3 ? HourOfDay.H2X : HourOfDay.H02) :
                    ((HourOfDay)ClockHour);    
            }
        }
        public int HourEnding { get { return EndPvt.Hour; } }
        public string HE { get { return (
            EndPvt.Hour == 0 ? 24 : 
            HourIndex == 1 && (IsFallBack || IsSpringForward)? 2: 
            EndPvt.Hour).ToString("'HE'00"); } }
        public string HE2X { get { return IsFallBack && HourIndex == 2 ? "HE2X" : HE; } }
        public string HE25 { get { return IsFallBack && HourIndex == 2 ? "HE25" : HE; } }
        #endregion DateTime properties
        #endregion properties

        #region utility Methods
        public HourInterval NextDayOfWeek(DayOfWeek dayOfWeek)
        {
            var offset = 1 + ((6 + (int)dayOfWeek - (int)StartPvt.DayOfWeek) % 7);
            return Make(StartPvt.Date.AddDays(offset).ToUniversalTime());
        }
        public HourInterval NextEndOfDayOfWeek(DayOfWeek dayOfWeek)
        {
            var offset = (7 + (int)dayOfWeek - (int)StartPvt.DayOfWeek) % 7;
            return Make(StartPvt.Date.AddDays(offset).ToUniversalTime()).EndOfDay;
        }
        public HourInterval PreviousDayOfWeek(DayOfWeek dayOfWeek)
        {
            var offset = 1+(6 + (int) StartPvt.DayOfWeek - (int) dayOfWeek)%7;
            return Make(StartPvt.Date.AddDays(-offset).ToUniversalTime());
        }
        public HourInterval AddDays(int daysToAdd) { return AddDays(daysToAdd, false); }
        public HourInterval AddDays(int daysToAdd, bool startOfDay)
        { return (StartOfDay + 3 + (24 * daysToAdd)).StartOfDay + (startOfDay? 0: HourIndex); }
        public IntervalRange Range(HourInterval otherInterval)
        { 
            return otherInterval > this ? 
               IntervalRange.Make(this, 1 + (otherInterval - this)):
               IntervalRange.Make(otherInterval, 1 + (this - otherInterval)); }
        public HourInterval GetValueOrDefault(HourInterval defaultInterval)
        {
            if (!defaultInterval.HasValue) throw new ArgumentNullException(
                "defaultInterval", "Default Hour Interval must have a value");
            return HasValue ? this : defaultInterval;
        }
        public bool Between(HourInterval start, HourInterval end, bool exclusive = false)
        { return start.HasValue && end.HasValue && 
            (exclusive ? this > start && this < end: this >= start && this <= end); }
        #endregion utility Methods

        #region IComparable Members
        public int CompareTo(object obj)
        {
            if (!(obj is HourInterval))
                throw new ArgumentException(string.Format(
                    "Object {0} is not a HourInterval.", obj));
            var oHour = (HourInterval)obj;
            if (IsNull || oHour.IsNull)
                throw new ArgumentNullException();
            return itvlId.CompareTo(oHour.itvlId);
        }
        public int CompareTo(HourInterval intvl)
        {
            if (IsNull)
                throw new NullReferenceException("Cannot Compare Null HourIntervals.");
            if(intvl.IsNull)
                throw new ArgumentNullException("intvl");
            return itvlId.CompareTo(intvl.itvlId);
        }
        #endregion
        
        #region operator overrides
        public static HourInterval operator +(HourInterval intvl, TimeSpan timSpan)
        { return intvl + timSpan.Hours; }
        public static HourInterval operator -(HourInterval intvl, TimeSpan timSpan)
        { return intvl - timSpan.Hours; }
        public static HourInterval operator +(HourInterval hourIntvl, int hours)
        { return new HourInterval(hourIntvl.itvlId + hours, true); }
        public static HourInterval operator -(HourInterval hourIntvl, int hours)
        { return hourIntvl + (-hours); }
        public static int operator -(HourInterval endIntvl, HourInterval startIntvl)
        { return endIntvl.IntervalId - startIntvl.IntervalId; }
        public static HourInterval operator ++(HourInterval hrIntvl){ return hrIntvl + 1; }
        public static HourInterval operator --(HourInterval hrIntvl) { return hrIntvl - 1; }

        public static bool operator ==(HourInterval x, HourInterval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return (x.itvlId == y.itvlId);
        }
        public static bool operator !=(HourInterval x, HourInterval y)
        { return !(x == y); }
        public static bool operator > (HourInterval x, HourInterval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.itvlId > y.itvlId;
        }
        public static bool operator < (HourInterval x, HourInterval y)
        {
            if (x.IsNull || y.IsNull) throw new ArgumentNullException();
            return x.itvlId < y.itvlId;
        }
        public static bool operator >=(HourInterval x, HourInterval y)
        { return !(x < y); }
        public static bool operator <=(HourInterval x, HourInterval y)
        { return !(x > y); }
        public override bool Equals(object o)
        {
            try { return (this == (HourInterval)o); }
            catch { return false; }
        }
        public override int GetHashCode()
        { return HasValue ? itvlId : 0; }
        #endregion operator overrides

        #region other overrides
        public override string ToString()
        { return ToString(DispFormat.Short); }
        public string ToString(DispFormat dFrmt)
        {
            switch(dFrmt)
            {
                case DispFormat.Long:
                    return HasValue? string.Format(
                            "{0:d MMM yyyy HH:'00'} Utc: " +
                            "{1:d MMM yyyy HH:'00'} PPT", 
                            StartUtc, StartPvt):
                        "Interval Null";
                case DispFormat.HE:
                    return HasValue? string.Format(
                            "{0:d MMM yy} {1} PPT", 
                            StartPvt, HE):
                        "Interval Null";

                case DispFormat.HE25:
                    return HasValue ? string.Format(
                            "{0:d MMM yy} {1} PPT",
                            StartPvt, HE25) :
                        "Interval Null";
                default:
                    return HasValue? string.Format(
                            "{0:d MMM yyyy HH:'00'} Utc", 
                            StartUtc):
                        "Interval Null";
            }
        }
        public string AbbrevDisplay
        { get { return HasValue ? StartUtc.ToString("HH'00,'d MMM yyyy") : "Null"; } }
        #endregion other overrides

        public static DateTime GetStartTimeLocal(DateTime anyTimeUtc)
        {
            var localDate = anyTimeUtc.ToPacificTime().Date;
            return localDate.AddDays(1 - localDate.Day);
        }
        public HourInterval AddHours(int hours) { return new HourInterval(itvlId + hours, true); }
        public enum DispFormat {Long, Short, HE, HE25}
    }

    [Serializable]
    public class HourIntervals : List<HourInterval>
    {
        private int Hrs;       

        public HourIntervals() { }
        public HourIntervals(HourInterval firstInterval, int numIntervals) 
        {
            for (var ndx=0; ndx < numIntervals; ndx++)
                Add(firstInterval + ndx);
        }

        public static HourIntervals Make(
          HourInterval startInterval, int hours)
        { return new HourIntervals(startInterval, hours); }

        
        [XmlIgnore]
        public HourInterval FirstInterval
        {
            get
            {
                var first = HourInterval.Make(DateTime.MaxValue);
                foreach (var itvl in this.Where
                            (itvl => itvl < first))
                    first = itvl;
                return first;
            }
        }

     
        [XmlIgnore]
        public HourInterval LastInterval
        {
            get
            {
                var last = HourInterval.Make(DateTime.MinValue);
                foreach (var itvl in this.Where
                            (itvl => itvl > last))
                    last = itvl;
                return last;
            }
        }

        public int Hours { get { return Hrs; } set { Hrs = value; } }
    }

    [Serializable]
    [DataContract(Name = "IntervalRange")]
    public class IntervalRange
    {
        #region private fields
        protected int strtItvlId;
        protected int cnt;
        #endregion private fields

        #region ctor / factorys
        protected IntervalRange() { }
        protected IntervalRange(HourInterval itvlA, HourInterval itvlB)
        {
            strtItvlId = (itvlA <= itvlB ? itvlA : itvlB).IntervalId;
            Length = 1 + (itvlA <= itvlB ? itvlB - itvlA :
                                          itvlA - itvlB);
        }
        public IntervalRange(int startIntervalId, int length)
        {
            if (length<0)
                throw new ArgumentOutOfRangeException(
                    "length", "length of Interval Range may not be negative.");
            strtItvlId = startIntervalId;
            Length = length;
        }
        public IntervalRange(HourInterval startInterval, int length)
            : this(startInterval.IntervalId, length) { }
        public static IntervalRange Make(
            HourInterval startInterval, int length)
        { return new IntervalRange(startInterval, length); }
        public static IntervalRange Make(HourInterval itvlA, HourInterval itvlB)
        { return new IntervalRange(itvlA, itvlB); }
        #endregion ctor / factorys

        public bool Contains (HourInterval interval)
        { return interval >= StartInterval && interval <= LastInterval; }
        public static IntervalRange Current
        { get { return Make(HourInterval.Now.Next, 1); } }
        public HourInterval StartInterval
        {
            get { return HourInterval.Make(strtItvlId); }
            set { strtItvlId = value.IntervalId; }
        }

        [DataMember(Name = "StartUtc", IsRequired = true, Order=0)]
        public DateTime StartUtc
        {
            get { return StartInterval.StartUtc; }
            set { strtItvlId = HourInterval.Make(value).IntervalId; }
        }
        public HourInterval LastInterval
        { get { return HourInterval.Make(strtItvlId + Length - 1); } }
        [DataMember(Name = "Length", IsRequired = true, Order = 1)]
        public int Length { get { return cnt; } set { cnt = value; } }
        public IEnumerable<HourInterval> Intervals
        {
            get
            {
                for (var ndx = 0; ndx < Length; ndx++)
                    yield return StartInterval + ndx;
            }
        }
        public virtual string ToString(string format)
        {
            if (string.IsNullOrEmpty(format)) return ToString();
            switch (format.ToLower())
            {
                case "local":
                case "lcl":
                    return string.Format(
                        "Interval Range: {0:MMM d, yyyy} {1} to " +
                        "{2:MMM d, yyyy} {3}, {4} intervals",
                        StartInterval.StartPvt, StartInterval.HE, 
                        LastInterval.EndPvt, LastInterval.HE, Length);
                case "simlocal": case "simlcl":
                    return string.Format(
                        "Simulation Period: {0:MMM d, yyyy} {1} to " +
                        "{2:MMM d, yyyy} {3}, {4} intervals",
                        StartInterval.StartPvt, StartInterval.HE,
                        LastInterval.EndPvt, LastInterval.HE, Length);
                default: return ToString();
            }
        }
        public override string ToString()
        {
            return string.Format(
              "Interval Range: {0} to {1}: {2} intervals",
               StartInterval, LastInterval, cnt);
        }
    }

    [Serializable]
    public enum HourOfDay : byte 
     { H00=0,H01=1,H02=2,H03=3,H04=4,
       H05=5,H06=6,H07=7,H08=8,H09=9,
       H10=10,H11=11,H12=12,H13=13,H14=14,
       H15=15,H16=16,H17=17,H18=18,H19=19,
       H20=20,H21=21,H22=22,H23=23,H2X=25}

    [Serializable]
    public enum LoadHour{LLH, HLH}
}