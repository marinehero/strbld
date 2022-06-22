// @nuget: BenchmarkDotNet -Version 0.12.1

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Loggers;
using Perfolizer.Horology;

[assembly: System.Diagnostics.Debuggable(isJITTrackingEnabled: false, isJITOptimizerDisabled: false)]

BenchmarkRunner.Run<Benchmark>(DefaultConfig.Instance
    .AddJob(Job.Default
        .WithToolchain(new InProcessEmitToolchain(
            timeout: TimeSpan.FromSeconds(60), // timeout for .NET Fiddle
            logOutput: false)) // Intermediate logging
        // .WithLaunchCount(1)
         .WithWarmupCount(1)
        .WithIterationCount(200)
        .WithIterationTime(TimeInterval.FromMilliseconds(200)))
    .AddLogger(new ConsoleLogger(unicodeSupport: true, ConsoleLogger.CreateGrayScheme())) // Color codes are not supported in .NET Fiddle
    .WithOptions(ConfigOptions.DisableLogFile));

[MemoryDiagnoser]
[RPlotExporter]
public class Benchmark
{
	public class DimIntersectDto
    {
        public ushort p { get; set; }
        public ushort l { get; set; }
        public ushort t { get; set; }
        public ushort m { get; set; }
        public bool elapsed { get; set; }
        public bool changed { get; set; }
        public double v { get; set; }
    }
	
    static DimIntersectDto[] _records = {
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
            new DimIntersectDto(), new DimIntersectDto(),
        };

    [Benchmark]
    public void WriteInterpolated()
    {
		var records = _records;
		byte[]? result = null;
		using var memoryStream = new MemoryStream();
		using (var streamWriter = new StreamWriter(memoryStream))
		{
			streamWriter.WriteLine("p,l,t,m,elapsed,changed,v");
			var sbt = new StringBuilder();
			var prv = new DimIntersectDto {
				p = 0,
				l = 0,
				t = 0,
				m = 0,
				v = 0,
				elapsed= false,
				changed = false,
			};
			foreach (var r in records) {
				sbt.Clear();
				// this averaged 7s vs 2.5 of using StringBuilder
				char elapsed = r.elapsed ? '1' : '0';
				char changed = r.changed ? '1' : '0';
				streamWriter.WriteLine($"{r.p},{r.l},{r.t},{r.m},{elapsed},{changed},{r.v}");
				prv = r;
			}
			streamWriter.Flush();
		};
		result = memoryStream.ToArray();
    }
	
	[Benchmark]
    public void WriteOptimized()
    {
        var records = _records;
        byte[]? result = null;
		using var memoryStream = new MemoryStream();
		using (var streamWriter = new StreamWriter(memoryStream))
		{
			streamWriter.WriteLine("p,l,t,m,elapsed,changed,v");
			var sbt = new StringBuilder();
			var prv = new DimIntersectDto {
				p = 0,
				l = 0,
				t = 0,
				m = 0,
				v = 0,
				elapsed= false,
				changed = false,
			};
			foreach (var r in records) {
				sbt.Clear();
				sbt.Append(r.p == prv.p ? string.Empty : r.p).Append(',').
					Append(r.l == prv.l ? string.Empty : r.l).Append(',').
					Append(r.t == prv.t ? string.Empty : r.t).Append(',').
					Append(r.m == prv.m ? string.Empty : r.m).Append(',').
					Append(r.elapsed == prv.elapsed ? string.Empty : r.elapsed ? '1' : '0').Append(',').
					Append(r.changed == prv.changed ? string.Empty : prv.changed ? '1' : '0').Append(',').
					Append(r.v == prv.v ? string.Empty : r.v.ToString("F2", CultureInfo.InvariantCulture)).AppendLine();
				streamWriter.Write(sbt);
				prv = r;
			}
			streamWriter.Flush();
		};
		result = memoryStream.ToArray();
		
    }

    [Benchmark]
    public void WriteOptimizedLessAlloc()
    {
        var records = _records;
        byte[]? result = null;
        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream))
        {
            streamWriter.WriteLine("p,l,t,m,elapsed,changed,v");
            var prv = new DimIntersectDto
            {
                p = 0,
                l = 0,
                t = 0,
                m = 0,
                v = 0,
                elapsed = false,
                changed = false,
            };
            foreach (var r in records)
            {
                var sbt = new ValueStringBuilder(stackalloc char[40]);
                if (r.p != prv.p) sbt.Append(r.p.ToString()); sbt.Append(',');
                if (r.l != prv.l) sbt.Append(r.l.ToString()); sbt.Append(',');
                if (r.t != prv.t) sbt.Append(r.t.ToString()); sbt.Append(',');
                if (r.m != prv.m) sbt.Append(r.m.ToString()); sbt.Append(',');
                if (r.elapsed != prv.elapsed) sbt.Append(r.elapsed ? '1' : '0'); sbt.Append(',');
                if (r.changed != prv.changed) sbt.Append(r.changed ? '1' : '0'); sbt.Append(',');
                if (r.v != prv.v) {
                    sbt.Append(r.v.ToString("F2", CultureInfo.InvariantCulture));
                }
                sbt.Append((char)0x0a);
                streamWriter.Write(sbt.AsSpan(false));
                //Console.WriteLine(sbt.ToString());
                prv = r;
            }
            streamWriter.Flush();
        };
        result = memoryStream.ToArray();

    }

}