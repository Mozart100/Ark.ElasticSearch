using System;

namespace Ark.StepRunner.Exceptions
{
    public class AScenarioStepTimeoutException : Exception
    {
        public AScenarioStepTimeoutException()
            : base("Scenario Step Timeout.")
        {

        }
    }
}