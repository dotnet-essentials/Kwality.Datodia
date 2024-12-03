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

    public void Verify()
    {
        // Arrange.
        var compilation = CreateCompilation();
        var compilationClone = compilation.Clone();
        var generator = new TGenerator().AsSourceGenerator();
        var options = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);

        // Act.
        var runResult1 = CSharpGeneratorDriver.Create([generator], driverOptions: options)
                                              .RunGeneratorsAndUpdateCompilation(compilation, out var result,
                                                   out var diagnostics).GetRunResult();

        var runResult2 = CSharpGeneratorDriver.Create([generator], driverOptions: options)
                                              .RunGenerators(compilationClone).GetRunResult();

        // Assert.
        _ = compilation.GetDiagnostics().Where(diagnostic => diagnostic.IsError()).Should().BeEmpty();
        _ = diagnostics.Should().BeEmpty();

        _ = result.SyntaxTrees.Select(x => x.ToString()).Should()
                  .HaveCount(compilation.SyntaxTrees.Count() + (this.ExpectedGeneratedSources?.Length ?? 0));

        _ = result.SyntaxTrees.Select(x => x.ToString()).Should().Contain(this.ExpectedGeneratedSources ?? []);
    }
}
