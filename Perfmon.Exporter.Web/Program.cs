using NLog;
using NLog.Web;
using Perfmon.Exporter.Core;
using Perfmon.Exporter.Core.Config;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
	DateTime start = DateTime.Now;
	var builder = WebApplication.CreateBuilder(args);
	builder.Services.Configure<PerfomanceCountersConfiguration>(builder.Configuration.GetSection("PerformanceCounters"));
	builder.Services.AddSingleton<Collector>();
	builder.Logging.ClearProviders();
	builder.Host.UseNLog();
	builder.Host.UseWindowsService();
	var app = builder.Build();
	logger.Info($"init main builder took {(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms");
	_ = app.Services.GetRequiredService<Collector>();
	logger.Info($"init main took {(int)DateTime.Now.Subtract(start).TotalMilliseconds}ms");

	app.MapGet("/", async (context) =>
	{
		context.Response.Headers.ContentType = "text/html; charset=utf-8";
		await context.Response.WriteAsync("<html><body>Go to <a href=\"/metrics\">/metrics</a></body></html>");
	});
	app.MapGet("/metrics", async (context) =>
	{
		context.Response.Headers.ContentType = "text/plain; charset=utf-8";
		Collector collector = context.RequestServices.GetRequiredService<Collector>();
		StringBuilder sb = new StringBuilder();
		collector.Collect(sb);
		await context.Response.WriteAsync(sb.ToString(), context.RequestAborted);
	});

	app.Run();
}
catch (Exception exception)
{
	// NLog: catch setup errors
	logger.Error(exception, "Stopped program because of exception");
	throw;
}
finally
{
	// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
	NLog.LogManager.Shutdown();
}