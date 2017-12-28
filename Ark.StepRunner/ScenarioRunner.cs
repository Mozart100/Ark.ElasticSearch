using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ark.StepRunner.CustomAttribute;
using Ark.StepRunner.Exceptions;
using Ark.StepRunner.ScenarioStepResult;
using Autofac;
using Serilog;
using System.Diagnostics;

namespace Ark.StepRunner
{
    [DebuggerStepThrough]
    public class ScenarioRunner
    {
        private class ScenarioStepReturnVoid : ScenarioStepReturnBase
        {
            public ScenarioStepReturnVoid()
                : base(parameters: null)
            {

            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        private class StepAndAttributeBundle
        {
            private readonly MethodInfo _methodInfo;
            private readonly ABusinessStepScenarioAttribute _businessStepScenario;
            private readonly AExceptionIgnoreAttribute _exceptionIgnoreAttribute;
            private readonly TimeSpan _timeout;
            private readonly AScenarioStepTimeoutAttribute _scenarioStepTimeoutAttribute;
            private readonly AScenarioStepParallelAttribute _scenarioStepParallelAttribute;

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            public StepAndAttributeBundle(
                MethodInfo methodInfo,
                ABusinessStepScenarioAttribute businessStepScenario,
                AExceptionIgnoreAttribute exceptionIgnoreAttribute,
                AScenarioStepTimeoutAttribute scenarioStepTimeoutAttribute,
                AScenarioStepParallelAttribute scenarioStepParallelAttribute)
            {
                _methodInfo = methodInfo;
                _businessStepScenario = businessStepScenario;
                _exceptionIgnoreAttribute = exceptionIgnoreAttribute;
                _scenarioStepTimeoutAttribute = scenarioStepTimeoutAttribute;
                _scenarioStepParallelAttribute = scenarioStepParallelAttribute;

                _timeout = ExtractTimeout(_methodInfo);

                Initialize();
            }

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            public MethodInfo MethodInfo => _methodInfo;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public ABusinessStepScenarioAttribute BusinessStepScenario => _businessStepScenario;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public AExceptionIgnoreAttribute ExceptionIgnoreAttribute => _exceptionIgnoreAttribute;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public AScenarioStepParallelAttribute ScenarioStepParallelAttribute => _scenarioStepParallelAttribute;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public TimeSpan Timeout => _timeout;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public AScenarioStepTimeoutAttribute ScenarioStepTimeoutAttribute => _scenarioStepTimeoutAttribute;

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            private void Initialize()
            {
                if (_scenarioStepParallelAttribute != null)
                {
                    if (_scenarioStepTimeoutAttribute == null)
                    {
                        throw new AScenarioStepTimeoutAttributeMissinigException();
                    }
                }
            }

            //--------------------------------------------------------------------------------------------------------------------------------------

            private TimeSpan ExtractTimeout(MethodInfo method)
            {
                var attribute = method.GetCustomAttribute(typeof(AScenarioStepTimeoutAttribute)) as AScenarioStepTimeoutAttribute;

                if (attribute != null)
                {
                    return attribute.Timeout;
                }

                return TimeSpan.Zero;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        private class ScenarioStepReturnResultBundle
        {
            private readonly ScenarioStepReturnBase _scenarioSStepReturn;
            private readonly ScenarioResult _scenarioResult;

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            public ScenarioStepReturnResultBundle(ScenarioStepReturnBase scenarioSStepReturn, ScenarioResult scenarioResult)
            {
                _scenarioSStepReturn = scenarioSStepReturn;
                _scenarioResult = scenarioResult;
            }

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------
            public ScenarioStepReturnBase ScenarioSStepReturn => _scenarioSStepReturn;

            //--------------------------------------------------------------------------------------------------------------------------------------

            public ScenarioResult ScenarioResult => _scenarioResult;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        //private readonly IStepPublisherLogger _stepPublisherLogger;
        private ILogger _logger;
        private readonly IContainer _containerBuilder;

        //--------------------------------------------------------------------------------------------------------------------------------------

        private readonly Dictionary<int, StepAndAttributeBundle> _scenarioSteps;
        private readonly Dictionary<int, StepAndAttributeBundle> _scenarioSetups;
        private readonly Dictionary<int, StepAndAttributeBundle> _scenarioCleanups;

        //--------------------------------------------------------------------------------------------------------------------------------------

        private readonly MethodInvoker _methodInvoker;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ScenarioRunner(ILogger logger, IContainer containerBuilder)
        {
            _containerBuilder = containerBuilder;
            _logger = logger;
            _methodInvoker = new MethodInvoker();

            _scenarioSteps = new Dictionary<int, StepAndAttributeBundle>();
            _scenarioSetups = new Dictionary<int, StepAndAttributeBundle>();
            _scenarioCleanups = new Dictionary<int, StepAndAttributeBundle>();
        }


        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------


        public ScenarioResult RunScenarioWithParameters<TScenario>(params object[] parameters)
        {

            var autofacParams = new List<Autofac.NamedParameter>();
            //var consPa5rams = typeof(TScenario).GetConstructors(BindingFlags.Public)[0].GetParameters();
            var consPa5rams = typeof(TScenario)
           .GetConstructors()
           .FirstOrDefault(c => c.GetParameters().Length > 0);

            int index = 0;
            foreach (var item in consPa5rams.GetParameters())
            {
                if (item.GetType().IsClass == true)
                {

                    object tmp;
                    if (_containerBuilder.TryResolve(item.ParameterType, out tmp) == false)
                    {
                        autofacParams.Add(new NamedParameter(item.Name, parameters[index++]));
                    }
                }
                else
                {
                    autofacParams.Add(new NamedParameter(item.Name, parameters[index++]));
                }

            }


            var result = RunScenarioStep<TScenario>(autofacParams.ToArray());
            return result;
        }

        public ScenarioResult RunScenario<TScenario>()
        {
            var result = RunScenarioStep<TScenario>();
            return result;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        private ScenarioResult RunScenarioStep<TScenario>(params NamedParameter[] parameters)
        {
            try
            {
                AssembleMetaDataForScenario<TScenario>();
            }
            catch (Exception exception)
            {
                return new ScenarioResult(isSuccessful: false, numberScenarioStepInvoked: 0, exceptions: exception);
            }

            TScenario scenario;

            if (parameters != null && parameters.Any() == true)
                scenario = _containerBuilder.Resolve<TScenario>(parameters);
            else
                scenario = _containerBuilder.Resolve<TScenario>();


            var result = RunScenario(scenario);
            return result;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        private ScenarioResult RunScenario<TScenario>(TScenario scenario)
        {
            var setupStepsResult = RunAllteps(scenario, _scenarioSetups);
            ScenarioResult businessStepsResult = new EmptyScenarioResult();

            if (setupStepsResult.IsSuccessful)
            {
                businessStepsResult = RunAllteps(scenario, _scenarioSteps);
            }

            var cleanupStepsResult = RunAllteps(scenario, _scenarioCleanups);

            return setupStepsResult + businessStepsResult + cleanupStepsResult;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        private ScenarioResult RunAllteps<TScenario>(
            TScenario scenario,
            IDictionary<int, StepAndAttributeBundle> steps)
        {
            ScenarioResult scenarioResult = new EmptyScenarioResult();
            object[] previousParameters = null;
            var tasks = new List<Tuple<int, Task<ScenarioStepReturnResultBundle>>>();

            var orderedMethods = steps.OrderBy(x => x.Key).ToList();

            for (var index = 0; index < orderedMethods.Count;)
            {
                var method = orderedMethods[index].Value.MethodInfo;
                var timeout = orderedMethods[index].Value.Timeout;
                var isParallel = orderedMethods[index].Value.ScenarioStepParallelAttribute != null;

                ScenarioResult scenarioResultCurrent;
                var taskScenarioStepBundle = RunScenarioStep(scenario, method, timeout, previousParameters, orderedMethods[index].Value.BusinessStepScenario);

                if (isParallel == true)
                {
                    tasks.Add(new Tuple<int, Task<ScenarioStepReturnResultBundle>>(index++, taskScenarioStepBundle));
                    continue;
                }

                var resultBundle = taskScenarioStepBundle.Result;


                _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");
                _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");
                _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");


                scenarioResult += resultBundle.ScenarioResult;

                if (resultBundle.ScenarioResult.IsSuccessful == false)
                {
                    if (orderedMethods[index].Value.ExceptionIgnoreAttribute == null)
                    {
                        return scenarioResult;
                    }

                    scenarioResult |= new EmptyScenarioResult(isSuccessful: true);
                }

                previousParameters = resultBundle.ScenarioSStepReturn.Parameters;

                var scenarioStepJumpToStep = resultBundle.ScenarioSStepReturn as ScenarioStepJumpToStep;

                if (scenarioStepJumpToStep != null)
                {
                    for (int i = index + 1; i < orderedMethods.Count; i++)
                    {
                        if (orderedMethods[i].Key == scenarioStepJumpToStep.IndexToJumpToStep)
                        {
                            index = i;
                            break;
                        }
                    }
                    //TODO elsewhere  throw exceptions.
                }
                else
                {
                    index++;
                }
            }

            var pureTasks = tasks.Select(x => x.Item2).ToArray();
            Task.WaitAll(pureTasks);
            foreach (var task in tasks)
            {
                scenarioResult += task.Item2.Result.ScenarioResult;

                if (task.Item2.Result.ScenarioResult.IsSuccessful == false)
                {
                    if (orderedMethods[task.Item1].Value.ExceptionIgnoreAttribute == null)
                    {
                        //return scenarioResult;
                        continue;
                    }

                    scenarioResult |= new EmptyScenarioResult(isSuccessful: true);
                }
            }

            return scenarioResult;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        private async Task<ScenarioStepReturnResultBundle> RunScenarioStep<TScenario>(
            TScenario scenario,
            MethodInfo method,
            TimeSpan timeout,
            object[] previousParameters,
            ABusinessStepScenarioAttribute businessStepScenario)
        {
            ScenarioStepReturnBase scenarioStepResult = null;
            try
            {
                _logger.Information(string.Format("[{0}] Step was started.", businessStepScenario.Description));
                //await Task.Yield();
                await Task.Run(() =>
                {
                    scenarioStepResult = _methodInvoker.MethodInvoke(scenario, method, timeout, previousParameters);
                });
            }
            catch (AScenarioStepTimeoutException timeoutException)
            {
                _logger.Error(string.Format("[{0}] Step was finished usuccessfully due to timeout.", businessStepScenario.Description));
                {
                    var scenarioResult = new ScenarioResult(isSuccessful: false, numberScenarioStepInvoked: 1, exceptions: timeoutException);
                    return new ScenarioStepReturnResultBundle(new ScenarioStepReturnVoid(), scenarioResult);
                }
            }

            catch (Exception exception)
            {
                _logger.Error(string.Format("[{0}] Step was finished usuccessfully due to the following exception [{1}].", businessStepScenario.Description, exception));

                {
                    var scenarioResult = new ScenarioResult(isSuccessful: false, numberScenarioStepInvoked: 1, exceptions: exception);
                    return new ScenarioStepReturnResultBundle(new ScenarioStepReturnVoid(), scenarioResult);
                }
            }

            _logger.Information(string.Format("[{0}] Step was finished successfully.", businessStepScenario.Description));

            return new ScenarioStepReturnResultBundle(scenarioStepResult, new EmptyScenarioResult(numberScenarioStepInvoked: 1));
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        private void AssembleMetaDataForScenario<TScenario>()
        {
            if (Attribute.IsDefined(typeof(TScenario), typeof(AScenarioAttribute)) == false)
            {
                throw new AScenarioMissingAttributeException();
            }

            foreach (var method in typeof(TScenario).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var exceptionIgnoreAttribute = method.GetCustomAttribute<AExceptionIgnoreAttribute>();
                var scenarioStepParallelAttribute = method.GetCustomAttribute<AScenarioStepParallelAttribute>();
                var timeoutAttribute = method.GetCustomAttribute<AScenarioStepTimeoutAttribute>();

                var setupAttribute = method.GetCustomAttribute(typeof(AStepSetupScenarioAttribute)) as AStepSetupScenarioAttribute;
                if (setupAttribute != null)
                {
                    _scenarioSetups.Add(setupAttribute.Index, new StepAndAttributeBundle(methodInfo: method, businessStepScenario: setupAttribute, exceptionIgnoreAttribute: exceptionIgnoreAttribute, scenarioStepTimeoutAttribute: timeoutAttribute, scenarioStepParallelAttribute: scenarioStepParallelAttribute));
                    continue;
                }

                var cleanupAttribute = method.GetCustomAttribute(typeof(AStepCleanupScenarioAttribute)) as AStepCleanupScenarioAttribute;
                if (cleanupAttribute != null)
                {
                    _scenarioCleanups.Add(cleanupAttribute.Index, new StepAndAttributeBundle(methodInfo: method, businessStepScenario: cleanupAttribute, exceptionIgnoreAttribute: exceptionIgnoreAttribute, scenarioStepTimeoutAttribute: timeoutAttribute, scenarioStepParallelAttribute: scenarioStepParallelAttribute));
                    continue;
                }

                var attribute = method.GetCustomAttribute(typeof(ABusinessStepScenarioAttribute)) as ABusinessStepScenarioAttribute;
                if (attribute != null)
                {
                    _scenarioSteps.Add(attribute.Index, new StepAndAttributeBundle(methodInfo: method, businessStepScenario: attribute, exceptionIgnoreAttribute: exceptionIgnoreAttribute, scenarioStepTimeoutAttribute: timeoutAttribute, scenarioStepParallelAttribute: scenarioStepParallelAttribute));
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        private class MethodInvoker
        {
            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            public MethodInvoker()
            {
            }

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------
            [DebuggerStepThrough]
            public ScenarioStepReturnBase MethodInvoke<TScenario>(
                TScenario scenario,
                MethodInfo methodInfo,
                TimeSpan timeout,
                params object[] parameters)
            {

                ScenarioStepReturnBase result = null;

                try
                {
                    var task = Invoke<TScenario>(scenario, methodInfo, parameters);

                    if (timeout != TimeSpan.Zero && task.Wait(timeout: timeout) == false)
                    {
                        try
                        {
                            task.Dispose();
                        }
                        catch (Exception)
                        {
                            throw new AScenarioStepTimeoutException();
                        }

                    }
                    result = task.Result;
                }
                catch (Exception exception)
                {
                    throw exception.InnerException ?? exception;
                }


                return result;
            }

            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------    
            [DebuggerStepThrough]
            private async Task<ScenarioStepReturnBase> Invoke<TScenario>(
              TScenario scenario,
              MethodInfo method,
              params object[] parameters)
            {
                ScenarioStepReturnBase methodResult = null;

                var task = Task.Run(() =>
                {
                    try
                    {
                        methodResult = method.Invoke(scenario, parameters: parameters) as ScenarioStepReturnBase;
                    }
                    catch (Exception exception)
                    {
                        throw exception.InnerException;
                    }
                });

                await task;

                return methodResult ?? new ScenarioStepReturnVoid();
            }

        }
    }
}
