using BenchmarkDotNet.Running;
using Tests;


var b = new Benchmarks();
Console.WriteLine(b.Calc1());
Console.WriteLine(b.Calc2());


//BenchmarkRunner.Run<Benchmarks>();