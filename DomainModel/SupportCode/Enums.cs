using System;
using System.Xml.Serialization;

namespace BES.SWMM.PAC
{
    [Flags]
    public enum PRStatus
    {
        [XmlEnum(Name = "Null")] Null = 0x0,
        [XmlEnum(Name = "Pend")] Pending = 0x1,
        [XmlEnum(Name = "Pass")] Pass = 0x2,
        [XmlEnum(Name = "JustReq")] JustificationRequired = 0x2,
        [XmlEnum(Name = "Fail")] Fail = 0x8
    }

    [Flags]
    public enum InfiltrationTestProcedure
    {
        [XmlEnum(Name = "Null")] Null = 0,
        [XmlEnum(Name = "OpenPit")] OpenPit = 1,
        [XmlEnum(Name = "Encased")] Encased = 2,
        [XmlEnum(Name = "DoubleRing")] DoubleRing = 4,
        [XmlEnum(Name = "N/A")] NA = 8
    }

    [Flags]
    public enum HierarchyLevel
    {
        [XmlEnum(Name = "Null")] Null = 0,
        [XmlEnum(Name = "1")] One = 0x01,
        [XmlEnum(Name = "1")] OnsiteWithSurfaceInfiltration = 0x01,
        [XmlEnum(Name = "2a")] TwoA = 0x02,
        [XmlEnum(Name = "2a")] DischargeToRiverSlough = 0x02,
        [XmlEnum(Name = "2b")] TwoB = 0x04,
        [XmlEnum(Name = "2b")] OverlandDischarge = 0x04,
        [XmlEnum(Name = "2c")] TwoC = 0x08,
        [XmlEnum(Name = "2c")] StormSewerDrainageDischarge = 0x08,
        [XmlEnum(Name = "3")] Three = 0x10,
        [XmlEnum(Name = "3")] CombinedSewerDischarge = 0x10,
        ReqWQ = One| TwoA | TwoB | TwoC,     
        Req2Yr = TwoB | TwoC, 
        Req5Yr = TwoC,
        ReqH2Yr = One | TwoA | TwoB | TwoC | Three,
        Req10Yr = One | TwoB | TwoC,
        Req25Yr = TwoB | Three,
        Serialize = One | TwoB | TwoC
    }

    [Flags]
    public enum FacilityCategory
    {
        [XmlEnum(Name = "Null")] Null = 0x00,
        [XmlEnum(Name = "Sloped")] SlopedFacility = 0x01,
        [XmlEnum(Name = "FlatPlanter")] FlatPlanter = 0x02,
        [XmlEnum(Name = "RectBasin")] RectBasin = 0x04,
        [XmlEnum(Name = "AmoebaBasin")] AmoebaBasin = 0x08,
        [XmlEnum(Name = "UserDefined")] UserDefinedBasin = 0x10,
        Basin = RectBasin | AmoebaBasin | UserDefinedBasin,
        Serialize = SlopedFacility | FlatPlanter | RectBasin |
                     AmoebaBasin | UserDefinedBasin
    }

    [Flags]
    public enum Shape
    {
        [XmlEnum(Name = "Null")] Null = 0x0,
        [XmlEnum(Name = "Rectangular")] Rectangular = 0x1,
        [XmlEnum(Name = "Amoeba")] Amoeba = 0x2,
        [XmlEnum(Name = "UserDefined")] UserDefined = 0x4,
        Serialize = Rectangular | Amoeba  | UserDefined
    }

    [Flags]
    public enum Configuration
    {
        [XmlEnum(Name = "Null")] Null = 0x00,
        [XmlEnum(Name = "A")] ConfigA = 0x01,
        [XmlEnum(Name = "A")] Infiltration = 0x01,
        [XmlEnum(Name = "B")] ConfigB = 0x02,
        [XmlEnum(Name = "B")] InfWithRockStorage = 0x02,
        [XmlEnum(Name = "C")] ConfigC = 0x04,
        [XmlEnum(Name = "C")] InfWRockAndUnderdrain = 0x04,
        [XmlEnum(Name = "D")] ConfigD = 0x08,
        [XmlEnum(Name = "D")] LinedWRockAndUnderdrain = 0x08,
        [XmlEnum(Name = "E")] ConfigE = 0x10,
        [XmlEnum(Name = "E")] InfWBypass2RockStorage = 0x10,
        [XmlEnum(Name = "F")] ConfigF = 0x20,
        [XmlEnum(Name = "F")] InfWBypass2RockAndUnderdrain = 0x20,
        ConfigCD = ConfigC | ConfigD, NeedsOrf = ConfigCD,
        ConfigAB = ConfigA | ConfigB, 
        ConfigABE = ConfigAB | ConfigE,
        ConfigEF = ConfigE | ConfigF ,
        ConfigCDF = ConfigC | ConfigD | ConfigF,
        ConfigBCEF = ConfigB| ConfigC|ConfigEF,
        ConfigABEF = ConfigABE | ConfigF,
        Serialize = ConfigAB | ConfigCD | ConfigEF
    }

    [Flags]
    public enum DesignStorm
    {
        Null = 0x00,
        [XmlEnum(Name = "WQ")] WQ = 0x01,
        [XmlEnum(Name = "H2Yr")] HalfTwoYear = 0x02,
        [XmlEnum(Name = "2Yr")] TwoYear = 0x04,
        [XmlEnum(Name = "5Yr")] FivYear = 0x08,
        [XmlEnum(Name = "10Yr")] TenYear = 0x10,
        [XmlEnum(Name = "25Yr")] TwntyFiv = 0x20,
        All = WQ|HalfTwoYear|TwoYear|FivYear|TenYear| TwntyFiv,
        H1 = WQ|TenYear, H2A = WQ,
        C2S = WQ | TwntyFiv,
        H2B = WQ | HalfTwoYear | FivYear | TenYear | TwntyFiv, 
        H2C = WQ | TwoYear | FivYear | TenYear,
        H3 = TwntyFiv
    }

    public static class DesignStormExt
    {
        public static string Name(this DesignStorm ds)
        {
            {
                return
                    ds == DesignStorm.WQ ? "Water Quality" :
                    ds == DesignStorm.TwoYear ? "Two-Year" :
                    ds == DesignStorm.HalfTwoYear ? "Half Two-Year" :
                    ds == DesignStorm.FivYear ? "Five-Year" :
                    ds == DesignStorm.TenYear ? "10-Year" :
                    ds == DesignStorm.TwntyFiv ? "25-Year" : null;
            }
        }
    }

    [Flags]
    public enum PrePost
    {
        [XmlEnum(Name = "WQ")] Null = 0x0,
        [XmlEnum(Name = "preDev")] Pre = 0x1,
        [XmlEnum(Name = "postDev")] Post = 0x2
    }

    [Flags]
    public enum AGP //Above Ground Controls
    {
        None = 0x00,
        BA = 0x001,
        ABW = 0x002,

        // ---------
        OFH = 0x004,
        OFA = 0x008,

        // ---------
        OFEH = 0x010,
        OFEA = 0x020,

        // ---------
        SS = 0x40,
        FD = 0x80,
        BP = 0x100,
        All = 0x1FF
    }

    [Flags]
    public enum BGP //Below Ground Controls
    {
        None = 0x00,
        IP = 0x01,
        RSD = 0x02,
        UDH = 0x04,
        RP = 0x08,
        RAW = 0x10,
        ORF = 0x20,
        C2S = 0x40,
        All = 0x7F
    }

    public enum DataControl
    {
        NA = 0,
        // -------------------------
        BSD = 1, IFR = 2, IMPA = 3, 
        PreCN = 4, PstCN = 5,
        PreTC = 6, PstTC = 7, CFG = 8,
        // -------------------------
        BA = 10, ABW=11, OFH = 12, OFA = 13, 
        OFEH = 14 , OFEA = 15,
        SS = 16, FD = 17, BP = 18, 
        // ---------------------
        IP = 20, RSD = 21, UDH = 22, RP = 23, 
        RBA = 24, ORD = 25, ORR = 26, C2S = 27

    }


    [Flags]
    public enum OrificeReason
    {
        [XmlEnum(Name = "NA")] Null = 0x0,
        [XmlEnum(Name = "None")] None = 0x1,
        [XmlEnum(Name = "WQ")] WQOnly = 0x2,
        [XmlEnum(Name = "FloCtrl")] MeetsFlowCtrl = 0x4,
        [XmlEnum(Name = "TooSmall")] CatchTooSmall = 0x8,
        Serialize = None | WQOnly | MeetsFlowCtrl | CatchTooSmall
    }

    [Flags]
    public enum FailType
    {
        Null = 0x000,
        WQOverflow = 0x001,
        ReqOverflow = 0x002,
        TotOverflow = 0x004,
        XcdsPreDev = 0x008,
        CatchTooSmallOverflow = 0x010,
        ImperArea2Large = 0x020,
        PeakDepth2High = 0x040,
        PeakOutflow = 0x080,
        PeakSurfaceOverflow = 0x100
    }
    public enum RAMSide { Neither, One, Both }
}