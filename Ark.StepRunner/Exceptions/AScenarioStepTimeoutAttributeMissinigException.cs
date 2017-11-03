namespace Ark.StepRunner.Exceptions
{
    using System;

    public class AScenarioStepTimeoutAttributeMissinigException : Exception
    {
        public AScenarioStepTimeoutAttributeMissinigException()
            :base("In Parallel Step should alway be Timeout Attribute")
        {
            
        }
    }
}