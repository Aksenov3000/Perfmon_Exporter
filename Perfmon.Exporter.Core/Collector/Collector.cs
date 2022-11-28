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
			Config = config.Value;
			Logger = logger;
			foreach (var categoryConfig in Config.Categories)
			{
				Categories.Add(new Category(Config, categoryConfig, logger));
			}
		}

		public void Collect(StringBuilder ret)
		{
			foreach (Category cat in Categories) cat.Collect(ret);
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
