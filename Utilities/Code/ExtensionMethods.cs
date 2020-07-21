using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using lib = CoP.Enterprise.Utilities;
using cryp = CoP.Enterprise.Crypto;
using sysTx = System.Transactions;
using sd = CoP.Enterprise.SerializableDictionary<string, object>;
#pragma warning disable 414

namespace CoP.Enterprise
{
    public static class DateTimeExt
    {
        private const DayOfWeek sat = DayOfWeek.Saturday;
        private const DayOfWeek sun = DayOfWeek.Sunday;

#pragma warning disable 414
        private static StringComparison icic = StringComparison.InvariantCultureIgnoreCase;
#pragma warning restore 414
            
        public static DateTime SetKind(this DateTime inValue, DateTimeKind kind)
        {
            const DateTimeKind
                unSpec = DateTimeKind.Unspecified,
                local = DateTimeKind.Local,
                utc = DateTimeKind.Utc;
            if (kind == unSpec) throw 
                new InvalidOperationException(
                    "DateTime Kind is already unspecified. " +
                    "Cannot set DateTime Kind to unspecified.");
            switch (inValue.Kind)
            {
                case local: return kind == local? inValue: inValue.ToUniversalTime();
                case utc:   return kind == utc ?  inValue: inValue.ToPacificTime();
                default:    return DateTime.SpecifyKind(inValue, kind);
            }
        }
        
        #region timezone utilities
        public static DateTime ToPacificTime(this DateTime inValue)
        { return inValue.ToTimeZoneTime("Pacific Standard Time"); }       
        public static DateTime ToTimeZoneTime(this DateTime inValue, string timeZoneId)
        {
            var newDt =  TimeZoneInfo.ConvertTimeBySystemTimeZoneId(inValue, "UTC", timeZoneId);
            return (timeZoneId.Equals(TimeZoneInfo.Local.Id, StringComparison.InvariantCultureIgnoreCase)) ?
                newDt.SetKind(DateTimeKind.Local) : newDt;
        }        
        public static DateTime FromPacificTime(this DateTime inPvt)
        { return inPvt.FromTimeZoneTime("Pacific Standard Time"); }
        public static DateTime FromTimeZoneTime(this DateTime inPvt, string timeZoneId)
        {
            var tz = timeZoneId.Equals("UTC", StringComparison.InvariantCultureIgnoreCase)? TimeZoneInfo.Utc:
                timeZoneId.Equals(TimeZoneInfo.Local.Id, StringComparison.InvariantCultureIgnoreCase)? TimeZoneInfo.Local:
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);    
            return TimeZoneInfo.ConvertTimeToUtc(inPvt, tz);
        }
        #endregion timezone utilities

        public static DateTime FirstOfMonth(this DateTime inDT)
        { return inDT.Date.AddDays(1 - inDT.Day); }

        public static DateTime StartOfMonth(this DateTime inDT)
        { return inDT.FirstOfMonth(); }
        public static DateTime EndOfMonth(this DateTime inDT)
        { return inDT.Date.AddDays(DateTime.DaysInMonth(inDT.Year, inDT.Month)-inDT.Day); }

        public static DateTime FirstOfNextMonth(this DateTime inDT)
        { return inDT.FirstOfMonth().AddMonths(1); }

        public static DateTime NextDayOfWeek(this DateTime inDT, DayOfWeek dow, 
            bool allowDirectHit = false)
        {
            var offset = allowDirectHit?
                ((7 + (int)dow - (int)inDT.DayOfWeek) % 7):
                1 + ((6 + (int)dow - (int)inDT.DayOfWeek) % 7);
            return inDT.Date.AddDays(offset);
        }

        public static DateTime BusinessDayOfWeek(this DateTime inDT, DayOfWeek dow, 
                    bool throwIfWeekend = false)
        {
            if (throwIfWeekend && new[]{sat, sun}.Contains(inDT.DayOfWeek))
                throw new ArgumentOutOfRangeException(nameof(inDT), 
                    "This method only may be called for a business day (Monday through Friday).");
            if (dow == sat || dow == sun)
                throw new ArgumentOutOfRangeException(nameof(dow), 
                    "Business days are defined as Monday through Friday.");
            var offset = (int)dow - (int)inDT.DayOfWeek;
            return inDT.Date.AddDays(offset);
        }

        public static int DaysInMonth(this DateTime inDt)
        { return DateTime.DaysInMonth(inDt.Year, inDt.Month);}

        public static DateTime NextBillDateLegacy(this DateTime inDt)
        {
            if (inDt.Day > 28) inDt = inDt.AddDays(1 + inDt.DaysInMonth() - inDt.Day);
            var tDt = inDt.AddMonths(1).AddDays(-20);
            var y = -(((int)tDt.DayOfWeek + 3) % 7);
            return tDt.AddDays(y);
        }
         
        public static DateTime NextBillDate(this DateTime inDt)
        {
            if (inDt.Day > 28) inDt = inDt.AddDays(1 + inDt.DaysInMonth() - inDt.Day);
            return inDt.AddDays(inDt.DaysInMonth() - 27)
                .NextDayOfWeek(DayOfWeek.Thursday);
        }

        public static string DateRangeDescription(this DateTime startDt, DateTime? endDt)
        {
            var dt1 = startDt.Date;
            var dt2 = !endDt.HasValue|| endDt.Value==startDt? startDt : endDt.Value.Date ;
            // ------------------------------------------------------
            return dt2 == dt1 ? $"{dt1:d MMM yyyy}":
                   dt2.Year == dt1.Year && dt2.Month == dt1.Month ?
                        $"{dt1.Day:0} - { dt2:d MMM yyyy}":
                   dt2.Year == dt1.Year? 
                        $"{dt1:d MMM}-{dt2:d MMM yyyy}" :
                        $"{dt1:d MMM yyyy}-{dt2:d MMM yyyy}";
        }
    }

    public static class StringExt
    {
        public static string ExtractNumeric(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var result = new char[s.Length];
            var resultIndex = 0;
            foreach (var c in s.Where(char.IsNumber))
                result[resultIndex++] = c;
            return resultIndex == 0 ? string.Empty :
                resultIndex == result.Length ? s :
                new string(result, 0, resultIndex);
        }
        public static bool IsNullOrEmpty(this string s)
        { return string.IsNullOrEmpty(s); }

        public static string Truncate(this string s, int maxLen)
        { return string.IsNullOrWhiteSpace(s)? string.Empty:
                s.Length > maxLen ? s.Substring(0, maxLen) : s; }

        public static bool Contains(this string s, string searchValue, 
            StringComparison comparison)
        { return !string.IsNullOrWhiteSpace(s) &&
                 !string.IsNullOrWhiteSpace(searchValue) &&
                 s.IndexOf(searchValue, comparison) >= 0; }

        public static string Right(this string s, int maxLen)
        { return s.Length < maxLen ? s: s.Substring(s.Length - maxLen, maxLen); }

        public static string ShortenForDisplay(this string target, int length)
        { return string.IsNullOrWhiteSpace(target)? string.Empty:
            target.Length <=length? target: target.Substring(0, length - 3) + "..."; }


        #region Des, triple Des encryption methods
        public static string EncryptDes(this string s) { return cryp.Encrypt(s); }
        public static string EncryptDes(this string s, string key)
        { return cryp.Encrypt(s, key, null); }
        public static string EncryptDes(this string s, string key, string iv)
        { return cryp.Encrypt(s, key, iv); }
        public static string DecryptDes(this string s) { return cryp.Decrypt(s); }
        public static string DecryptDes(this string s, string key)
        { return cryp.Decrypt(s, key, null); }
        public static string DecryptDes(this string s, string key, string iv)
        { return cryp.Decrypt(s, key, iv); }
        // -----------------------------------
        public static string EncryptTripleDes(this string s) { return cryp.EncryptTripleDES(s); }
        public static string EncryptTripleDes(this string s, string key)
        { return cryp.EncryptTripleDES(s, key, null); }
        public static string EncryptTripleDes(this string s, string key, string iv)
        { return cryp.EncryptTripleDES(s, key, iv); }
        public static string DecryptTripleDes(this string s) { return cryp.DecryptTripleDES(s); }
        public static string DecryptTripleDes(this string s, string key)
        { return cryp.DecryptTripleDES(s, key, null); }
        public static string DecryptTripleDes(this string s, string key, string iv)
        { return cryp.DecryptTripleDES(s, key, iv); }
        #endregion Des, triple Des encryption methods
    }

    public static class DoubleFloatExt
    {
        public static bool IsNumeric(this float val)  { return !float.IsNaN(val); }
        public static bool IsNumeric(this float? val) { return val.HasValue && IsNumeric(val.Value); }
        public static bool IsNumeric(this double val)   { return !double.IsNaN(val); }
        public static bool IsNumeric(this double? val)  { return val.HasValue && IsNumeric(val.Value); }
        // ------------------------------------------------------------------------------------------
        public static bool IsIntegral(this float val) { return IsNumeric(val) && val == Math.Round(val); }
        public static bool IsIntegral(this double val)  { return IsIntegral(val); }
        public static bool IsIntegral(this float? val){ return val.HasValue && IsNumeric(val) && val.Value == Math.Round(val.Value); }
        public static bool IsIntegral(this double? val) { return IsIntegral(val); }
    }

    public static class ListBoxExtensions
    {
        public static void SetupWithEnum(this ComboBox cb, Type enumTyp, 
            object value, bool showNullAsBlank = false)
        {
            cb.DataSource = null;
            cb.Items.Clear();
            cb.Font = new Font("Consolas", 8f, FontStyle.Regular);
            cb.DataSource = Enum.GetValues(enumTyp);
            foreach (var val in cb.Items.Cast<object>().Where(
                val => val.Equals(value)))
                cb.SelectedItem = val;
        }

        public static void AutoSearch(this ComboBox cb, 
            KeyPressEventArgs e, bool blnLimitToList)
        {
            string strFindStr;
            if (e.KeyChar == (char)8) // BS
            {
                if (cb.SelectionStart <= 1)
                {
                    cb.Text = "";
                    cb.SelectedIndex = -1;
                    return;
                }
                strFindStr = cb.SelectionLength == 0 ?
                    cb.Text.Substring(0, cb.Text.Length - 1) :
                    cb.Text.Substring(0, cb.SelectionStart - 1);
            }

            else
            {
                if (cb.SelectionLength == 0)
                    strFindStr = cb.Text + e.KeyChar;
                else
                    strFindStr = cb.Text.Substring(0, cb.SelectionStart) + e.KeyChar;
            }
            // ---------------------------------------
            var intIdx = cb.FindString(strFindStr);
            if (intIdx != -1)
            {
                cb.SelectedText = "";
                cb.SelectedIndex = intIdx;
                cb.SelectionStart = strFindStr.Length;
                cb.SelectionLength = cb.Text.Length;
                e.Handled = true;
                cb.Tag = cb.Items[intIdx];
            }
            else
            {
                cb.SelectedIndex = -1;
                cb.Text = strFindStr;
                cb.Tag = blnLimitToList? null: cb.Text;
                e.Handled = blnLimitToList;
            }
        }

        public static CheckedListBox.ObjectCollection UncheckedItems(this CheckedListBox lb)
        {
            var uchkItms = new CheckedListBox.ObjectCollection(lb);
            for (int i = 0; i < lb.Items.Count; i++)
            {
                if (!lb.GetItemChecked(i))
                    uchkItms.Add(lb.Items[i]);
            }
            return uchkItms;   
        }
    }

    public static class ADONetExtensions
    {
        public static T? ColumnValue<T>(this DataRow dr, string columnName) where T : struct
        { return (dr.IsNull(columnName) ? null : (T?)(T)dr[columnName]); }
        public static double? DoubleValue(this DataRow dr, string columnName)
        { return dr.ColumnValue<double>(columnName); }

        public static DataView Find(this DataTable dt, string filter)
        {
            var vw = dt.DefaultView;
            vw.RowFilter = filter;
            return vw;
        }
    }

    public static class ExceptionExtensions
    {
        private static readonly string sNL = Environment.NewLine;
        public static string Details(this Exception eX, 
            string message = null, bool includeTime = false)
        {
            var sbXMsg = new StringBuilder(
                (includeTime? DateTime.Now.ToLongTimeString() + "   ": "")
                 + message);
            var X = eX; var eXLevel = 0;
            while (X != null)
            {
                sbXMsg.Append(string.Format(sNL +
                    string.Empty.PadRight(70, '-') + sNL +
                    "{0} Exception {1}, {2}" + sNL +
                    "Stack Trace: " + "{3}",
                    (eXLevel == 0 ? "Main " : lib.Ordinal(eXLevel) + " inner "),
                    X.GetType(), X.Message, X.StackTrace));
                X = X.InnerException;
                eXLevel++;
            }
            return sbXMsg.ToString();
        }
    }

    public static class LinkedListNodeExtentions
    {
        public static LinkedListNode<object> NextOrFirst(this LinkedListNode<object> nod)
        { return nod.Next?? nod.List.First; }
        public static LinkedListNode<object> PreviousOrLast(this LinkedListNode<object> nod)
        { return nod.Previous?? nod.List.Last; }
    }

    public static class DataRowExtensions
    {
        private static StringComparison icic =
            StringComparison.InvariantCultureIgnoreCase;
        /// <summary>
        /// Fetches value from data row
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="row">ADO.Net DataRow object</param>
        /// <param name="columnName">string name of column to fetch</param>
        /// <param name="defaultValue">value to use if column is null and nulls are allowed</param>
        /// <param name="allowNulls">whether to allow nulls</param>
        /// <returns>DataRow value</returns>
        public static T Get<T>(this DataRow row, string columnName, 
            T defaultValue = default(T), bool allowNulls = true)
        {
            // validate the column
            if (!row.Table.Columns.Contains(columnName))
                throw new DataException(string.Format(
                    "The referenced DataTable does not contain column {0}", 
                    columnName));

            // get the value and handle nulls
            var val = row[columnName];
            if (!allowNulls && val is DBNull)
                throw new DataException(string.Format(
                    "DataRow contains a column {0} but it cannot be NULL", 
                    columnName));
            if (val is string && string.IsNullOrWhiteSpace(val.ToString())) return defaultValue;
            if (val is DBNull) return defaultValue;

            var type = typeof(T);

            // enum types must be cast to Enum's underlying type first
            if (type.IsEnum)
                return (T)Convert.ChangeType(val, 
                    Enum.GetUnderlyingType(type));

            // nullable types must be converted through their 
            // underlying types (otherwise we'll get exception)
            var underlyingType = Nullable.GetUnderlyingType(type);
            return (T)Convert.ChangeType(val, underlyingType ?? type);
        }

        /// <summary>
        /// Fetches value from data row
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="row">ADO.Net DataRow object</param>
        /// <param name="index">integral index of column in datarow</param>
        /// <param name="defaultValue">value to use if column is null and nulls are allowed</param>
        /// <param name="allowNulls">whether to allow nulls</param>
        /// <returns>DataRow value</returns>
        public static T Get<T>(this DataRow row, int index, 
            T defaultValue = default(T), bool allowNulls = true)
        {
            // validate the column
            if (index < 0 || index >= row.Table.Columns.Count)
                throw new DataException(string.Format(
                    "DataRow does not contain column {0}", index));

            if (index >= row.Table.Columns.Count)
                throw new DataException(string.Format(
                    "DataRow only has {0} columns. it does not contain column {1}",
                    row.Table.Columns.Count, index));

            // get the value and handle nulls
            var val = row[index];
            if (!allowNulls && val is DBNull)
                throw new DataException(string.Format(
                    "DataRow contains a column #{0} but it cannot be NULL", index));
            if (val is DBNull) return defaultValue;

            var type = typeof(T);
            // enum types must be cast to Enum's underlying type first
            if (type.IsEnum)
                return (T)Convert.ChangeType(val,
                    Enum.GetUnderlyingType(type));

            // nullable types must be converted through their 
            // underlying types (otherwise we'll get exception)
            var underlyingType = Nullable.GetUnderlyingType(type);
            return (T)Convert.ChangeType(val, underlyingType ?? type);
        }

        public static T StripNull<T>(this DataRow dr, string colNm)
        {
            var obj = dr[colNm];
            return obj == DBNull.Value || obj == null ?
                default(T) : (T) obj;
        }
        public static string StripAndFormat(this DataRow dr, string colNm, int len)
        {
            return dr.StripNull<string>(colNm)
                .Truncate(len).PadRight(len);
        }

        public static string StripEnum<T>(this DataRow dr, 
            string colNm, int len)
        {
            var strVal = dr[colNm].ToString();

            return 
                string.IsNullOrWhiteSpace(strVal) ||
                strVal.Equals("0", icic)? "  ":
                Enum.GetName(typeof(T), dr[colNm])
                .Right(len).PadLeft(len);
        }

        public static string StripDecimalNull(this DataRow dr, 
            string colNm, string formatSpec, int len)
        { return dr.StripNull<decimal>(colNm)
                .ToString(formatSpec).PadLeft(len); }
        public static string StripDateTimeNull(this DataRow dr, 
            string colNm, string formatSpec)
        { return dr.StripNull<DateTime?>(colNm)?
                .ToString(formatSpec)?? "00000000"; }

    }

    public static class DataRowViewExtensions
    {
        public static T Get<T>(this DataRowView row, string columnName,
            T defaultValue = default(T), bool allowNulls = true)
        {
            // validate the column
            if (!row.Row.Table.Columns.Contains(columnName))
                throw new DataException(
                    $"The referenced DataTable does not contain column {columnName}");
            // get the value and handle nulls
            var val = row[columnName];
            if (!allowNulls && val is DBNull)
                throw new DataException(
                    $"DataTable contains column {columnName} but it cannot be NULL");
            if (val is DBNull) return defaultValue;

            var type = typeof(T);

            // enum types must be cast to Enum's underlying type first
            if (type.IsEnum)
                return (T)Convert.ChangeType(val,
                    Enum.GetUnderlyingType(type));

            // nullable types must be converted through their 
            // underlying types (otherwise we'll get exception)
            var underlyingType = Nullable.GetUnderlyingType(type);
            return (T)Convert.ChangeType(val, underlyingType ?? type);
        }
    
    public static T GetWNull<T>(this DataRowView row, string columnName,
        T defaultValue = default(T), bool allowNulls = true,
        bool nullIfMissing = false)
    {
        // validate the column
        if (!row.Row.Table.Columns.Contains(columnName))
        {
            if (nullIfMissing) return defaultValue;
            throw new DataException(
                $"The referenced DataTable does not contain column {columnName}");
        }
        // get the value and handle nulls
        var val = row[columnName];
        if (!allowNulls && val is DBNull)
            throw new DataException(
                $"DataTable contains column {columnName} but it cannot be NULL");
        if (val is DBNull) return defaultValue;

        var type = typeof(T);

        // enum types must be cast to Enum's underlying type first
        if (type.IsEnum)
        {
            try
            {
                return val is string && Enum.IsDefined(type, val) ?
                    (T) Enum.Parse(type, (string) val) :
                    (T) Convert.ChangeType(val, Enum.GetUnderlyingType(type));
            }
            catch (Exception x)
            {
                var s = x.Message;
                throw;
            }
        }

        // nullable types must be converted through their 
        // underlying types (otherwise we'll get exception)
        var underlyingType = Nullable.GetUnderlyingType(type);
        return (T)Convert.ChangeType(val, underlyingType ?? type);
    }
}

    public static class IEnumerableExtensions
    {
        #region Generating sequences

        public static IEnumerable<T> Generate<T>(Func<T> generator) where T : class
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            T t;
            while ((t = generator()) != null) yield return t;
        }

        public static IEnumerable<T> Generate<T>(Func<T?> generator) where T : struct
        {
            if (generator == null) throw new ArgumentNullException("generator");

            T? t;
            while ((t = generator()).HasValue) yield return t.Value;
        }

        public static IEnumerable<T> FromEnumerator<T>(IEnumerator<T> enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");

            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        public static IEnumerable<T> Single<T>(T value)
        {
            return Enumerable.Repeat(value, 1);
        }

        public static IEnumerable<int> Gaps(this IEnumerable<int> nums)
        {
            int? last = null;
            foreach (var v in nums)
            {
                if (last.HasValue)
                    for (var j = last.Value + 1; j < v; j++)
                        yield return j;
                last = v;
            }
        }

        #endregion Generating sequences

        #region I/O

        public static IEnumerable<string> ReadLinesFromFile(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            using (var file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null) yield return line;
            }
        }

        public static IEnumerable<string> ReadLinesFromConsole()
        {
            return ReadLinesFrom(Console.In);
        }

        public static IEnumerable<string> ReadLinesFrom(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            return Generate(() => reader.ReadLine());
        }

        public static void WriteLinesTo<T>(this IEnumerable<T> lines, TextWriter writer)
        {
            if (lines == null) throw new ArgumentNullException("lines");
            if (writer == null) throw new ArgumentNullException("writer");

            lines.ForEach(line => writer.WriteLine(line.ToString()));
        }

        public static void WriteLinesToConsole<T>(this IEnumerable<T> lines)
        {
            lines.WriteLinesTo(Console.Out);
        }

        public static void WriteLinesToFile<T>(this IEnumerable<T> lines, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            using (TextWriter file = new StreamWriter(path)) lines.WriteLinesTo(file);
        }

        #endregion I/O

        #region Side Effects

        /// <summary>
        /// Execute a specified Action method on each of a list of objects 
        /// of Type T and return the list of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the list</typeparam>
        /// <param name="source">A cvollection of objects of type T</param>
        /// <param name="action">The void method [Action] to execute on each object</param>
        /// <returns>the list of objects</returns>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var elem in source)
            {
                action(elem);
                yield return elem;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (T elem in source) action(elem);
        }

        #endregion Side Effects

        #region ToStringPretty

        public static string ToStringPretty<T>(this IEnumerable<T> source,
            string delimiter = ",")
        {
            return ToStringPretty(source, "", delimiter, "");
        }

        public static string ToStringPretty<T>(this IEnumerable<T> source,
            string before, string delimiter, string after)
        {
            if (source == null) throw new ArgumentNullException("source");

            var result = new StringBuilder();
            result.Append(before);

            var firstElement = true;
            foreach (var elem in source)
            {
                if (firstElement) firstElement = false;
                else result.Append(delimiter);

                result.Append(elem);
            }

            result.Append(after);
            return result.ToString();
        }

        #endregion ToStringPretty

        #region statistical functions

        public static double Variance(this ICollection<decimal> source)
        {
            var avg = source.Average();
            var runningSum = source.Cast<int>().Sum(value => (value - avg)*(value - avg));
            return Convert.ToDouble(runningSum/source.Count());
        }

        public static double StdDeviation(this ICollection<double> source)
        {
            return Math.Sqrt(source.Variance());
        }

        public static double Variance(this ICollection<double> source)
        {
            var avg = source.Average();
            var runningSum = source.Cast<int>().Sum(value => (value - avg)*(value - avg));
            return Convert.ToDouble(runningSum/source.Count());
        }

        public static double StdDeviation(this ICollection<decimal> source)
        {
            return Math.Sqrt(source.Variance());
        }

        #endregion statistical functions

        #region Other

        public static IEnumerable<TOut> Combine<TIn1, TIn2, TOut>(
            this IEnumerable<TIn1> in1, IEnumerable<TIn2> in2,
            Func<TIn1, TIn2, TOut> func)
        {
            if (in1 == null) throw new ArgumentNullException("in1");
            if (in2 == null) throw new ArgumentNullException("in2");
            if (func == null) throw new ArgumentNullException("func");

            using (var e1 = in1.GetEnumerator())
            using (var e2 = in2.GetEnumerator())
                while (e1.MoveNext() && e2.MoveNext())
                    yield return func(e1.Current, e2.Current);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return Shuffle(source, new Random(DateTime.Now.Millisecond));
        }

        public static IEnumerable<T> Shuffle<T>(
            this IEnumerable<T> source, Random random)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (random == null) throw new ArgumentNullException("random");

            var lst = source.ToList();
            while (lst.Count > 0)
            {
                var n = random.Next(lst.Count);
                yield return lst[n];
                lst.Remove(lst[n]);
            }
        }

        public static string PipeList<T>(this IEnumerable<T> list)
        {
            return ItemList(list, "|");
        }

        public static string ItemList<T>(this IEnumerable<T> list, string delimiter)
        {
            if (list == null) return null;
            var sb = new StringBuilder();
            foreach (T val in list)
                sb.Append(val + delimiter);
            return sb.Length == 0 ? null :
                sb.ToString(0, sb.Length - delimiter.Length);
        }

        public static string Combine<T>(this IEnumerable<T> list,
            string delimeter, string format = null)
        {
            if (list == null)
                return null;
            var builder = new StringBuilder();
            foreach (T item in list)
            {
                if (builder.Length != 0)
                    builder.Append(delimeter);
                if (format == null)
                    builder.Append(item);
                else
                    builder.AppendFormat(format, item);
            }
            return builder.Length == 0 ? null : builder.ToString();
        }

        public static bool Contains(this IEnumerable<string> stringList,
            string value, StringComparison comparison)
        {
            return stringList.Any(s => s.Equals(value, comparison));
        }

        /// <summary>
        /// Generate cartesian product of tuples of one 
        ///  object from extended list and one object from other list
        /// </summary>
        /// <typeparam name="T">Type of objects in list</typeparam>
        /// <param name="elist">List of Ts</param>
        /// <param name="includeMatches">whether to include tuples with identical members</param>
        /// <param name="getAllPermutations">whether to get ordered permutations</param>
        /// <returns>Enumerated list of tuples</returns>
        public static IEnumerable<Tuple<T, T>> Combinations<T>
            (this IEnumerable<T> elist,
                bool includeMatches = false, bool getAllPermutations = false)
            where T : IComparable
        {
            var list = elist.ToList();
            return
                from x in list
                from y in list
                where y.CompareTo(x) > 0 ||
                      (getAllPermutations && y.CompareTo(x) < 0) ||
                      (includeMatches && y.CompareTo(x) == 0)
                select new Tuple<T, T>(y, x);
        }

        /// <summary>
        /// Generate cartesian product of tuples of one 
        ///  object from extended list and one object from other list
        /// </summary>
        /// <typeparam name="T">Type of objects in lists</typeparam>
        /// <param name="elist">List of Ts</param>
        /// <param name="otherList">the other list of Ts</param>
        /// <param name="includeMatches">whether to include tuples with identical members</param>
        /// <param name="getAllPermutations">whether to get ordered permutations</param>
        /// <returns>Enumerated list of tuples</returns>
        public static IEnumerable<Tuple<T, T>> Combinations<T>
            (this IEnumerable<T> elist, IEnumerable<T> otherList,
                bool includeMatches = false, bool getAllPermutations = false)
            where T : IComparable
        {
            var list = elist.ToList();
            return
                from x in list
                from y in otherList
                where y.CompareTo(x) > 0 ||
                      (getAllPermutations && y.CompareTo(x) < 0) ||
                      (includeMatches && y.CompareTo(x) == 0)
                select new Tuple<T, T>(y, x);
        }

        public static void RemoveAll<T>(this ICollection<T> coll, Func<T, bool> predicate)
        {
            for (var i = 0; i < coll.Count; i++)
            {
                var elm = coll.ElementAt(i);
                if (!predicate(elm)) continue;
                coll.Remove(elm);
                i--;
            }
        }

        #endregion Other
    }

    /// <summary>
    /// 
    /// </summary>
    public static class CoPTxScope
    {
        public static sysTx.TransactionScope Make(sysTx.DependentTransaction depTx)
        { return Make(sysTx.TransactionScopeOption.Required, 
            sysTx.IsolationLevel.ReadCommitted, 30d, depTx); }

        public static sysTx.TransactionScope Make(
            sysTx.TransactionScopeOption txScopOption = sysTx.TransactionScopeOption.Required,
            sysTx.IsolationLevel isoLevel = sysTx.IsolationLevel.ReadCommitted,
            double timeOutMinutes = 30d, sysTx.DependentTransaction depTx = null)
        {
            return depTx != null?
                new sysTx.TransactionScope(depTx):
                new sysTx.TransactionScope(txScopOption,
                    new sysTx.TransactionOptions { IsolationLevel = isoLevel, 
                        Timeout = TimeSpan.FromMinutes(timeOutMinutes) });
        }
    }

    public static class WinFormsExtensions
    {
        public static void ClearClickEvents(this IDisposable ctrl)
        {
            var f1 = typeof(Control).GetField("EventClick",
                BindingFlags.Static | BindingFlags.NonPublic);
            var obj = f1.GetValue(ctrl);
            var pi = ctrl.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var list = (EventHandlerList)pi.GetValue(ctrl, null);
            list.RemoveHandler(obj, list[obj]);
        }

        public static bool IsMouseOverControl(this Control control)
        {
            if (control == null) throw new ArgumentNullException("control");
            Contract.EndContractBlock();

            var pt = control.PointToClient(Control.MousePosition);
            return (pt.X >= 0 && 
                pt.Y >= 0 && 
                pt.X <= control.Width && 
                pt.Y <= control.Height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <summary>
    /// 
    /// </summary>
    public static class ControlExtensions
    {
        public static TreeNode RootNode(this TreeNode nod)
        {
            while (nod.Parent != null) nod = nod.Parent;
            return nod;
        }

        public static void DoubleBuffered(this Control ctrl, bool enable)
        {
            ctrl.GetType().GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ctrl, enable, null);
        }
    }

    public static class TextBoxExtensions
    {
        public static string NullValue(this TextBox tb)
        { return string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text; }
    }

    public static class IListExtensions
    {
        public static DataTable ConvertToDataTable<T>(this IEnumerable<T> list, string tableName = null)
        {
            var table = CreateDataTable<T>(tableName);
            var entityType = typeof(T);
            var properties = TypeDescriptor.GetProperties(entityType);

            foreach (var item in list)
            {
                var row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);
            }
            return table;
        }

        public static DataTable CreateDataTable<T>(string tableName = null)
        {
            var entityType = typeof(T);
            var table = new DataTable(tableName ?? entityType.Name);
            var properties = TypeDescriptor.GetProperties(entityType);

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(
                    prop.PropertyType) ?? prop.PropertyType);

            return table;
        }
    }

    public static class AssemblyExtensions
    {
        public static DateTime BuildTime(
            this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            const int maxByteArray = 0x0800;

            var buffer = new byte[maxByteArray];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, maxByteArray);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
    }

    public static class sdExtensions
    {
        public static string ParmValue(this sd mPs, string key)
        {
            return mPs.ContainsKey("oldStatIO") && mPs["oldStatIO"] is string ?
                (string)mPs["oldStatIO"] : null;
        }
    }
}