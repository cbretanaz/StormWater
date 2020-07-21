namespace BES.SWMM.PAC
{
    public interface ICalculateResults
    {
        StormResults CalculateResults(Catchment ctch, bool getTimestepDetails = true);
    }

    public interface IValidateScenarioData
    {
        void ValidateScenarioData(Scenario scen);
    }
}