using Ark.StepRunner;
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
using Serilog.Sinks.Elasticsearch;
using Ark.TracePyramid;

namespace Ark.ElasticSearch
{
    class Program
    {

        const string OutTemplate = "{Environment} | {Timestamp:yyyy-MM-dd HH:mm:ss} | [{Level}] | {Message}{NewLine}{Exception}";

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
                //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                //{

                //    AutoRegisterTemplate = true,
                //    //MinimumLogEventLevel = (LogEventLevel)esConfig.MinimumLogEventLevel,
                //    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                //    IndexFormat = "ark-personstorage"
                //})
                .Enrich.WithProperty("Environment", "Developer")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static IContainer InitDi()
        {
            //var uri = new Uri("http://localhost:9200");
            //var settings = new ConnectionSettings(uri).DefaultIndex("ark-personstorage");
            //var elasticLowLevelClient = new ElasticLowLevelClient(settings);
            //var client = new ElasticClient(settings);
            var elasticSearchConfig = new ElasticConfig(uri: "http://localhost:9200", index: "ark-personstorage");
            var structuredLogger = new StructuredLog(_logger, elasticSearchConfig);

            var container = new ContainerBuilder();

            container.Register<IStructuredLog>(x => structuredLogger);
            container.RegisterType<ElasticSearchScenario>();
            container.Register(x => elasticSearchConfig);


            return container.Build();
        }
    }


}
