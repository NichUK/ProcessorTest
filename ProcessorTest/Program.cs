using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessorTest
{
    class Program
    {
        public const int ProcessorCount = 88;

        private static IList<Task> Tasks = new List<Task>();
        private static ILogger _logger;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Program>>();

            RunLoad(ProcessorCount);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(config => config.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace)
                .AddTransient<Program>();
        }

        public static void RunLoad(int numberOfLoads)
        {
            _logger.LogDebug("Starting");

            for (var slaveIdx = 0; slaveIdx < numberOfLoads; slaveIdx++)
            {
                Tasks.Add(GetSlaveTask(slaveIdx));
            }

            Task.WaitAll(Tasks.ToArray());
            _logger.LogDebug("All Slaves Complete");
        }

        public static Task GetSlaveTask(int index)
        {
            return Task.Factory.StartNew(stateIn =>
            {
                var idx = (int)stateIn;
                _logger.LogDebug($"Slave {idx}: Starting");
                var result = LoadSink.GregoryLeibnizGetPI(int.MaxValue);
                _logger.LogDebug($"Slave {idx}: Result={result}");
            }, index, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

    }
}
