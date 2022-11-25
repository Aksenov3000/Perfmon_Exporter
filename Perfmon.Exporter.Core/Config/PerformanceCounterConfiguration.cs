namespace Perfmon.Exporter.Core.Config
{
	public class PerformanceCounterConfiguration
	{
		public string Name { get; set; } = "";
		public bool CustomDescription { get; set; } = false;
		public string? Description { get; set; } = null;
		public string Kind { get; set; } = "";
		public string Prefix { get; set; } = "";
	}
}
