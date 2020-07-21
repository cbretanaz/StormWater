using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CoP.Enterprise
{
    /// <summary>
    /// Provides a generic collection that supports data binding and additionally supports sorting.
    /// See http://msdn.microsoft.com/en-us/library/ms993236.aspx
    /// If the elements are IComparable it uses that; otherwise compares the ToString()
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class SortableBindingList<T> : BindingList<T>, IBindingListView
    {
        private static readonly FilterOperator EQ = FilterOperator.EqualTo;
        private static readonly FilterOperator LT = FilterOperator.LessThan;
        private static readonly FilterOperator GT = FilterOperator.GreaterThan;
        private static readonly FilterOperator NA = FilterOperator.None;
        private readonly List<T> originalList = new List<T>();
        public List<T> OriginalList => originalList;

        private readonly Dictionary<Type, PropertyComparer<T>> comparers;
        private bool isSorted;
        private ListSortDirection listSortDirection;
        private PropertyDescriptor propertyDescriptor;

        /// <summary>
        /// constructor to be used to instantiate an empty sortable binding list
        /// </summary>
        public SortableBindingList(): base(new List<T>())
        { comparers = new Dictionary<Type, PropertyComparer<T>>(); }

        /// <summary>
        /// constructor to be used to instantiate the sortable binding list
        /// which instantiates binding list with specified collection of items
        /// </summary>
        /// <param name="enumeration">Enumerable list of items of type T 
        /// to be inserted into the binding list</param>
        public SortableBindingList(IEnumerable<T> enumeration)
            : base(new List<T>(enumeration))
        { comparers = new Dictionary<Type, PropertyComparer<T>>(); }

        /// <summary>
        ///  Indicates that the binding list supports Sorting operations
        ///  on the properties of the contained items
        /// </summary>
        protected override bool SupportsSortingCore => true;

        /// <summary>
        /// Indicates that the list is currently sorted by one of the core items properties
        /// </summary>
        protected override bool IsSortedCore => isSorted;

        /// <summary>
        /// 
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;

        /// <summary>
        /// 
        /// </summary>
        protected override ListSortDirection SortDirectionCore => listSortDirection;

        /// <summary>
        /// 
        /// </summary>
        protected override bool SupportsSearchingCore => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="direction"></param>
        protected override void ApplySortCore(PropertyDescriptor property, 
            ListSortDirection direction)
        {
            var itemsList = Items as List<T>;

            var propertyType = property.PropertyType;
            PropertyComparer<T> comparer;
            if (!comparers.TryGetValue(propertyType, out comparer))
            {
                comparer = new PropertyComparer<T>(property, direction);
                comparers.Add(propertyType, comparer);
            }

            comparer.SetPropertyAndDirection(property, direction);
            itemsList.Sort(comparer);

            propertyDescriptor = property;
            listSortDirection = direction;
            isSorted = true;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RemoveSortCore()
        {
            isSorted = false;
            propertyDescriptor = base.SortPropertyCore;
            listSortDirection = base.SortDirectionCore;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override int FindCore(PropertyDescriptor property, object key)
        {
            var count = Count;
            for (var i = 0; i < count; ++i)
            {
                var element = this[i];
                if (property.GetValue(element).Equals(key)) return i;
            }
            return -1;
        }

        #region IBindingListView
        public void ApplySort(ListSortDescriptionCollection sorts)
        { throw new NotImplementedException(); }

        public void RemoveFilter()
        { if (Filter != null) Filter = null; }

        private string filter;

        public string Filter
        {
            get { return filter; }
            set
            {
                if (filter == value) return;
                if (!string.IsNullOrEmpty(value) &&
                    !Regex.IsMatch(value,
                    BuildRegExForFilterFormat(), RegexOptions.Singleline))
                    throw new ArgumentException("Filter is not in " +
                                                "the format: propName[<>=]'value'.");
                //Turn off list-changed events. 
                RaiseListChangedEvents = false;

                // If the value is null or empty, reset list. 
                var count = 0;
                if (string.IsNullOrWhiteSpace(value))
                    ResetList();
                else
                {
                    ClearItems();
                    var matches = value.Split(new[] {" AND "},
                        StringSplitOptions.RemoveEmptyEntries);
                    while (count < matches.Length)
                    {
                        var filterPart = matches[count];
                        // Parse and apply the filter. 
                        var filterInfo = ParseFilter(filterPart);
                        ApplyFilter(filterInfo, count == 0);
                        count++;
                    }
                }
                // Set the filter value and turn on list changed events. 
                filter = value;
                RaiseListChangedEvents = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }
        public ListSortDescriptionCollection SortDescriptions { get { return null; } }
        public bool SupportsAdvancedSorting { get { return false; } }
        public bool SupportsFiltering { get { return true; } }
        #endregion IBindingListView

        public static string BuildRegExForFilterFormat()
        {
            StringBuilder regex = new StringBuilder();

            // Look for optional literal brackets, 
            // followed by word characters or space. 
            regex.Append(@"\[?[\w\s]+\]?\s?");

            // Add the operators: > < or =. 
            regex.Append(@"[><=]");

            //Add optional space followed by optional quote and 
            // any character followed by the optional quote. 
            regex.Append(@"\s?'?.+'?");

            return regex.ToString();
        }
        private void ResetList()
        {
            ClearItems();
            foreach (var t in originalList) Items.Add(t);
            if (IsSortedCore)
                ApplySortCore(SortPropertyCore, SortDirectionCore);
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            // If the list is reset, check for a filter. If a filter
            // is applied, don't allow items to be added to the list.
            if (e.ListChangedType == ListChangedType.Reset)
                AllowNew = string.IsNullOrEmpty(Filter);
            // Add the new item to the original list.
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                OriginalList.Add(this[e.NewIndex]);
                if (!string.IsNullOrEmpty(Filter))
                {
                    var cachedFilter = Filter;
                    Filter = string.Empty;
                    Filter = cachedFilter;
                }
            }
            // Remove the new item from the original list.
            if (e.ListChangedType == ListChangedType.ItemDeleted)
                OriginalList.RemoveAt(e.NewIndex);

            base.OnListChanged(e);
        }

        internal SingleFilterInfo ParseFilter(string filterPart)
        {
            filterPart = filterPart.Replace("==", "=");
            var filterInfo = new SingleFilterInfo 
                {OperatorValue = DetermineFilterOperator(filterPart)};

            var filterStringParts = filterPart.Split((char)filterInfo.OperatorValue);

            filterInfo.PropName =
                filterStringParts[0].Replace("[", "").
                Replace("]", "").Replace(" AND ", "").Trim();

            // Get the property descriptor for the filter property name.
            var filterPropDesc = TypeDescriptor.GetProperties(typeof(T))[filterInfo.PropName];

            // Convert the filter compare value to the property type.
            if (filterPropDesc == null)
                throw new InvalidOperationException("Specified property to " +
                    "filter " + filterInfo.PropName +
                    " on does not exist on type: " + typeof(T).Name);

            filterInfo.PropDesc = filterPropDesc;

            var comparePartNoQuotes = StripOffQuotes(filterStringParts[1]);
            try
            {
                var converter = TypeDescriptor.GetConverter(filterPropDesc.PropertyType);
                filterInfo.CompareValue =
                    converter.ConvertFromString(comparePartNoQuotes);
            }
            catch (NotSupportedException)
            {
                throw new InvalidOperationException("Specified filter" +
                    "value " + comparePartNoQuotes + " can not be converted" +
                    "from string. Implement a type converter for " +
                    filterPropDesc.PropertyType);
            }
            return filterInfo;
        }

        internal FilterOperator DetermineFilterOperator(string filterPart)
        {
            // Determine the filter's operator.
            return Regex.IsMatch(filterPart, "[^>^<]=")? EQ:
                Regex.IsMatch(filterPart, "<[^>^=]")?    LT:
                Regex.IsMatch(filterPart, "[^<]>[^=]")?  GT: NA;
        }

        internal static string StripOffQuotes(string filterPart)
        {
            // Strip off quotes in compare value if they are present.
            if (Regex.IsMatch(filterPart, "'.+'"))
            {
                int quote = filterPart.IndexOf('\'');
                filterPart = filterPart.Remove(quote, 1);
                quote = filterPart.LastIndexOf('\'');
                filterPart = filterPart.Remove(quote, 1);
                filterPart = filterPart.Trim();
            }
            return filterPart;
        }
        internal void ApplyFilter(SingleFilterInfo filterParts, bool addMatches = true)
        {
            // Check to see if the property type we are filtering by implements
            // the IComparable interface.
            var interfaceType =
                TypeDescriptor.GetProperties(typeof(T))[filterParts.PropName]
                .PropertyType.GetInterface("IComparable");

            if (interfaceType == null)
                throw new InvalidOperationException(
                    "Filtered property must implement IComparable.");

            var removes = new List<T>();
            // Check each value and add to the results list.
            if(addMatches)
                foreach (var item in originalList.Where(i=>!Contains(i)))
                {
                    if (filterParts.PropDesc.GetValue(item) == null || Contains(item)) continue;
                    var compareValue = filterParts.PropDesc.GetValue(item) as IComparable;
                    var result = compareValue.CompareTo(filterParts.CompareValue);

                    if (filterParts.OperatorValue == EQ && result == 0) Add(item);
                    if (filterParts.OperatorValue == GT && result > 0)  Add(item);
                    if (filterParts.OperatorValue == LT && result < 0)  Add(item);
                }
            else // removing items that don't match
                foreach (var item in this)
                {
                    if (filterParts.PropDesc.GetValue(item) == null || !Contains(item)) continue;
                    var compareValue = filterParts.PropDesc.GetValue(item) as IComparable;
                    var result = compareValue.CompareTo(filterParts.CompareValue);
                    if (filterParts.OperatorValue == EQ && result != 0) removes.Add(item);
                    if (filterParts.OperatorValue == GT && result <= 0) removes.Add(item);
                    if (filterParts.OperatorValue == LT && result >= 0) removes.Add(item);
                }
            foreach (var item in removes) Remove(item);
        }
    }

    public struct SingleFilterInfo
    {
        internal string PropName;
        internal PropertyDescriptor PropDesc;
        internal Object CompareValue;
        internal FilterOperator OperatorValue;
    }
    public enum FilterOperator
    {
        EqualTo = '=',
        LessThan = '<',
        GreaterThan = '>',
        None = ' '
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly IComparer comparer;
        private PropertyDescriptor propertyDescriptor;
        private int reverse;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="direction"></param>
        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            propertyDescriptor = property;
            var comparerForPropertyType = typeof (Comparer<>).MakeGenericType(property.PropertyType);
            comparer = (IComparer) comparerForPropertyType.InvokeMember("Default",
                BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public,
                null, null, null);
            SetListSortDirection(direction);
        }

        #region IComparer<T> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            return reverse*comparer.Compare(propertyDescriptor.GetValue(x),
                propertyDescriptor.GetValue(y));
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="direction"></param>
        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            SetPropertyDescriptor(descriptor);
            SetListSortDirection(direction);
        }

        private void SetPropertyDescriptor(PropertyDescriptor descriptor)
        { propertyDescriptor = descriptor; }

        private void SetListSortDirection(ListSortDirection direction)
        { reverse = direction == ListSortDirection.Ascending ? 1 : -1; }
    }
}