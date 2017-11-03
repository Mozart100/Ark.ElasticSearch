namespace Ark.StepRunner.ScenarioStepResult
{
    public class ScenarioStepJumpToStep : ScenarioStepReturnBase
    {
        private readonly int _indexToJumpToStep;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ScenarioStepJumpToStep(int indexToJumpToStep, params object[] parameters)
            : base(parameters)
        {
            _indexToJumpToStep = indexToJumpToStep;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public int IndexToJumpToStep => _indexToJumpToStep;
    }
}