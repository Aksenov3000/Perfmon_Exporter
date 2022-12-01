using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Perfmon.Exporter.Core.Config;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Collector
	{
		private PerfomanceCountersConfiguration Config;
		private ILogger<Collector> Logger;

		public List<Category> Categories { get; set; } = new List<Category>();

		public Collector(IOptions<PerfomanceCountersConfiguration> config, ILogger<Collector> logger)
		{
			DateTime start = DateTime.Now;
			try
			{
				logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor 01");
				Config = config.Value;
				Logger = logger;
				logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor 02");
				foreach (var categoryConfig in Config.Categories)
				{
					logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor 03.1 {categoryConfig.Name}");
					Categories.Add(new Category(Config, categoryConfig, logger));
					logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor 03.2 {categoryConfig.Name}");
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor error {ex}");
			}
			logger.LogDebug($"{(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms Collector constructor 04");
		}

		public void Collect(StringBuilder ret)
		{
			foreach (Category cat in Categories) cat.Collect(ret);
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
