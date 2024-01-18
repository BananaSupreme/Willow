using BenchmarkDotNet.Running;

using Benchmarks;

BenchmarkRunner.Run<VadBenchmarks>();
BenchmarkRunner.Run<CommandBenchmarks>();
BenchmarkRunner.Run<WhisperBenchmarks>();
