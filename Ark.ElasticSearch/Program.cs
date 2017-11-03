﻿using Ark.StepRunner;
using Autofac;
using Elasticsearch.Net;
using GenFu;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;

namespace Ark.ElasticSearch
{
    class Program
    {

        const string OutTemplate = " {Environment} | {Timestamp:yyyy-MM-dd HH:mm:ss} | [{Level}] | {Message}{NewLine}{Exception}";

        //const string OutTemplate = " {Environment} | {Timestamp:yyyy-MM-dd HH:mm:ss} | [{Level}] | {Bookmark} | {Message}{NewLine}{Exception}";

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        private static Logger _logger;

        static void Main(string[] args)
        {
            //var uri = new Uri("http://localhost:9200");
            //var singleConnectionPool = new SingleNodeConnectionPool(uri);
            //var settings = new ConnectionConfiguration(singleConnectionPool);

            //var elasticLowLevelClient = new ElasticLowLevelClient(settings);

            CreateLogger();
            var container = InitDi();

            var stepRunner = new ScenarioRunner(_logger, container);
            var scenarioResult = stepRunner.RunScenario<ElasticSearchScenario>();

#if DEBUG
            _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");
            _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");
            _logger.Information("-------------------------------------------------------------------------------------------------------------------------------------");

            _logger.Information("Please press any key in order to exit.");
            Console.ReadKey();
#endif


            //elasticLowLevelClient.clos();



        }

        private static void CreateLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: OutTemplate)
                .Enrich.WithProperty("Environment", "Developer")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static IContainer InitDi()
        {
            var container = new ContainerBuilder();

            container.Register<ILogger>(x => _logger);
            container.RegisterType<ElasticSearchScenario>();

            return container.Build();
        }
    }


}