using System;

namespace Ark.StepRunner
{
    using System.Collections.Generic;

    public class ScenarioResult
    {
        private readonly bool _isSuccessful;
        private readonly int _numberScenarioStepInvoked;
        private readonly Exception[] _exceptions;


        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ScenarioResult(bool isSuccessful, int numberScenarioStepInvoked, params Exception[] exceptions)
        {
            _isSuccessful = isSuccessful;
            _numberScenarioStepInvoked = numberScenarioStepInvoked;
            _exceptions = exceptions;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------


        public bool IsSuccessful => _isSuccessful;

        //--------------------------------------------------------------------------------------------------------------------------------------

        public int NumberScenarioStepInvoked => _numberScenarioStepInvoked;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Exception> Exceptions => _exceptions;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public static ScenarioResult operator +(ScenarioResult scenarioResult1, ScenarioResult scenarioResult2)
        {
            var exceptions = new List<Exception>();
            if (scenarioResult1.Exceptions != null)
            {
                exceptions.AddRange(scenarioResult1.Exceptions);
            }

            if (scenarioResult2.Exceptions != null)
            {
                exceptions.AddRange(scenarioResult2.Exceptions);
            }

            var result = new ScenarioResult(
                isSuccessful: scenarioResult1.IsSuccessful & scenarioResult2.IsSuccessful,
                numberScenarioStepInvoked: scenarioResult1.NumberScenarioStepInvoked + scenarioResult2.NumberScenarioStepInvoked,
                exceptions: exceptions.ToArray());


            return result;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        public static ScenarioResult operator | (ScenarioResult scenarioResult1, ScenarioResult scenarioResult2)
        {
            var exceptions = new List<Exception>();
            if (scenarioResult1.Exceptions != null)
            {
                exceptions.AddRange(scenarioResult1.Exceptions);
            }

            if (scenarioResult2.Exceptions != null)
            {
                exceptions.AddRange(scenarioResult2.Exceptions);
            }

            var result = new ScenarioResult(
                isSuccessful: scenarioResult1.IsSuccessful | scenarioResult2.IsSuccessful,
                numberScenarioStepInvoked:Math.Max(scenarioResult1.NumberScenarioStepInvoked, scenarioResult2.NumberScenarioStepInvoked),
                exceptions: exceptions.ToArray());


            return result;
        }
    }


    public class EmptyScenarioResult : ScenarioResult
    {
        public EmptyScenarioResult(bool isSuccessful = true ,int numberScenarioStepInvoked = 0, Exception exception = null)
            : base(isSuccessful: isSuccessful, numberScenarioStepInvoked: numberScenarioStepInvoked, exceptions: null)
        {

        }

    }


}