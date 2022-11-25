using Microsoft.Extensions.Options;
using Perfmon.Exporter.Core;
using Perfmon.Exporter.Core.Config;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<PerfomanceCountersConfiguration>(builder.Configuration.GetSection("PerformanceCounters"));
var app = builder.Build();

app.MapGet("/", async (context) =>
{
	context.Response.Headers.ContentType = "text/html; charset=utf-8";
	await context.Response.WriteAsync("<html><body>Go to <a href=\"/metrics\">/metrics</a></body></html>");
});
app.MapGet("/metrics", async (context) =>
{
	context.Response.Headers.ContentType = "text/plain; version=0.0.4; charset=utf-8";
	ILogger<Collector> logger = context.RequestServices.GetRequiredService<ILogger<Collector>>();
	IOptions<PerfomanceCountersConfiguration> options = context.RequestServices.GetRequiredService<IOptions<PerfomanceCountersConfiguration>>();
	await context.Response.WriteAsync(Collector.Collect(options.Value, logger), context.RequestAborted);
});

app.Run();
