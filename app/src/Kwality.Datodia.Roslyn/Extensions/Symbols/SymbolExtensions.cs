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
namespace Kwality.Datodia.Roslyn.Extensions.Symbols;

using Microsoft.CodeAnalysis;

internal static class SymbolExtensions
{
    public static string? GetFullNamespace(this ISymbol symbol)
    {
        var namespaceSymbol = symbol.ContainingNamespace;
        if (namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace)
        {
            // Return null when the namespace isn't known or global
            return null;
        }

        // Collect namespace parts
        var namespaceParts = new Stack<string>();
        while (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
        {
            namespaceParts.Push(namespaceSymbol.Name);
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        // Collect containing type parts (if nested)
        var typeParts = new Stack<string>();
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            typeParts.Push(containingType.Name);
            containingType = containingType.ContainingType;
        }

        // Combine namespace and type parts
        var fullNamespace = string.Join(".", namespaceParts);
        var nestedTypes = string.Join(".", typeParts);

        return string.IsNullOrEmpty(nestedTypes)
                   ? fullNamespace
                   : $"{fullNamespace}.{nestedTypes}";
    }
}
