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

			logger.LogDebug($"Parent.InstaceNames.Length={Parent.InstaceNames.Length} Parent.Config.SplitInstances={Parent.Config.SplitInstances}");
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
			logger.LogDebug($"Instances.Count={Instances.Count}");

			CounterName = mainConfig.Prefix + "_" + Parent.Config.Prefix + "_" + Config.Prefix;
			CounterHelp = "# HELP " + CounterName + " " + (Config.CustomDescription ? Config.Description : Instances[0].CounterHelp);
			CounterType = "# TYPE " + CounterName + " " + Config.Kind;
			foreach (PerformanceCounter counter in Instances)
			{
				try
				{
					string instanceFullName = CounterName + (counter.InstanceName == "" ? "" : " {" + (Parent.Config.InstanceLabel == "" ? "instance" : Parent.Config.InstanceLabel) + "=\"" + counter.InstanceName + "\"}");
					InstanceFullNames.Add(instanceFullName);
					float val = counter.NextValue();
					logger.LogDebug($"instanceFullName={instanceFullName} Val={val}");
				}
				catch (Exception ex)
				{
					Logger.LogError($"Cant Init counter. counter.InstanceName=[{counter.InstanceName}] counter.CategoryName=[{counter.CategoryName}] counter.CounterName=[{counter.CounterName}] {ex}");
					throw;
				}
			}
		}

		public void Collect(StringBuilder ret)
		{
			ret.AppendLine(CounterHelp);
			ret.AppendLine(CounterType);
			for (int i = 0; i < Instances.Count; i++)
			{
				try
				{
					if (Instances[i].InstanceName == "")
					{
						ret.AppendLine(InstanceFullNames[i] + " " + Instances[i].NextValue().ToString().Replace(",", "."));
					}
					else if (Parent.Instance.InstanceExists(Instances[i].InstanceName))
					{
						ret.AppendLine(InstanceFullNames[i] + " " + Instances[i].NextValue().ToString().Replace(",", "."));
					}
					else
					{
						Logger.LogWarning($"Cant collect. Instance not found.  counter.InstanceName=[{Instances[i].InstanceName}] counter.CategoryName=[{Instances[i].CategoryName}] counter.CounterName=[{Instances[i].CounterName}] ");
					}
				}
				catch (Exception ex)
				{
					Logger.LogError($"counter.InstanceName=[{Instances[i].InstanceName}] counter.CategoryName=[{Instances[i].CategoryName}] counter.CounterName=[{Instances[i].CounterName}] {ex}");
				}
			}
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
