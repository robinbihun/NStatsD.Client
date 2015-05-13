using System;
using System.Diagnostics;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            //If you want to set the Host/Port in code, use this.
            //var cs = new StatsDConfigurationSection();
            //cs.Server.Host = "devlexicesnu003.mycompany.svc";
            //cs.Server.Port = 8125;
            //NStatsD.Client.Current.Config = cs;

            NStatsD.Client.Current.Increment("test.increment");
            NStatsD.Client.Current.Decrement("test.decrement");
            NStatsD.Client.Current.Timing("test.increment", timer.ElapsedMilliseconds);
            NStatsD.Client.Current.Gauge("test.gauge", 25);

            Console.Read();
        }
    }
}
