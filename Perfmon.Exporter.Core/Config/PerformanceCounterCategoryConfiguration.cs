namespace Perfmon.Exporter.Core.Config
{
	public class PerformanceCounterCategoryConfiguration
	{
		public string Name { get; set; } = "";
		public bool SplitInstances { get; set; } = false;
		public string InstanceLabel { get; set; } = "";
		public string Prefix { get; set; } = "";
		public List<PerformanceCounterConfiguration> Counters { get; set; } = new List<PerformanceCounterConfiguration>();
	}
}
