namespace Perfmon.Exporter.Core.Config
{
	public class PerfomanceCountersConfiguration
	{
		public string Prefix { get; set; } = "";
		public List<PerformanceCounterCategoryConfiguration> Categories { get; set; } = new List<PerformanceCounterCategoryConfiguration>();
	}
}
