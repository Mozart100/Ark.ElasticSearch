namespace Ark.StepRunner.Exceptions
{
    using System;

    public class AScenarioConstructorParameterNullException : Exception
    {
        private readonly string _parameterName;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public AScenarioConstructorParameterNullException(string parameterName)
            : base("In constructor a parameter was null.")
        {
            _parameterName = parameterName;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        public string ParameterName => _parameterName;
    }
}