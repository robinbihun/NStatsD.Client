using System;
using System.Diagnostics;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();

            NStatsD.Client.Current.Increment("test.increment");
            NStatsD.Client.Current.Decrement("test.decrement");
            NStatsD.Client.Current.Timing("test.increment", timer.ElapsedMilliseconds);
            NStatsD.Client.Current.Gauge("test.gauge", 25);

            Console.Read();
        }
    }
}
