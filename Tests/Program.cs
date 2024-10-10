using BenchmarkDotNet.Running;
using Tests;

/* This is a side project aimed at testing computational efficiency of some distance algorithm */

var b = new Benchmarks();
Console.WriteLine(b.DTW_Original());
Console.WriteLine(b.DTW_Alternative());


//BenchmarkRunner.Run<Benchmarks>();