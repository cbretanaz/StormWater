namespace BES.SWMM.PAC
{
    public interface IManageDataFiles
    {
        Scenario LoadScenario();
        void SaveScenario(Scenario scen);
    }
}
