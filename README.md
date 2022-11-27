## Performance Counters Exporter

Universal Windows Performance Counters Exporter for OpenMetrics

Зарегистрировать службу
sc.exe create Perfmon_Exporter binpath="C:\Sites\Perfmon_Exporter\Perfmon.Exporter.Web.exe --contentRoot C:\Sites\Perfmon_Exporter\ --urls http://0.0.0.0:9183" start=auto

Открыть файервол
...