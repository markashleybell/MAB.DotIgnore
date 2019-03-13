using System;
using System.Collections.Generic;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MAB.DotIgnore;
using testdata;

namespace benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var runMode = TestData.ParseRunMode(args[0]);

            var jobs = new[] {
                new Job(runMode).With(Runtime.Clr).WithId("v3.0.0 CLR"),
                new Job(runMode).With(Runtime.Core).WithId("v3.0.0 Core")
            };

            var config = DefaultConfig.Instance.With(jobs);

            BenchmarkRunner.Run<MatchBenchmark>(config);
        }
    }

    [MemoryDiagnoser]
    public class MatchBenchmark
    {
        private string[] _fileList;
        private IgnoreList _ignoreList;

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine("SETUP: " + Assembly.GetAssembly(typeof(IgnoreList)).FullName);
            Console.WriteLine();

            // This gives us an array of file paths
            _fileList = TestData.Files;
            _ignoreList = new IgnoreList(TestData.Patterns);
        }

        [Benchmark]
        public int Ignore()
        {
            var results = new List<bool>();

            foreach (var f in _fileList)
            {
                var isIgnored = _ignoreList.IsIgnored(f, pathIsDirectory: false);

                results.Add(isIgnored);
            }

            return results.Count;
        }
    }
}
