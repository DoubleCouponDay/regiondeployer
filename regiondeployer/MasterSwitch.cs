using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;

namespace regiondeployer
{
    public class MasterSwitch
    {
        public const string MasterFarmerName = "chronictick";

        private IFunctionApp GetFirstMatch(Action<string> logMessage)
        {
            var functionApps = GetFunctionApps(logMessage);
            var output = functionApps.FirstOrDefault(currentApp => currentApp.Name.Contains(MasterFarmerName));
            return output;
        }

        public void ToggleMasterFarmer(bool shouldRun, Action<string> logMessage)
        {
            logMessage($"toggling master farmers operational state to {shouldRun}");

            var master = GetFirstMatch(logMessage);

            if (master == null)
            {
                throw new Exception($"no master farmer found. deploy that first. Its an azure function app whose name must contain {MasterFarmerName}");
            }
            else
            {
                if (shouldRun)
                {
                    master.Start();
                }
                else
                {
                    master.Stop();
                }
                logMessage("masterFarmer toggled.");
            }
        }
    }
}