using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CoP.Enterprise.Configuration
{
    [XmlRoot("DateListConfig")]
    public class DateListConfig
    {
        [XmlArrayItem(ElementName = "DateList")]
        public DateLists DateLists { get; set; }
        // -------------------------------------------
        public bool Contains(string listName)
        { return DateLists.Contains(listName); }
        public DateList this[string listName] => DateLists[listName];

        // ----------------------------------------------------
        public bool Contains(string listName, DateTime aDate)
        { return DateLists.Contains(listName) &&
            DateLists[listName].ListDates.Contains(aDate.Date);
        }
        public ListDate this[string listName, DateTime holidayDate] => DateLists.Contains(listName) ?
            DateLists[listName][holidayDate.Date] : null;
    }

    public class DateLists : List<DateList>
    {
        public bool Contains(string listName)
        {
            return this.Any(lst => lst.ListName.Equals(
                listName, StringComparison.InvariantCultureIgnoreCase));
        }

        public DateList this[string listName]
        {
            get
            { return this.FirstOrDefault(lst => lst.ListName.Equals(
                listName, StringComparison.InvariantCultureIgnoreCase)); }
        }
    }
    public class DateList
    {
        [XmlAttribute(DataType = "string",
            AttributeName = "listName")]
        public string ListName { get; set; }
        [XmlAttribute(DataType = "int",
            AttributeName = "priority")]
        public int Priority { get; set; }

        [XmlArrayItem(ElementName = "ListDate")]
        public ListDates ListDates { get; set; }

        public DateList() { }
        public DateList(string listName) { ListName = listName; }

        public ListDate this[DateTime listDate] => ListDates[listDate.Date];
        public override string ToString() { return ListName; }
    }
 
    public class ListDates : List<ListDate>
    {
        private static readonly StringComparison icic
            = StringComparison.InvariantCultureIgnoreCase;
        public bool Contains(DateTime listDate)
        { return this.Any(dt => dt.Date == listDate.Date); }
        public ListDate this[DateTime listDate]
        {
            get
            {
                return this.FirstOrDefault(
                    dt => dt.Date == listDate.Date);
            }
        }
        public bool Contains(string dateName, int year)
        { return this.Any(dt => dt.Name.Equals(dateName, icic) &&
            dt.Date.HasValue && dt.Date.Value.Year >= year); }
        public ListDate this[string dateName, int year]
        {
            get { return this.OrderBy(a=>a.Date.Value) 
                    .FirstOrDefault(dt => dt.Name.Equals(dateName, icic) &&
                    dt.Date.HasValue && dt.Date.Value.Year >= year); }
        }
    }
    public class ListDate
    {
        private string cfgDt;

        [XmlAttribute(DataType = "string",
          AttributeName = "date")]
        public string cfgListDate
        {
            get => cfgDt;
            set => cfgDt = value;
        }

        [XmlAttribute(DataType = "string",
            AttributeName = "name")]
        public string Name { get; set; }

        [XmlIgnore]
        public DateTime? Date
        {
            set => cfgDt = value?.ToString("d MMM yyyy");
            get => !string.IsNullOrEmpty(cfgDt) &&
                DateTime.TryParse(cfgDt, out DateTime retVal) ?
                    retVal : (DateTime?)null;
        }

        public ListDate() { }
        public ListDate(DateTime theDate, string name)
        { Date = theDate.Date; Name = name; }
        public override string ToString() { return $"{Name}: {Date:d MMM yyyy}"; }
    }
}