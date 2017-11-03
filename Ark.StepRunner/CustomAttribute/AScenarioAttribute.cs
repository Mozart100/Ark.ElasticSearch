using System;
using System.Diagnostics;

namespace Ark.StepRunner.CustomAttribute
{
    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Class)]
    public class AScenarioAttribute : System.Attribute
    {
        private readonly string _description;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public AScenarioAttribute(string description)
        {
            _description = description;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
    }
}