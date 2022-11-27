using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Perfmon.Exporter.Core.Config;
using System.Diagnostics;
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

		public class Category
		{
			public PerformanceCounterCategoryConfiguration Config { get; set; }
			public PerformanceCounterCategory Instance { get; set; }
			public List<Counter> Counters { get; set; } = new List<Counter>();
			public string[] InstaceNames { get; set; }

			private ILogger<Collector> Logger;

			public Category(PerfomanceCountersConfiguration mainConfig, PerformanceCounterCategoryConfiguration config, ILogger<Collector> logger)
			{
				Config = config;
				Logger = logger;

				if (!PerformanceCounterCategory.Exists(Config.Name))
				{
					logger.LogError($"PerformanceCounterCategory {Config.Name} does not exists");
					throw new NotSupportedException($"PerformanceCounterCategory {Config.Name} does not exists");
				}

				Instance = new PerformanceCounterCategory(Config.Name);
				InstaceNames = Instance.GetInstanceNames();

				foreach (var counterConfig in Config.Counters)
				{
					Counters.Add(new Counter(mainConfig, this, counterConfig, logger));
				}
			}

			public void Collect(StringBuilder ret)
			{
				foreach (Counter counter in Counters) counter.Collect(ret);
			}
		}

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
					InstanceFullNames.Add(CounterName + (counter.InstanceName == "" ? "" : " {" + (Parent.Config.InstanceLabel==""?"instance": Parent.Config.InstanceLabel) + "=\"" + counter.InstanceName + "\"}"));
					counter.NextValue();
				}
			}

			public void Collect(StringBuilder ret)
			{
				ret.AppendLine(CounterHelp);
				ret.AppendLine(CounterType);
				for (int i=0;i< Instances.Count;i++)
				{
					ret.AppendLine(InstanceFullNames[i] + " " + Instances[i].NextValue().ToString().Replace(",", "."));
				}
			}
		}
	}
}
#pragma warning restore CA1416 // Validate platform compatibility
