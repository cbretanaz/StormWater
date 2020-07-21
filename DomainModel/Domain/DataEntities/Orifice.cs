using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BES.SWMM.PAC.Annotations;
using Newtonsoft.Json;

namespace BES.SWMM.PAC
{
    [Serializable]
    public class Orifice : Constants, INotifyPropertyChanged
    {
        #region Dirty Binding & state Properties
        [XmlIgnore]
        public bool IsBinding { get; set; }
        [XmlIgnore]
        public bool IsDirty { get; set; }
        #endregion Dirty Binding & state Properties

        #region Properties   

        [XmlIgnore] private bool hasOrf;
        [XmlAttribute(AttributeName = "hasOrifice", DataType = "boolean")]
        public bool xmlHasOrf
        {
            get => hasOrf;
            set
            {
                if (value.Equals(hasOrf)) return;
                hasOrf = value;
                NotifyPropertyChanged(nameof(HasOrifice));
            }
        }
        [XmlIgnore]
        public bool HasOrifice
        {
            get => diam.HasValue || Reason == OrificeReason.Null;
            set => SetField(ref hasOrf, value);
        }
        public bool ShouldSerializeHasOrifice() { return true; }
        // -----------------------------------------------------------

        [XmlIgnore] public decimal? diam;
        [XmlAttribute(AttributeName = "diameter", DataType = "decimal")]
        public decimal xmlDiameter
        {
            get => diam.Value;
            set
            {
                if (value.Equals(diam)) return;
                diam = value;
                NotifyPropertyChanged(nameof(Diameter));
            }
        }
        [XmlIgnore]
        public decimal? Diameter
        {
            get => diam;
            set => SetField(ref diam, value);
        }
        public bool ShouldSerializexmlDiameter() { return HasOrifice && diam.HasValue; }
        // -----------------------------------------------------------

        [XmlIgnore] public OrificeReason rsn;
        [XmlAttribute(AttributeName = "reason", DataType = "string")]
        public string xmlReason
        {
            get
            {
                switch (rsn)
                {
                    case OrificeReason.WQOnly: return "WQOnly";
                    case OrificeReason.CatchTooSmall: return "TooSmall";
                    case OrificeReason.MeetsFlowCtrl: return "Okay";
                    case OrificeReason.None: return "None";
                    default: return null;
                }
            }
            set
            {
                switch (value)
                {
                    case "WQOnly":
                        Reason = OrificeReason.WQOnly;
                        break;
                    case "TooSmall":
                        Reason = OrificeReason.CatchTooSmall;
                        break;
                    case "Okay":
                        Reason = OrificeReason.MeetsFlowCtrl;
                        break;
                    case "None":
                        Reason = OrificeReason.None;
                        break;
                    default:
                        Reason = OrificeReason.Null;
                        break;
                }
            }
        }

        [XmlIgnore]
        public OrificeReason Reason
        {
            get => rsn;
            set => SetField(ref rsn, value);
        }
        public bool ShouldSerializexmlReason() { return !HasOrifice && OrificeReason.Serialize.HasFlag(Reason); }

        #endregion Properties     

        #region ctor/factories
        public static global::BES.SWMM.PAC.Orifice Empty => new global::BES.SWMM.PAC.Orifice { Diameter = null, Reason = orNull };
        public static global::BES.SWMM.PAC.Orifice WQ => new global::BES.SWMM.PAC.Orifice { Diameter = null, Reason = orWQ };
        public static global::BES.SWMM.PAC.Orifice TS => new global::BES.SWMM.PAC.Orifice { Diameter = null, Reason = orTS };
        public static global::BES.SWMM.PAC.Orifice OK => new global::BES.SWMM.PAC.Orifice { Diameter = null, Reason = orOK };
        public static global::BES.SWMM.PAC.Orifice Default => new global::BES.SWMM.PAC.Orifice {HasOrifice = true, Diameter = 1m, Reason = orNone };
        public static global::BES.SWMM.PAC.Orifice Make(decimal diameter)
        { return new global::BES.SWMM.PAC.Orifice { Diameter = diameter, Reason = orNull }; }
        public static global::BES.SWMM.PAC.Orifice Make(OrificeReason resn)
        { return new global::BES.SWMM.PAC.Orifice { Diameter = null, Reason = resn }; }
        #endregion ctor/factories

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
        protected virtual void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
        public void SetIsBinding(bool isBndng) { IsBinding = isBndng; }
        #endregion INotifyPropertyChanged implementation
    }
}