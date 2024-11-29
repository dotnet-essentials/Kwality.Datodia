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
namespace Kwality.Datodia.Tests.Verifiers.Abstractions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal abstract class RoslynComponentVerifier
{
    public string[]? InputSources
    {
        get;
        init;
    }

    protected Compilation CreateCompilation()
    {
        var trustedPlatformAssemblies =
            AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")?.ToString()?.Split(Path.PathSeparator) ?? [];

        var references = trustedPlatformAssemblies.Select(p => MetadataReference.CreateFromFile(p)).ToList();

        // references.Add(MetadataReference
        //                   .CreateFromFile("/home/kevin-de-coninck/Development/github.com/kdeconinck/Kwality.Datodia/app/src/Kwality.Datodia/bin/Debug/netstandard2.0/Kwality.Datodia.dll"));

        return CSharpCompilation.Create("Kwality.Datodia",
                                        (this.InputSources ?? []).Select(x => CSharpSyntaxTree.ParseText(x)),
                                        references, new(OutputKind.DynamicallyLinkedLibrary));
    }
}
