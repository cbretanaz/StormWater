namespace BES.SWMM.PAC
{
    public abstract class Constants
    {

        #region Design Storm consts
        protected const DesignStorm dswq = DesignStorm.WQ;
        protected const DesignStorm dsH2 = DesignStorm.HalfTwoYear;
        protected const DesignStorm ds2 = DesignStorm.TwoYear;
        protected const DesignStorm ds5 = DesignStorm.FivYear;
        protected const DesignStorm ds10 = DesignStorm.TenYear;
        protected const DesignStorm ds25 = DesignStorm.TwntyFiv;
        #endregion Design Storm consts

        #region static FacilityCategory consts
        protected const FacilityCategory slpdFclty = FacilityCategory.SlopedFacility;
        protected const FacilityCategory fltPntr = FacilityCategory.FlatPlanter;
        protected const FacilityCategory rectBasin = FacilityCategory.RectBasin;
        protected const FacilityCategory amoebaBasin = FacilityCategory.AmoebaBasin;
        protected const FacilityCategory usrBasin = FacilityCategory.UserDefinedBasin;
        #endregion static FacilityCategory consts

        #region Configuration Consts
        protected const Configuration cfgA = Configuration.ConfigA;
        protected const Configuration cfgB = Configuration.ConfigB;
        protected const Configuration cfgC = Configuration.ConfigC;
        protected const Configuration cfgD = Configuration.ConfigD;
        protected const Configuration cfgE = Configuration.ConfigE;
        protected const Configuration cfgF = Configuration.ConfigF;
        protected const Configuration cfgAB = Configuration.ConfigAB;
        protected const Configuration cfgABE = Configuration.ConfigABE;
        protected const Configuration cfgCDF = Configuration.ConfigCDF;
        #endregion Configuration Consts

        #region Heirarchy Consts
        protected const HierarchyLevel H1 = HierarchyLevel.One;
        protected const HierarchyLevel H2A = HierarchyLevel.TwoA;
        protected const HierarchyLevel H2B = HierarchyLevel.TwoB;
        protected const HierarchyLevel H2C = HierarchyLevel.TwoC;
        protected const HierarchyLevel H3 = HierarchyLevel.Three;
        #endregion Heirarchy Consts

        #region OrificeReason Consts
        protected const OrificeReason orNull = OrificeReason.Null;
        protected const OrificeReason orNone = OrificeReason.None;
        protected const OrificeReason orWQ = OrificeReason.WQOnly;
        protected const OrificeReason orTS = OrificeReason.CatchTooSmall;
        protected const OrificeReason orOK = OrificeReason.MeetsFlowCtrl;
        #endregion OrificeReason Consts

        #region PRStatus Consts
        protected const PRStatus stNull = PRStatus.Null;
        protected const PRStatus PEND = PRStatus.Pending;
        protected const PRStatus PASS = PRStatus.Pass;
        protected const PRStatus JUST = PRStatus.JustificationRequired;
        protected const PRStatus FAIL = PRStatus.Fail;
        #endregion PRStatus Consts

    }
}