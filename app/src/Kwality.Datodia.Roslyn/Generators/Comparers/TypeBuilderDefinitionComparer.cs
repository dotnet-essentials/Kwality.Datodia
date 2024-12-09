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
namespace Kwality.Datodia.Roslyn.Generators.Comparers;

using Kwality.Datodia.Roslyn.Generators.Models;

internal sealed class TypeBuilderDefinitionComparer : IEqualityComparer<TypeBuilderDefinition?>
{
    public bool Equals(TypeBuilderDefinition? x, TypeBuilderDefinition? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.FullTypeName == y.FullTypeName && x.BuilderName == y.BuilderName && x.Namespace == y.Namespace
            && x.Parameters.SequenceEqual(y.Parameters);
    }

    public int GetHashCode(TypeBuilderDefinition? obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (obj?.FullTypeName.GetHashCode() ?? 0);
            hash = hash * 31 + (obj?.BuilderName.GetHashCode() ?? 0);
            hash = hash * 31 + (obj?.Namespace?.GetHashCode() ?? 0);

            return Enumerable.Aggregate(obj?.Parameters ?? [], hash,
                                        (current, parameter) => current * 31 + (parameter.Type?.GetHashCode() ?? 0));
        }
    }
}
