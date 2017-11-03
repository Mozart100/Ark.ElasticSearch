namespace Ark.StepRunner.CustomAttribute
{
    using System;
    using System.Diagnostics;

    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Method)]
    public class AStepSetupScenarioAttribute : ABusinessStepScenarioAttribute
    {
        public AStepSetupScenarioAttribute(int index, string description)
            : base(index: index, description: description)
        {
        }
    }
}