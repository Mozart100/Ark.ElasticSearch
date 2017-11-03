using System;
using System.Diagnostics;

namespace Ark.StepRunner.CustomAttribute
{
    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Method)]
    public class ABusinessStepScenarioAttribute : System.Attribute
    {
        private readonly int _index;
        private readonly string _description;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ABusinessStepScenarioAttribute(int index, string description)
        {
            _index = index;
            _description = description;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public int Index => _index;

        //--------------------------------------------------------------------------------------------------------------------------------------

        public string Description => _description;
    }



}