using System;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Ark.StepRunner.Exceptions
{
    public class AScenarioMissingAttributeException: Exception
    {
        public AScenarioMissingAttributeException()
            :base("AScenario main attribute missing")
        {
            
        }
    }
}
