
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable, XmlRoot("Scenario")]
    public class Scenario: INotifyPropertyChanged
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        public void SetIsBinding(bool isBndng) { Catchment.SetIsBinding(isBndng); }
        #endregion Dirty Binding & state Properties

        #region fields
        private string nam;
        private DateTime cr8t;
        private Catchment ctch;
        private ExpectedResult expRslt;
        private StormResults actRslts;
        #endregion fields

        #region Scenario Properties
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name
        {
            get => nam;
            set => SetField(ref nam, value);
        }
        [XmlAttribute(AttributeName = "created", DataType = "date")]
        public DateTime Created
        {
            get => cr8t;
            set => SetField(ref cr8t, value);
        }
        public Catchment Catchment
        {
            get => ctch;
            set
            {
                if (value == ctch) return;
                ctch = value;
                NotifyPropertyChanged(nameof(Catchment));
            }
        }
        public ExpectedResult ExpectedResult
        {
            get => expRslt;
            set
            {
                if (value == expRslt) return;
                expRslt = value;
                NotifyPropertyChanged(nameof(ExpectedResult));
            }
        }
        public StormResults ActualResults
        {
            get => actRslts;
            set
            {
                if (value == actRslts) return;
                actRslts = value;
                NotifyPropertyChanged(nameof(ActualResults));
            }
        }
        #endregion Scenario Properties

        #region factory/ctors
        public static Scenario Empty => new Scenario
            {Catchment = PAC.Catchment.Empty, ExpectedResult = ExpectedResult.Empty};
        public static Scenario Make(string name, 
            Catchment catchment,
            DateTime? createdUtc = null,
            ExpectedResult expRslt = null)
        { return new Scenario { Name = name, 
            Catchment = catchment?? Catchment.Empty,
            Created = createdUtc?? DateTime.UtcNow,
            ExpectedResult = expRslt?? ExpectedResult.Empty
        }; }
        #endregion factory/ctors

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void SetField<T>(ref T field, T value,
            [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            NotifyPropertyChanged(propertyName);
        }
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation
    }
}
