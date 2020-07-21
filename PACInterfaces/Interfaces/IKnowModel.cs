using System;
namespace BES.SWMM.PAC
{
    public interface IKnowModel
    {
        Scenario GetScenario();
        Scenario LoadScenario();
        EventHandler LoadScenarioEvent(Scenario scen);
    }
}