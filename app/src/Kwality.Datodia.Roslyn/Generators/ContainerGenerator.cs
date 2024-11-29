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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member - No API usage defined.
namespace Kwality.Datodia.Roslyn.Generators;

using System.Collections.Immutable;
using System.Text;

using Kwality.Datodia.Roslyn.Extensions.Symbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class ContainerGenerator : IIncrementalGenerator
{
    private const string typeBuilderFullName = "Kwality.Datodia.Builders.Abstractions.ITypeBuilder<T>";

    private const string containerSource = """
                                           namespace Kwality.Datodia;

                                           using System;
                                           using System.Collections.Generic;

                                           using Kwality.Datodia.Exceptions;

                                           /// <summary>
                                           ///     Container used to create objects.
                                           /// </summary>
                                           public sealed class Container
                                           {
                                           [[#[[TYPE_BUILDERS_INSTANCES]]#]]
                                           [[#[[TYPE_BUILDERS_MAP]]#]]
                                               /// <summary>
                                               ///     The total number of elements to create when using <see cref="CreateMany{T}" />.
                                               /// </summary>
                                               /// <remarks>Defaults to 3.</remarks>
                                               public int RepeatCount{ get; set; } = 3;
                                           
                                               /// <summary>
                                               ///     Create an instance of T.
                                               /// </summary>
                                               /// <typeparam name="T">The type to create.</typeparam>
                                               /// <returns>An instance of T.</returns>
                                               /// <exception cref="DatodiaException">An instance of T couldn't be created.</exception>
                                               public T Create<T>()
                                               {
                                                   if (this.typeBuilders.TryGetValue(typeof(T), out var builder)) return (T)builder();
                                           
                                                   throw new DatodiaException($"No resolver registered for type '{typeof(T)}'.");
                                               }
                                           
                                               /// <summary>
                                               ///     Create multiple instances of T.
                                               /// </summary>
                                               /// <typeparam name="T">The type to create.</typeparam>
                                               /// <returns>A collection of T elements.</returns>
                                               /// <exception cref="DatodiaException">An instance of T couldn't be created.</exception>
                                               public IEnumerable<T> CreateMany<T>()
                                               {
                                                   for (var i = 0; i < this.RepeatCount; i++) yield return this.Create<T>();
                                               }
                                           
                                               /// <summary>
                                               ///     Register a custom builder that replaces the built-in builder for T.
                                               /// </summary>
                                               /// <param name="builder">The builder used to create instances of T.</param>
                                               /// <typeparam name="T">The type the builder can create.</typeparam>
                                               public void Register<T>(Func<object> builder)
                                               {
                                                   this.typeBuilders[typeof(T)] = builder;
                                               }
                                           }
                                           """;

    private readonly TypeBuilderDefinition[] builtInTypeBuilders =
    [
        new("string", "Kwality.Datodia.Builders.StringTypeBuilder"),
        new("System.Guid", "Kwality.Datodia.Builders.GuidTypeBuilder"),
        new("bool", "Kwality.Datodia.Builders.BoolTypeBuilder"),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeBuilders = context.SyntaxProvider
                                  .CreateSyntaxProvider(TypeBuildersPredicate, TypeBuildersTransformation)
                                  .Where(typeBuilder => typeBuilder is not null);

        context.RegisterSourceOutput(typeBuilders.Collect(), GenerateContainerSource);

        bool TypeBuildersPredicate(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax { BaseList: not null };
        }

        TypeBuilderDefinition? TypeBuildersTransformation(GeneratorSyntaxContext ctx,
            CancellationToken cancellationToken)
        {
            var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as INamedTypeSymbol;
            var @interface = symbol?.GetInterface(typeBuilderFullName);

            return symbol != null && @interface != null
                       ? new(@interface.TypeArguments[0].ToDisplayString(), symbol.ToDisplayString()) : null;
        }

        void GenerateContainerSource(SourceProductionContext ctx,
            ImmutableArray<TypeBuilderDefinition?> typeBuilderDefinitions)
        {
            var allTypeBuilders = this.builtInTypeBuilders.Concat(typeBuilderDefinitions).ToImmutableArray();
            var typeBuildersInstance = CreateTypeBuilderInstances(allTypeBuilders);
            var typeBuilderMap = CreateTypeBuilderMap(allTypeBuilders);

            var generatedContainerSource = containerSource
                                          .Replace("[[#[[TYPE_BUILDERS_INSTANCES]]#]]", typeBuildersInstance)
                                          .Replace("[[#[[TYPE_BUILDERS_MAP]]#]]", typeBuilderMap);

            ctx.AddSource("Container.g.cs", SourceText.From(generatedContainerSource, Encoding.UTF8));
        }
    }

    private static string CreateTypeBuilderInstances(ImmutableArray<TypeBuilderDefinition?> typeBuilderDefinitions)
    {
        var sBuilder = new StringBuilder();

        foreach (var definition in Enumerable.OfType<TypeBuilderDefinition>(typeBuilderDefinitions))
        {
            _ = sBuilder.AppendLine(definition.FormatAsInstance());
        }

        return sBuilder.ToString();
    }

    private static string CreateTypeBuilderMap(ImmutableArray<TypeBuilderDefinition?> typeBuilderDefinitions)
    {
        var sBuilder = new StringBuilder();
        _ = sBuilder.AppendLine("    private readonly Dictionary<Type, Func<object>> typeBuilders = new()");
        _ = sBuilder.AppendLine("    {");

        foreach (var definition in Enumerable.OfType<TypeBuilderDefinition>(typeBuilderDefinitions))
        {
            _ = sBuilder.AppendLine(definition.FormatAsMapEntry());
        }

        _ = sBuilder.AppendLine("    };");

        return sBuilder.ToString();
    }

    private sealed record TypeBuilderDefinition(string Type, string FullName)
    {
        public string Type
        {
            get;
        } = Type;

        public string FullName
        {
            get;
        } = FullName;

        public string FormatAsInstance()
        {
            return $"    private static readonly {this.FullName} {this.FullName.Replace(".", "_")}_Instance = new {this.FullName}();";
        }

        public string FormatAsMapEntry()
        {
            return $"        {{ typeof({this.Type}), {this.FullName.Replace(".", "_")}_Instance.Create }},";
        }
    }
}
