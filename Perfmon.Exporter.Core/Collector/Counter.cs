using Microsoft.Extensions.Logging;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Counter
	{
		public Category Parent { get; set; }
		public PerformanceCounterConfiguration Config { get; set; }
		public List<PerformanceCounter> Instances { get; set; }
		public List<string> InstanceFullNames { get; set; } = new List<string>();

		private ILogger<Collector> Logger;

		private string CounterName;
		private string CounterHelp;
		private string CounterType;


		public Counter(PerfomanceCountersConfiguration mainConfig, Category category, PerformanceCounterConfiguration config, ILogger<Collector> logger)
		{
			Parent = category;
			Config = config;
			Logger = logger;

			if (Parent.InstaceNames.Length == 0)
			{
				Instances = new List<PerformanceCounter>();
				Instances.Add(new PerformanceCounter(Parent.Config.Name, Config.Name));
			}
			else
			{
				if (Parent.Config.SplitInstances)
				{
					Instances = Parent.InstaceNames.Select(s => new PerformanceCounter(Parent.Config.Name, Config.Name, s)).ToList();
				}
				else
				{
					Instances = new List<PerformanceCounter>();
					Instances.Add(new PerformanceCounter(Parent.Config.Name, Config.Name, "_Total"));
				}
			}
			CounterName = mainConfig.Prefix + "_" + Parent.Config.Prefix + "_" + Config.Prefix;
			CounterHelp = "# HELP " + CounterName + " " + (Config.CustomDescription ? Config.Description : Instances[0].CounterHelp);
			CounterType = "# TYPE " + CounterName + " " + Config.Kind;
			foreach (PerformanceCounter counter in Instances)
			{
				InstanceFullNames.Add(CounterName + (counter.InstanceName == "" ? "" : " {" + (Parent.Config.InstanceLabel == "" ? "instance" : Parent.Config.InstanceLabel) + "=\"" + counter.InstanceName + "\"}"));
				counter.NextValue();
			}
		}

		public void Collect(StringBuilder ret)
		{
			ret.AppendLine(CounterHelp);
			ret.AppendLine(CounterType);
			for (int i = 0; i < Instances.Count; i++)
			{
				if (Instances[i].InstanceName == "")
				{
					ret.AppendLine(InstanceFullNames[i] + " " + Instances[i].NextValue().ToString().Replace(",", "."));
				}
				else if (Parent.Instance.InstanceExists(Instances[i].InstanceName))
				{
					ret.AppendLine(InstanceFullNames[i] + " " + Instances[i].NextValue().ToString().Replace(",", "."));
				}
			}
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
