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

using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

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

    public void Verify(string[] trackingNames)
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
        _ = diagnostics.Should().BeEmpty();

        _ = result.SyntaxTrees.Select(x => x.ToString()).Should()
                  .HaveCount(compilation.SyntaxTrees.Count() + (this.ExpectedGeneratedSources?.Length ?? 0));

        _ = result.SyntaxTrees.Select(x => x.ToString()).Should().Contain(this.ExpectedGeneratedSources ?? []);
    }

    private static void CompareResults(GeneratorDriverRunResult r1, GeneratorDriverRunResult r2, string[] trackingNames)
    {
        var trackedSteps1 = GetTrackedSteps(r1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(r2, trackingNames);
        trackedSteps1.Should().NotBeEmpty();
        trackedSteps1.Should().HaveSameCount(trackedSteps2).And.ContainKeys(trackedSteps2.Keys);

        foreach ((var trackingName, var runSteps1) in trackedSteps1)
        {
            var runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        return;

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(
            GeneratorDriverRunResult runResult, string[] trackingNames)
        {
            return runResult.Results[0].TrackedSteps.Where(step => trackingNames.Contains(step.Key))
                            .ToDictionary(x => x.Key, x => x.Value);
        }
    }

    private static void AssertEqual(ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2, string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(x => x.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(x => x.Value);
            outputs1.Should().Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used 
            runStep2.Outputs.Should()
                    .OnlyContain(x => x.Reason == IncrementalStepRunReason.Cached || x.Reason == IncrementalStepRunReason.Unchanged,
                                 $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {
                                     IncrementalStepRunReason.Unchanged}");

            // Make sure we're not using anything we shouldn't
            AssertObjectGraph(runStep1, stepName);
        }
    }

    private static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
    {
        // Including the stepName in error messages to make it easy to isolate issues
        var because = $"{stepName} shouldn't contain banned symbols";
        var visited = new HashSet<object>();

        // Check all of the outputs - probably overkill, but why not
        foreach ((var obj, _) in runStep.Outputs)
        {
            Visit(obj);
        }

        void Visit(object node)
        {
            // If we've already seen this object, or it's null, stop.
            if (node is null || !visited.Add(node))
            {
                return;
            }

            // Make sure it's not a banned type
            node.Should().NotBeOfType<Compilation>(because).And.NotBeOfType<ISymbol>(because).And
                .NotBeOfType<SyntaxNode>(because);

            // Examine the object
            var type = node.GetType();

            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                return;
            }

            // If the object is a collection, check each of the values
            if (node is IEnumerable collection and not string)
            {
                foreach (var element in collection)
                {
                    // recursively check each element in the collection
                    Visit(element);
                }

                return;
            }

            // Recursively check each field in the object
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldValue = field.GetValue(node);
                Visit(fieldValue);
            }
        }
    }
}
