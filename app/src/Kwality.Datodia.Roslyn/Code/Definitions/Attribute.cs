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
namespace Kwality.Datodia.Roslyn.Code.Definitions;

using Kwality.Datodia.Roslyn.Code.Enumerations;
using Kwality.Datodia.Roslyn.Code.Extensions.Definitions.Attributes;
using Kwality.Datodia.Roslyn.Code.Extensions.Enumerations;
using Kwality.Datodia.Roslyn.Helpers.Normalizers;

internal sealed record Attribute
{
    private readonly Accessibility accessibility;
    private readonly string attributeName;
    private readonly AttributeTargets targets;

    private Attribute(string name, AttributeTargets targets, Accessibility accessibility)
    {
        this.attributeName = AttributeNameNormalizer.Normalize(name);
        this.targets = targets;
        this.accessibility = accessibility;
    }

    public string HintName => $"Kwality.Datodia.{this.attributeName}.Attribute.g.cs";

    public static Attribute Public(string name, AttributeTargets targets)
    {
        return new(name, targets, Accessibility.Public);
    }

    public override string ToString()
    {
        var targetString = this.targets.ToUsageString();
        var accessibilityString = this.accessibility.ToDisplayString();

        return $$"""
                 [global::System.AttributeUsage({{targetString}})]
                 {{accessibilityString}} sealed class {{this.attributeName}}Attribute : System.Attribute
                 {
                     // NOTE: Intentionally left blank.
                     //       Used as a "marker" attribute.
                 }
                 """;
    }
}
