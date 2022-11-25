using Microsoft.Extensions.Logging;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Collector
	{
		public static string Collect(PerfomanceCountersConfiguration config, ILogger<Collector> logger)
		{
			StringBuilder ret = new StringBuilder();
			foreach (var categoryConfig in config.Categories)
			{
				if (!PerformanceCounterCategory.Exists(categoryConfig.Name))
				{
					logger.LogError("PerformanceCounterCategory {0} does not exists", categoryConfig.Name);
					continue;
				}

				PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryConfig.Name);
				string[] instaces = cat.GetInstanceNames();

				foreach(var counterConfig in categoryConfig.Counters)
				{
					List<PerformanceCounter> counterList;
					if (instaces.Length == 0)
					{
						counterList = new List<PerformanceCounter>();
						counterList.Add(new PerformanceCounter(categoryConfig.Name, counterConfig.Name));
					}
					else
					{
						if (categoryConfig.SplitInstances)
						{
							counterList = instaces.Select(s => new PerformanceCounter(categoryConfig.Name, counterConfig.Name, s)).ToList();
						}
						else
						{
							counterList = new List<PerformanceCounter>();
							counterList.Add(new PerformanceCounter(categoryConfig.Name, counterConfig.Name, "_Total"));
						}
					}
					string counterName = config.Prefix + "_" + categoryConfig.Prefix + "_" + counterConfig.Prefix;
					ret.AppendLine("# HELP " + counterName + " " + (counterConfig.CustomDescription ? counterConfig.Description : counterList[0].CounterHelp));
					ret.AppendLine("# HELP " + counterName + " " + counterConfig.Kind);
					foreach (PerformanceCounter counter in counterList)
					{
						string counterFullName = counterName + (counter.InstanceName==""?"":" {"+categoryConfig.InstanceLabel+"=\""+ counter.InstanceName + "\"}");
						ret.AppendLine(counterFullName + " " + counter.NextValue());
					}
				}
			}

			return ret.ToString();
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
