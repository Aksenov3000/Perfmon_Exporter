using Microsoft.Extensions.Logging;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Perfmon.Exporter.Core
{
	public class Counter
	{
		public class PerformanceCounterWithName
		{
			public PerformanceCounter Counter { get; set; }
			public string InstanceName { get; set; }
			public string FullInstanceName { get; set; }

			public PerformanceCounterWithName(string instanceName, Counter counter)
			{
				if (instanceName == "")
				{
					Counter = new PerformanceCounter(counter.Parent.Config.Name, counter.Config.Name);
				}
				else
				{
					Counter = new PerformanceCounter(counter.Parent.Config.Name, counter.Config.Name, instanceName);
				}
				InstanceName = instanceName;
				FullInstanceName = counter.CounterName + (Counter.InstanceName == "" ? "" : " {" + (counter.Parent.Config.InstanceLabel == "" ? "instance" : counter.Parent.Config.InstanceLabel) + "=\"" + Counter.InstanceName + "\"}");
				Counter.NextValue();
			}
		}
		public Category Parent { get; set; }
		public PerformanceCounterConfiguration Config { get; set; }
		public Dictionary<string, PerformanceCounterWithName> Instances { get; set; } = new Dictionary<string, PerformanceCounterWithName>();

		private ILogger<Collector> Logger;
		private string CounterName;
		private string CounterHelp;
		private string CounterType;


		public Counter(PerfomanceCountersConfiguration mainConfig, Category category, PerformanceCounterConfiguration config, ILogger<Collector> logger)
		{
			Parent = category;
			Config = config;
			Logger = logger;
			CounterName = mainConfig.Prefix + "_" + Parent.Config.Prefix + "_" + Config.Prefix;
			CounterHelp = "# HELP " + CounterName + " " + Config.Description;
			CounterType = "# TYPE " + CounterName + " " + Config.Kind;
		}

		private void CheckInstances()
		{
			try
			{
				string[] InstaceNames;
				if (Parent.Instance.CategoryType == PerformanceCounterCategoryType.MultiInstance)
				{
					InstaceNames = Parent.Instance.GetInstanceNames();
					Logger.LogDebug($"InstaceNames = [{string.Join(",", InstaceNames)}] InstaceNames.Length={InstaceNames.Length} Parent.Config.SplitInstances={Parent.Config.SplitInstances}");

					if (InstaceNames.Length == 0)
					{
						foreach (var kv in Instances) kv.Value.Counter.Dispose();
						Instances = new Dictionary<string, PerformanceCounterWithName>();
					}
					else
					{
						Logger.LogDebug($"InstaceNames.Length <> 0. SplitInstances={Parent.Config.SplitInstances}");
						if (Parent.Config.SplitInstances)
						{
							List<string> namesToDelete = new List<string>();

							foreach (var kv in Instances)
							{
								if (!InstaceNames.Contains(kv.Key)) namesToDelete.Add(kv.Key);
							}
							Logger.LogDebug($"namesToDelete = [{string.Join(",", namesToDelete)}]");

							foreach (var name in namesToDelete)
							{
								Instances[name].Counter.Dispose();
								Instances.Remove(name);
							}

							foreach (string name in InstaceNames)
							{
								if (!Instances.ContainsKey(name))
								{
									Instances.Add(name, new PerformanceCounterWithName(name, this));
									Logger.LogDebug($"Instances.Add {name}");
								}
							}
						}
						else
						{
							CheckName(InstaceNames, "_Total");
						}
					}
					Logger.LogDebug($"Instances.Count={Instances.Count}");
				}
				else
				{
					InstaceNames = new string[] { "" };
					CheckName(InstaceNames, "");
				}
			}
			catch (Exception ex)
			{
				Logger.LogError($"Cant CheckInstances. CounterName={CounterName} {ex}");
				throw;
			}
		}

		private void CheckName(string[] instaceNames, string name)
		{
			if (!instaceNames.Contains(name))
			{
				Logger.LogDebug($"namesToDelete = [{name}]");
				Instances[name].Counter.Dispose();
				Instances.Remove(name);
			}
			else if (!Instances.ContainsKey(name))
			{
				Instances.Add(name, new PerformanceCounterWithName(name, this));
				Logger.LogDebug($"Instances.Add {name}");
			}
		}

		public void Collect(StringBuilder ret)
		{
			CheckInstances();

			ret.AppendLine(CounterHelp);
			ret.AppendLine(CounterType);

			foreach (var kv in Instances)
			{
				try
				{
					ret.AppendLine(kv.Value.FullInstanceName + " " + kv.Value.Counter.NextValue().ToString().Replace(",", "."));
				}
				catch (Exception ex)
				{
					Logger.LogError($"counter.InstanceName=[{kv.Value.InstanceName}] counter.CategoryName=[{kv.Value.Counter.CategoryName}] counter.CounterName=[{kv.Value.Counter.CounterName}] {ex}");
				}
			}
		}


	}
}
#pragma warning restore CA1416 // Validate platform compatibility
