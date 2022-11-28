using Microsoft.Extensions.Logging;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Category
	{
		public PerformanceCounterCategoryConfiguration Config { get; set; }
		public PerformanceCounterCategory Instance { get; set; }
		public List<Counter> Counters { get; set; } = new List<Counter>();
		public string[] InstaceNames { get; set; }

		private ILogger<Collector> Logger;

		public Category(PerfomanceCountersConfiguration mainConfig, PerformanceCounterCategoryConfiguration config, ILogger<Collector> logger)
		{
			Config = config;
			Logger = logger;

			if (!PerformanceCounterCategory.Exists(Config.Name))
			{
				logger.LogError($"PerformanceCounterCategory {Config.Name} does not exists");
				throw new NotSupportedException($"PerformanceCounterCategory {Config.Name} does not exists");
			}

			Instance = new PerformanceCounterCategory(Config.Name);
			InstaceNames = Instance.GetInstanceNames();

			foreach (var counterConfig in Config.Counters)
			{
				Counters.Add(new Counter(mainConfig, this, counterConfig, logger));
			}
		}

		public void Collect(StringBuilder ret)
		{
			foreach (Counter counter in Counters) counter.Collect(ret);
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
