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
		public List<Counter> Counters { get; set; } = new List<Counter>();

		private ILogger<Collector> Logger;

		public Category(PerfomanceCountersConfiguration mainConfig, PerformanceCounterCategoryConfiguration config, ILogger<Collector> logger)
		{
			DateTime start = DateTime.Now;
			try
			{
				logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Category constructor 01");
				Config = config;
				Logger = logger;
				MainConfig = mainConfig;
				logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Category constructor 02");
				Instance = new PerformanceCounterCategory(Config.Name);
				logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Category constructor 03");
				Counters = Config
					.Counters
					.Select(counterConfig => new Counter(MainConfig, this, counterConfig, Logger))
					.ToList();
			}
			catch (Exception ex)
			{
				logger.LogError($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Category constructor error {ex}");
			}
			logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Category constructor 04");
		}

		public void Collect(StringBuilder ret)
		{
			foreach (var counter in Counters) counter.Collect(ret);
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
