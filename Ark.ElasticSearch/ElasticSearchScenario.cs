using Ark.StepRunner.CustomAttribute;
using Ark.StepRunner.ScenarioStepResult;
using Ark.TracePyramid;
using Elasticsearch.Net;
using GenFu;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            VerifyingEverythingInElasticsearch,
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

        private readonly IStructuredLog _logger;
        private readonly Process _processElasticsearch;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly Process _processKibana;
        private readonly ElasticConfig _elasticConfig;
        private readonly IList<string> _countries;

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public ElasticSearchScenario(IStructuredLog logger, ElasticConfig elasticConfig)
        {
            _logger = logger;
            _elasticConfig = elasticConfig;

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

            _countries = GetCountries();
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        [AExceptionIgnore]
        [AStepSetupScenario((int)ScenarioSteps.CleanAllRunningProcess, "Clean AllRunning Process.")]
        public void CleanAllRunningProcess()
        {
            CleanAllRunningProcesses_ElastiSearch_Kibana();

            int x = 10;
            while (x-- > 0)
            {
                try
                {
                    Directory.Delete(ElsticSearchNode, recursive: true);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return;
                }
                catch (Exception exception)
                {
                    _logger.Warning(exception, "Trying to remove old elastic search data.");
                }
            }

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
                var request = WebRequest.Create(_elasticConfig.Uri);
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
        public ScenarioStepReturnNextStep SendingIndex()
        {

            int id = 0;
            Random random = new Random();

            A.Configure<Person>()
             .Fill(p => p.Id, () => ++id)
             .Fill(p => p.Name).AsFirstName()
             .Fill(p => p.FamilyName).AsLastName()
             .Fill(p => p.Age).WithinRange(19, 60)
             .Fill(p => p.City).AsCity()
             .Fill(p => p.Country).WithRandom(_countries)
             .Fill(p => p.RecordDate, () => DateTime.Now.AddYears(-random.Next(0, 5)));

            var people = A.ListOf<Person>(200);

            id = 0;
            A.Configure<IncidentReport>()
                .Fill(r => r.Id, () => id++)
                .Fill(r => r.ReportedBy)
                .WithRandom(people);

            var incidents = A.ListOf<IncidentReport>(250);


            int index = 0, age = 50, isOver50 = 0;


            foreach (var incident in incidents)
            {
                var tmp = _logger.StoreIndex(incident);

                _logger.Information("[{index}]: The {@incident}", ++index, incident);
                if (incident.ReportedBy.Age == age)
                {
                    isOver50++;
                }
            }

            return new ScenarioStepReturnNextStep(age, isOver50);
        }

        //--------------------------------------------------------------------------------------------------------------------------------------

        [ABusinessStepScenario((int)ScenarioSteps.VerifyingEverythingInElasticsearch, "Verifying Everything In Elasticsearch.")]
        public void VerifyingEverythingInElasticsearch(int age, int isOver50)
        {
            var uri = new Uri(_elasticConfig.Uri);
            var settings = new ConnectionSettings(uri).DefaultIndex(_elasticConfig.Index);
            var elasticLowLevelClient = new ElasticLowLevelClient(settings);
            var client = new ElasticClient(settings);

            var searchResults = client.Search<Person>(s => s
                                      .From(0)
                                      .Size(10)
                                      .Query(q => q
                                      .Term(p => p.Age, age.ToString())));

            Assert.AreEqual(isOver50, searchResults.Documents.Count, "isOver50 != searchResults.Documents.Count");
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


        private IList<string> GetCountries()
        {
            var regionInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                         .Select(c => new RegionInfo(c.LCID))
                                         .Distinct()
                                         .Select(x=>x.EnglishName)
                                         .ToList();

            return new List<string> { "Iraq", "Israel", "Canada", "Italy" , "Brazil" , "Russia"};

           
            //List<RegionInfo> countries = new List<RegionInfo>();
            //foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            //{
            //    RegionInfo country = new RegionInfo(culture.LCID);
            //    if (countries.Where(p => p.Name == country.Name).Count() == 0)
            //        countries.Add(country);
            //}
            //var countries2 = countries.OrderBy(p => p.EnglishName).ToList();

            return regionInfos;
        }
    }



}
