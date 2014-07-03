﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeoid.Proj4ComparisonTests
{
    class Program
    {
        static void Main(string[] args) {
            using (var file = File.Open("results.csv", FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            using(var writer = new StreamWriter(file))
            {
                var runner = new TestRunner();
                foreach (var testCase in runner.CreateTestCases()) {

                    Console.WriteLine(testCase.ToString());
                    var testResult = runner.Execute(testCase);
                    var statsData = testResult.StatsData;
                    if (statsData != null) {
                        writer.Write(testCase.Source.ToString() + "," + testCase.Target.ToString());
                        writer.Write(",{0},{1},{2}", statsData.AvgErrorRatio, statsData.MinErrorRatio, statsData.MaxErrorRatio);
                        writer.Write(",{0},{1},{2}", statsData.AvgDistance,statsData.MinDistance, statsData.MaxDistance);
                        writer.WriteLine();
                    }
                }
            }

        }
    }
}
