using System;
using System.Diagnostics;

namespace Ark.StepRunner.CustomAttribute
{
    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Method)]
    public class AScenarioStepTimeoutAttribute : Attribute
    {
        private readonly TimeSpan _timeout;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public AScenarioStepTimeoutAttribute(int seconds)
            : this(new TimeSpan(0, 0, seconds))
        {
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        private AScenarioStepTimeoutAttribute(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        public TimeSpan Timeout
        {
            get { return _timeout; }
        }
    }
}