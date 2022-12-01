namespace Perfmon.Exporter.Core.Config
{
	public class PerformanceCounterConfiguration
	{
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
		public string Kind { get; set; } = "";
		public string Prefix { get; set; } = "";
	}
}
