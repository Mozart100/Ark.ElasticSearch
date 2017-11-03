using Ark.StepRunner.CustomAttribute;
using Elasticsearch.Net;
using GenFu;
using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.ElasticSearch
{
    [AScenario("Elastic scenario")]
    public class ElasticSearchScenario
    {
        private enum ScenarioSteps
        {
            CleanAllRunningProcess,
            LaunchElasticsearch,
            LaunchKibana,
            VerifyElasticSearchIsUp,
            SendingIndex,
            CancelAllRunningThreads,
            CleanAllRunningProcesses,
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        const string ElasticSearchExe = @"C:\Program Files\Elastic\Elasticsearch\bin\elasticsearch.exe";
        const string ElsticSearchNode = @"C:\ProgramData\Elastic\Elasticsearch\data\nodes";
        const string KibanahExe = @"C:\Program Files\Kibana\bin\kibana.bat";


        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        private readonly ILogger _logger;
        private readonly Process _processElasticsearch;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly Process _processKibana;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ElasticSearchScenario(ILogger logger)
        {
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _processElasticsearch = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ElasticSearchExe,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(ElasticSearchExe),
                    Arguments = ""

                }
            };


            _processKibana = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = KibanahExe,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(KibanahExe),
                    Arguments = ""

                }
            };
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        [AStepSetupScenario((int)ScenarioSteps.CleanAllRunningProcess, "Clean AllRunning Process.")]
        public void CleanAllRunningProcess()
        {
            CleanAllRunningProcesses_ElastiSearch_Kibana();

            Directory.Delete(ElsticSearchNode, recursive: true);
        }

        //--------------------------------------------------------------------------------------------------------------------------------------


        [AStepSetupScenario((int)ScenarioSteps.LaunchElasticsearch, "Launch Elastic Search Agent.")]
        public void LaunchElasticSearch()
        {
            _processElasticsearch.Start();
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        [AStepSetupScenario((int)ScenarioSteps.LaunchKibana, "Launch Kibana Agent.")]
        public void LaunchKibana()
        {
            _processKibana.Start();
        }

        //--------------------------------------------------------------------------------------------------------------------------------------


        [AStepSetupScenario((int)ScenarioSteps.VerifyElasticSearchIsUp, "Verify Elastic Search is up.")]
        public void VerifyElasticSearchIsUp()
        {

            while (_cancellationToken.IsCancellationRequested == false)
            {

                var request = WebRequest.Create("http://localhost:9200");
                request.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    var response = request.GetResponse();
                    return;
                }
                catch (Exception exception)
                {
                    _logger.Warning(exception, "VerifyElasticSearchIsUp Step.");

                }
            }

        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        [ABusinessStepScenario((int)ScenarioSteps.SendingIndex, "Sending Index.")]
        public void SendingIndex()
        {
            A.Configure<Person>()
             .Fill(p => p.Age)
             .WithinRange(19, 100);

            var people = A.ListOf<Person>(200);

            var uri = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(uri).DefaultIndex("ark-personstorage");
            var elasticLowLevelClient = new ElasticLowLevelClient(settings);
            var client = new ElasticClient(settings);


            foreach (var person in people)
            {
                var tmp = client.Index(person);

                _logger.Information("The response is {@person}", person);
                //_logger.Information("The response is {@tmp}", tmp);
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------------------

        [AStepCleanupScenario((int)ScenarioSteps.CancelAllRunningThreads, "Cancel All running threads.")]
        public void CancelAllRunningThreads()
        {
            _cancellationTokenSource.Cancel();
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        [AExceptionIgnore]
        [AStepCleanupScenario((int)ScenarioSteps.CleanAllRunningProcesses, "Kill Elastic Search.")]
        public void CleanAllRunningProcesses()
        {
            CleanAllRunningProcesses_ElastiSearch_Kibana();

        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------


        private void CleanAllRunningProcesses_ElastiSearch_Kibana()
        {
            foreach (var item in Process.GetProcesses())
            {
                if (item.ProcessName.ToLower().Equals("elasticsearch") ||
                    item.ProcessName.ToLower().Equals("node") ||
                    item.ProcessName.ToLower().Equals("java"))
                {
                    try
                    {
                        _logger.Information("Process name = {ProcessName} is killed ", item.ProcessName);
                        item.Kill();
                    }
                    catch (Exception exception)
                    {
                        _logger.Warning(exception, "During killing  of step CleanAllRunningProcess.");

                    }
                }
            }

          
        }
    }



}
