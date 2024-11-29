// =====================================================================================================================
// == LICENSE:       Copyright (c) 2024 Kevin De Coninck
// ==
// ==                Permission is hereby granted, free of charge, to any person
// ==                obtaining a copy of this software and associated documentation
// ==                files (the "Software"), to deal in the Software without
// ==                restriction, including without limitation the rights to use,
// ==                copy, modify, merge, publish, distribute, sublicense, and/or sell
// ==                copies of the Software, and to permit persons to whom the
// ==                Software is furnished to do so, subject to the following
// ==                conditions:
// ==
// ==                The above copyright notice and this permission notice shall be
// ==                included in all copies or substantial portions of the Software.
// ==
// ==                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// ==                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// ==                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// ==                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// ==                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// ==                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// ==                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// ==                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable // Consider making public types internal
#pragma warning disable CA1050 // Declare types in namespaces
using AutoFixture;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Kwality.Datodia;

BenchmarkRunner.Run<DatodiaComparisonBenchmarks>();

[MemoryDiagnoser]
[RankColumn]
public class DatodiaComparisonBenchmarks
{
    private const int Iterations = 1_000_000;

    [Benchmark]
    public void CreateStringUsingAutoFixture()
    {
        var fixture = new Fixture();

        for (var i = 0; i < Iterations; i++)
        {
            _ = fixture.Create<string>();
        }
    }

    [Benchmark]
    public void CreateStringUsingDatodia()
    {
        var container = new Container();

        for (var i = 0; i < Iterations; i++)
        {
            _ = container.Create<string>();
        }
    }
}
