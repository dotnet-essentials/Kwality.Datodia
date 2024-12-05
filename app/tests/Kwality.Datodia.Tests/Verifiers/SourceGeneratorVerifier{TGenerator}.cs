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
namespace Kwality.Datodia.Tests.Verifiers;

using FluentAssertions;

using Kwality.Datodia.Tests.Extensions;
using Kwality.Datodia.Tests.Verifiers.Abstractions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal sealed class SourceGeneratorVerifier<TGenerator> : RoslynComponentVerifier
    where TGenerator : IIncrementalGenerator, new()
{
    public string[]? ExpectedGeneratedSources
    {
        get;
        init;
    }

    public void Verify(string[]? trackingNames = null)
    {
        // Arrange.
        var compilation = CreateCompilation();
        var compilationClone = compilation.Clone();
        var generator = new TGenerator().AsSourceGenerator();
        var options = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);
        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options);

        // Act (Run 1).
        driver = driver.RunGenerators(compilation);
        var runResult1 = driver.GetRunResult();

        // Act (Run 2).
        driver = driver.RunGenerators(compilationClone);
        var runResult2 = driver.GetRunResult();

        // Assert.
        _ = runResult1.Diagnostics.Should().BeEmpty();

        _ = runResult2.Results[0].TrackedOutputSteps.SelectMany(x => x.Value).SelectMany(x => x.Outputs).Should()
                      .OnlyContain(x => x.Reason == IncrementalStepRunReason.Cached);

        if (trackingNames is null || trackingNames.Length == 0)
        {
            return;
        }

        _ = runResult1.Should().Match(runResult2, trackingNames);
    }
}
