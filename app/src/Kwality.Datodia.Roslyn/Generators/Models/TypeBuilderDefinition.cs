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
namespace Kwality.Datodia.Roslyn.Generators.Models;

internal sealed record TypeBuilderDefinition(string FullTypeName, string BuilderName, string Namespace)
{
    public string FullTypeName
    {
        get;
    } = FullTypeName;

    public string BuilderName
    {
        get;
    } = BuilderName;

    public string Namespace
    {
        get;
    } = Namespace;

    private string FqName => $"{this.Namespace}.{this.BuilderName}";
    private string InstanceName => $"{this.FqName.Replace(".", "_")}_Instance";

    public string FormatAsInstance()
    {
        return $"    private static readonly {this.FqName} {this.InstanceName} = new {this.FqName}();";
    }

    public string FormatAsMapEntry()
    {
        return $"        {{ typeof({this.FullTypeName}), {this.InstanceName}.Create }},";
    }
}
