using SiteAdminUtils.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        private static readonly string _filePath = ConfigurationManager.AppSettings["LogFilePath"];
        private static object _syncRoot = new object();


        public static void TestSpeedParallelForConcurrencyBag()
        {
            var allLines = File.ReadAllLines(_filePath);
            int linesToProcess = allLines.Length;
            var parsedLogEntries = new ConcurrentBag<ApacheLogEntry>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, linesToProcess, i => parsedLogEntries.Add(ApacheLogAnalyser.GetLogEntryFromString(allLines[i])));

            sw.Stop();
            Console.WriteLine("TestSpeedParallelForConcurrencyBag() Time: {0} ms, Count: {1}", sw.Elapsed.TotalMilliseconds, parsedLogEntries.Count);
        }

        public static void TestSpeedParallelForList()
        {
            var allLines = File.ReadAllLines(_filePath);
            int linesToProcess = allLines.Length;
            var parsedLogEntries = new List<ApacheLogEntry>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.For(0, linesToProcess, i =>
            {
                lock (_syncRoot)
                {
                    parsedLogEntries.Add(ApacheLogAnalyser.GetLogEntryFromString(allLines[i]));
                }
            });

            sw.Stop();
            Console.WriteLine("TestSpeedParallelForList() Time: {0} ms, Count: {1}", sw.Elapsed.TotalMilliseconds, parsedLogEntries.Count);
        }

        public static void TestSpeedForeachList()
        {
            var allLines = File.ReadAllLines(_filePath);
            var parsedLogEntries = new List<ApacheLogEntry>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var line in allLines)
            {
                parsedLogEntries.Add(ApacheLogAnalyser.GetLogEntryFromString(line));
            }


            sw.Stop();
            Console.WriteLine("TestSpeedForeachList() Time: {0} ms, Count: {1}", sw.Elapsed.TotalMilliseconds, parsedLogEntries.Count);
        }


        static void Main(string[] args)
        {
            string filePath = ConfigurationManager.AppSettings["LogFilePath"];

            TestSpeedForeachList();
            TestSpeedParallelForList();
            TestSpeedParallelForConcurrencyBag();

            Console.ReadKey();
        }
    }
}
