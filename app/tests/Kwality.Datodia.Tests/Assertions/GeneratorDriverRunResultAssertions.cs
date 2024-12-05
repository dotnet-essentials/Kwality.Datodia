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
namespace Kwality.Datodia.Tests.Assertions;

using System.Collections.Immutable;

using FluentAssertions;
using FluentAssertions.Primitives;

using Microsoft.CodeAnalysis;

internal sealed class GeneratorDriverRunResultAssertions(GeneratorDriverRunResult subject)
    : ReferenceTypeAssertions<GeneratorDriverRunResult, GeneratorDriverRunResultAssertions>(subject)
{
    protected override string Identifier => "GeneratorDriverRunResult";

    [CustomAssertion]
    public AndConstraint<GeneratorDriverRunResultAssertions> Match(GeneratorDriverRunResult runResult,
        string[] trackingNames)
    {
        var run1TrackedSteps = GetTrackedSteps(Subject);
        var run2TrackedSteps = GetTrackedSteps(runResult);

        // Ensure that both runs have the same tracked steps.
        _ = run1TrackedSteps.Should().NotBeEmpty().And.HaveSameCount(run2TrackedSteps).And
                            .ContainKeys(run2TrackedSteps.Keys);

        // Loop over all tracked steps and ensure that both runs have the same number of run steps.
        for (var i = 0; i < run1TrackedSteps.Keys.Count; i++)
        {
            var runSteps1 = run1TrackedSteps.Values.ElementAt(i);
            var runSteps2 = run2TrackedSteps.Values.ElementAt(i);

            // Ensure that both tracked steps have the same number of run steps.
            _ = runSteps1.Should().HaveSameCount(runSteps2);

            // Loop over all run steps and ensure that both runs have the same number of outputs.
            for (var x = 0; x < runSteps1.Length; x++)
            {
                var outputs1 = runSteps1[x].Outputs.Select(o => o.Value);
                var outputs2 = runSteps2[x].Outputs.Select(o => o.Value);

                // Ensure that both run steps have the same number of outputs.
                _ = outputs1.Should().Equal(outputs2);

                // Ensure that the second output does only contain cached data.
                _ = runSteps2[x].Outputs.Should()
                                .OnlyContain(o => o.Reason == IncrementalStepRunReason.Cached
                                               || o.Reason == IncrementalStepRunReason.Unchanged);
            }
        }

        return new(this);

        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(GeneratorDriverRunResult result)
        {
            return result.Results[0].TrackedSteps.Where(step => trackingNames.Contains(step.Key))
                         .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
