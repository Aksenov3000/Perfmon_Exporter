using Microsoft.Extensions.Logging;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Category
	{
		public PerfomanceCountersConfiguration MainConfig { get; set; }
		public PerformanceCounterCategoryConfiguration Config { get; set; }
		public PerformanceCounterCategory Instance { get; set; }
		public PerformanceCounterCategoryType CategoryType { get; set; }

		private ILogger<Collector> Logger;

		public Category(PerfomanceCountersConfiguration mainConfig, PerformanceCounterCategoryConfiguration config, ILogger<Collector> logger)
		{
			Config = config;
			Logger = logger;
			MainConfig = mainConfig;

			//if (!PerformanceCounterCategory.Exists(Config.Name))
			//{
			//	logger.LogError($"PerformanceCounterCategory {Config.Name} does not exists");
			//	throw new NotSupportedException($"PerformanceCounterCategory {Config.Name} does not exists");
			//}
			//logger.LogDebug($"PerformanceCounterCategory {Config.Name} exists");

			Instance = new PerformanceCounterCategory(Config.Name);
			CategoryType = Instance.CategoryType;
			logger.LogDebug($"PerformanceCounterCategory {Config.Name} CategoryType = [{CategoryType}]");

			//foreach (var counterConfig in Config.Counters)
			//{
			//	Counters.Add(new Counter(mainConfig, this, counterConfig, logger));
			//}
		}

		public void Collect(StringBuilder ret)
		{
			foreach (var counterConfig in Config.Counters)
			{
				Counter counter = new Counter(MainConfig, this, counterConfig, Logger);
				counter.Collect(ret);
			}
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
