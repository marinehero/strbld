// @nuget: BenchmarkDotNet -Version 0.12.1

using System.Text;
using System.Globalization;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Loggers;

using Microsoft.IO;

using Perfolizer.Horology;

// 
// Based on Steve Toub's excellent article on Spans<>
// https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay
//

// Plan to determine what best technique to use for compressing massive CSV file gen at webapi level
// Must scale, be fast and not utilize much memory

#if DEBUG
#else
[assembly: System.Diagnostics.Debuggable(isJITTrackingEnabled: false, isJITOptimizerDisabled: false)]
#endif

public class Program
{
    static void Main(string[] args)
    {

        new Benchmark().WriteOptimizedLessAllocNumRM();

        var x = ConvertZ.ToString(23);

        BenchmarkRunner.Run<Benchmark>(
            DefaultConfig.Instance
                .AddJob(Job.Default
                    .WithId("Job-400")
                    .WithToolchain(new InProcessEmitToolchain(
                        // timeout for .NET Fiddle
                        timeout: TimeSpan.FromSeconds(240), 
                        // Intermediate logging
                        logOutput: false)) 
                    .WithLaunchCount(1)
                    .WithWarmupCount(10)
                    .WithIterationCount(800)
                    .WithIterationTime(TimeInterval.FromMilliseconds(30))
                    )
                // Color codes are not supported in .NET Fiddle
                .AddLogger(new ConsoleLogger(unicodeSupport: true, ConsoleLogger.CreateGrayScheme()))
                .WithOptions(ConfigOptions.DisableLogFile));

    }
}

[MemoryDiagnoser]
// If you want the following option you must install R and XQuartz
// see: https://sparkbyexamples.com/r-language/install-r-language-and-rstudio-on-mac-os/
[RPlotExporter]
public class Benchmark
{

    /// <summary>
    /// Provides more convenient access to Microsoft's RecyclableMemoryStream methods.  https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream
    /// </summary>
    public static class RecyclableMemoryManager
    {
        static RecyclableMemoryStreamManager RecyclableStreamManager 
        // = new RecyclableMemoryStreamManager(512, 1024, 1024);
        = new RecyclableMemoryStreamManager(RecyclableMemoryStreamManager.DefaultBlockSize, RecyclableMemoryStreamManager.DefaultLargeBufferMultiple, 64*1024*1024);

        static RecyclableMemoryManager()
        {
            //RecyclableStreamManager.MaximumFreeLargePoolBytes = 400*1024*1024; // ~400mb
            //RecyclableStreamManager.MaximumFreeSmallPoolBytes = 200*1024*1024; // ~200mb
        }

        /// <summary>
        /// Gets an empty RecyclableMemoryStream from pool. Not necessarily an unallocated stream.
        /// </summary>
        /// <returns>RecyclableMemoryStream.</returns>
        public static MemoryStream GetStream()
        {
            return RecyclableStreamManager.GetStream();
        }


        /// <summary>
        /// Gets a RecyclableMemoryStream from the pool and bases it on data.
        /// </summary>
        /// <param name="data">Data to base stream on.</param>
        /// <param name="offset">Offset in array to start at.</param>
        /// <param name="length">Length to read from data into stream.</param>
        /// <param name="name">Name of stream in pool for later identification.</param>
        /// <returns>RecyclableMemoryStream containing data.</returns>
        public static MemoryStream GetStream(byte[] data, int offset = 0, int length = -1, string name = "")
        {
            return RecyclableStreamManager.GetStream(name, data, offset, length == -1 ? data.Length : length);
        }


        /// <summary>
        /// Gets a RecyclableMemoryStream of a given size from pool.
        /// </summary>
        /// <param name="requiredSize">Starting size stream must have.</param>
        /// <param name="name">Name of stream in pool for later identification.</param>
        /// <returns>RecyclableMemoryStream of at least given size.</returns>
        public static MemoryStream GetStream(int requiredSize, string name = "")
        {
            return RecyclableStreamManager.GetStream(name, requiredSize);
        }
    }

/*        private static readonly Lazy<RecyclableMemoryStreamManager> recyclableMemoryStreamManager 
            = new Lazy<RecyclableMemoryStreamManager>();
        private static RecyclableMemoryStreamManager RecyclableMemoryStreamManager
        {
            get
            {
                var recyclableMemoryStream = recyclableMemoryStreamManager.Value;
                //recyclableMemoryStream.MaximumFreeSmallPoolBytes = 64 * 1024 * 1024;
                //recyclableMemoryStream.MaximumFreeLargePoolBytes = 64 * 1024 * 32;
                // recyclableMemoryStream.AggressiveBufferReturn = true;
                recyclableMemoryStream.AggressiveBufferReturn = true;
                return recyclableMemoryStream;
            }
        }
        internal static MemoryStream GetStream()
        {
            return RecyclableMemoryStreamManager.GetStream(TagSource);
        }

        internal static MemoryStream GetStream(byte[] array)
        {
            return RecyclableMemoryStreamManager.GetStream(array);
        }

        internal static MemoryStream GetStream(int capacity)
        {
            return RecyclableMemoryStreamManager.GetStream(TagSource, capacity);
        }
        private const string TagSource = "James.Bench";
*/
	public class DimIntersectDto
    {
        public ushort p { get; set; }
        public ushort l { get; set; }
        public ushort t { get; set; }
        public ushort m { get; set; }
        public bool elapsed { get; set; }
        public bool changed { get; set; }
        public double v { get; set; }
        public DimIntersectDto() {
            /*p = (ushort)Random.Shared.NextInt64(ushort.MaxValue);
            l = (ushort)Random.Shared.NextInt64(ushort.MaxValue);
            t = (ushort)Random.Shared.NextInt64(ushort.MaxValue);*/
            m = (ushort)Random.Shared.Next(ushort.MaxValue);
            elapsed = true;
            v = (double)Random.Shared.NextDouble();
        }
    }
	
    const char COMMA = ',';
    const char ONE  = '1';
    const char ZERO = '0';
    const char LF = (char) 0x0a;
    const string F2 = "F2";
    const string HDR = "p,l,t,m,elapsed,changed,v";

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
    public void WriteOptimizedLessAllocNumRM()
    {
        var records = _records;
        byte[]? result = null;
        // await using (RecyclableMemoryStream memoryStream = new(s_manager.Value)) {
        using (var memoryStream = RecyclableMemoryManager.GetStream())
        using (var streamWriter = new StreamWriter(memoryStream))
        {
            streamWriter.WriteLine(HDR);
            var prv = new DimIntersectDto();
            prv.p ^= prv.p;
            var sbt = new ValueStringBuilder(stackalloc char[40]);
            foreach (var r in records)
            {
                sbt.Length = 0;
//#if !_TURTLE_
                if (r.p != prv.p) sbt.Append( ConvertZ.ToString(r.p)); sbt.Append(COMMA);
                if (r.l != prv.l) sbt.Append( ConvertZ.ToString(r.l)); sbt.Append(COMMA);
                if (r.t != prv.t) sbt.Append( ConvertZ.ToString(r.t)); sbt.Append(COMMA);
                if (r.m != prv.m) sbt.Append( ConvertZ.ToString(r.m)); sbt.Append(COMMA);
// #else
//                     if (r.p != prv.p) sbt.Append( Convert.ToString(r.p)); sbt.Append(',');
//                     if (r.l != prv.l) sbt.Append( Convert.ToString(r.l)); sbt.Append(',');
//                     if (r.t != prv.t) sbt.Append( Convert.ToString(r.t)); sbt.Append(',');
//                     if (r.m != prv.m) sbt.Append( Convert.ToString(r.m)); sbt.Append(',');
// #endif
                if (r.elapsed != prv.elapsed) sbt.Append(r.elapsed ? ONE : ZERO); sbt.Append(COMMA);
                if (r.changed != prv.changed) sbt.Append(r.changed ? ONE : ZERO); sbt.Append(COMMA);
                if (r.v != prv.v) {
                    sbt.Append(r.v.ToString(F2, CultureInfo.InvariantCulture));
                }
                sbt.Append(LF);
                streamWriter.Write(sbt.AsSpan());
                //Console.WriteLine(sbt.ToString());
                //Console.WriteLine(r.m.ToString());
                prv = r;
            }
            streamWriter.Flush();
            result = memoryStream.ToArray();
        }
    }

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
			var prv = new DimIntersectDto();
            prv.p ^= prv.p;
			foreach (var r in records) {
				sbt.Clear();
				// this averaged 7s vs 2.5 of using StringBuilder
				char elapsed = r.elapsed ? '1' : '0';
				char changed = r.changed ? '1' : '0';
				streamWriter.WriteLine($"{r.p},{r.l},{r.t},{r.m},{elapsed},{changed},{r.v:F2}");
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
			var prv = new DimIntersectDto();
            prv.p ^= prv.p;
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
			var prv = new DimIntersectDto();
            prv.p ^= prv.p;
            var sbt = new ValueStringBuilder(stackalloc char[40]);
            foreach (var r in records)
            {
                sbt.Length = 0;
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
                streamWriter.Write(sbt.AsSpan());
                //Console.WriteLine(sbt.ToString());
                prv = r;
            }
            streamWriter.Flush();
        };
        result = memoryStream.ToArray();

    }

    [Benchmark]
    public void WriteOptimizedLessAllocNum()
    {
        var records = _records;
        byte[]? result = null;
        using (var memoryStream = new MemoryStream()) {
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.WriteLine(HDR);
                var prv = new DimIntersectDto();
                prv.p ^= prv.p;
                var sbt = new ValueStringBuilder(stackalloc char[40]);
                foreach (var r in records)
                {
                    sbt.Length = 0;
    //#if !_TURTLE_
                    if (r.p != prv.p) sbt.Append( ConvertZ.ToString(r.p)); sbt.Append(COMMA);
                    if (r.l != prv.l) sbt.Append( ConvertZ.ToString(r.l)); sbt.Append(COMMA);
                    if (r.t != prv.t) sbt.Append( ConvertZ.ToString(r.t)); sbt.Append(COMMA);
                    if (r.m != prv.m) sbt.Append( ConvertZ.ToString(r.m)); sbt.Append(COMMA);
    // #else
    //                     if (r.p != prv.p) sbt.Append( Convert.ToString(r.p)); sbt.Append(',');
    //                     if (r.l != prv.l) sbt.Append( Convert.ToString(r.l)); sbt.Append(',');
    //                     if (r.t != prv.t) sbt.Append( Convert.ToString(r.t)); sbt.Append(',');
    //                     if (r.m != prv.m) sbt.Append( Convert.ToString(r.m)); sbt.Append(',');
    // #endif
                    if (r.elapsed != prv.elapsed) sbt.Append(r.elapsed ? ONE : ZERO); sbt.Append(COMMA);
                    if (r.changed != prv.changed) sbt.Append(r.changed ? ONE : ZERO); sbt.Append(COMMA);
                    if (r.v != prv.v) {
                        sbt.Append(r.v.ToString(F2, CultureInfo.InvariantCulture));
                    }
                    sbt.Append(LF);
                    streamWriter.Write(sbt.AsSpan());
                    //Console.WriteLine(sbt.ToString());
                    //Console.WriteLine(r.m.ToString());
                    prv = r;
                }
                streamWriter.Flush();
            };
            result = memoryStream.ToArray();
        }
    }

}