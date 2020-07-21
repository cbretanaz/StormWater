using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;

namespace BES.SWMM.PAC
{
    public class StormEventInterval : INotifyPropertyChanged
    {
        #region Properties
        public int itvl;

        [XmlAttribute(AttributeName = "timestep", DataType="int")]
        public int Timestep
        {
            get => itvl;
            set
            {
                if (value < 0 || value > 1443)
                    throw new PACInputDataException(typeof(short),
                        "Timestep Number must be between zero and 1,443.");
                SetField(ref itvl, value);
            }
        }

        #region RunOff
        public decimal cumRnOf;

        [XmlAttribute(AttributeName ="cumRunOff", DataType = "decimal")]
        public decimal RunOff // Cumulative Runoff at this time interval;
        {
            get => cumRnOf;
            set => SetField(ref cumRnOf, value); 
        }
        #endregion RunOff
        #endregion Properties

        #region ctor/Factories
        public static StormEventInterval Empty => new StormEventInterval();

        public static StormEventInterval Make(int ti)
        {return new StormEventInterval {Timestep = ti};}
        #endregion ctor/Factories

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void SetField<T>(ref T field, T value, 
            [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))return;
            field = value;
            NotifyPropertyChanged(propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged implementation

        public override string ToString()
        { return $"ts:{Timestep:000},   Runoff {RunOff:0.00 cubic feet}"; }
    }
}