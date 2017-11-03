using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.StepRunner.ScenarioStepResult
{
    public abstract class ScenarioStepReturnBase
    {
        private readonly object[] _parameters;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        protected ScenarioStepReturnBase(params object[] parameters)
        {
            _parameters = parameters;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------    

        public object[] Parameters
        {
            get { return _parameters; }
        }
    }
}
