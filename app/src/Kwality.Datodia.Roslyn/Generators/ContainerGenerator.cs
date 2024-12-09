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

using Kwality.Datodia.Roslyn.Code.Definitions;
using Kwality.Datodia.Roslyn.Extensions.Symbols;
using Kwality.Datodia.Roslyn.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class ContainerGenerator : IIncrementalGenerator
{
    private const string markerAttribute = "TypeBuilderAttribute";
    private const string typeBuilderNamespace = "Kwality.Datodia.Builders";
    private const string typeBuilderInterfaceNamespace = $"{typeBuilderNamespace}.Abstractions";
    private const string typeBuilderInterfaceName = "ITypeBuilder";
    private const string typeBuilderInterfaceFqName = $"{typeBuilderInterfaceNamespace}.{typeBuilderInterfaceName}<T>";
    private const string systemTypeBuildersNamespace = $"{typeBuilderNamespace}.System";

    private const string containerSource = """
                                           namespace Kwality.Datodia;

                                           using System;
                                           using System.Collections.Generic;

                                           using Kwality.Datodia.Abstractions;
                                           using Kwality.Datodia.Exceptions;

                                           /// <summary>
                                           ///     Container used to create objects.
                                           /// </summary>
                                           public sealed class Container : IContainer
                                           {
                                           [[#[[TYPE_BUILDERS_INSTANCES]]#]]
                                           [[#[[TYPE_BUILDERS_MAP]]#]]
                                               /// <summary>
                                               ///     The total number of elements to create when using <see cref="CreateMany{T}" />.
                                               /// </summary>
                                               /// <remarks>Defaults to 3.</remarks>
                                               public int RepeatCount{ get; set; } = 3;
                                           
                                               /// <inheritdoc />
                                               public T Create<T>()
                                               {
                                                   if (this.typeBuilders.TryGetValue(typeof(T), out var builder)) return (T)builder(this);
                                           
                                                   throw new DatodiaException($"No resolver registered for type '{typeof(T)}'.");
                                               }
                                           
                                               /// <inheritdoc />
                                               public IEnumerable<T> CreateMany<T>()
                                               {
                                                   for (var i = 0; i < this.RepeatCount; i++) yield return this.Create<T>();
                                               }
                                           
                                               /// <summary>
                                               ///     Register a custom builder that replaces the built-in builder for T.
                                               /// </summary>
                                               /// <param name="builder">The builder used to create instances of T.</param>
                                               /// <typeparam name="T">The type the builder can create.</typeparam>
                                               public void Register<T>(Func<IContainer, object> builder)
                                               {
                                                   this.typeBuilders[typeof(T)] = builder;
                                               }
                                           }
                                           """;

    private readonly TypeBuilderDefinition[] systemTypeBuilders =
    [
        new("string", "StringTypeBuilder", systemTypeBuildersNamespace),
        new("System.Guid", "GuidTypeBuilder", systemTypeBuildersNamespace),
        new("bool", "BoolTypeBuilder", systemTypeBuildersNamespace),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        AddTypeBuilderMarkerAttribute(context);
        var typeBuildersProvider = GetTypeBuildersProvider(context);
        var recordsProvider = GetRecordsProvider(context);

        var combinedProvider = typeBuildersProvider.Collect().Combine(recordsProvider.Collect())
                                                   .SelectMany((tuple, _) => tuple.Left.Concat(tuple.Right)).Collect();

        context.RegisterSourceOutput(recordsProvider, GenerateTypeBuilder);
        context.RegisterSourceOutput(combinedProvider, GenerateContainer);

        void GenerateContainer(SourceProductionContext ctx, ImmutableArray<TypeBuilderDefinition?> typeBuilders)
        {
            var allTypeBuilders = this.systemTypeBuilders.Concat(typeBuilders).ToImmutableArray();
            var typeBuildersInstance = CreateTypeBuilderInstances(allTypeBuilders);
            var typeBuilderMap = CreateTypeBuilderMap(allTypeBuilders);

            var generatedContainer = containerSource.Replace("[[#[[TYPE_BUILDERS_INSTANCES]]#]]", typeBuildersInstance)
                                                    .Replace("[[#[[TYPE_BUILDERS_MAP]]#]]", typeBuilderMap);

            ctx.AddSource("Container.g.cs", SourceText.From(generatedContainer, Encoding.UTF8));
        }

        void GenerateTypeBuilder(SourceProductionContext ctx, TypeBuilderDefinition? definition)
        {
            if (definition == null)
            {
                return;
            }

            // @formatter:off
            var typeBuilderSource = $$"""
                                      namespace {{definition.Namespace}};
                                      
                                      using global::Kwality.Datodia.Abstractions;
                                      
                                      [global::TypeBuilder]
                                      public sealed class {{definition.BuilderName}} : global::{{typeBuilderInterfaceNamespace}}.{{typeBuilderInterfaceName}}<global::{{definition.FullTypeName}}>
                                      {
                                          /// <inheritdoc />
                                          public object Create(IContainer container)
                                          {
                                              return new global::{{definition.FullTypeName}}();
                                          }
                                      }
                                      """;
            // @formatter:on

            ctx.AddSource($"{definition.Namespace}.{definition.BuilderName}.g.cs",
                          SourceText.From(typeBuilderSource, Encoding.UTF8));
        }
    }

    private static void AddTypeBuilderMarkerAttribute(IncrementalGeneratorInitializationContext context)
    {
        var attribute = Attribute.Public(markerAttribute, AttributeTargets.Class);
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(attribute.HintName, attribute.ToString()));
    }

    private static IncrementalValuesProvider<TypeBuilderDefinition?> GetTypeBuildersProvider(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(markerAttribute, Predicate, Transformation)
                      .WithTrackingName("Provider.InitialExtraction").Where(builder => builder is not null)
                      .WithTrackingName("Provider.NotNull");

        bool Predicate(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax;
        }

        TypeBuilderDefinition? Transformation(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            var @interface = classSymbol.GetInterface(typeBuilderInterfaceFqName);

            if (@interface == null)
            {
                return null;
            }

            var displayName = @interface.TypeArguments.First().ToDisplayString();
            var symbolNamespace = classSymbol.GetFullNamespace();
            var @namespace = string.IsNullOrEmpty(symbolNamespace) ? string.Empty : symbolNamespace;

            return new(displayName, classSymbol.Name, @namespace);
        }
    }

    private static IncrementalValuesProvider<TypeBuilderDefinition?> GetRecordsProvider(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transformation)
                      .WithTrackingName("Records.InitialExtraction").Where(builder => builder is not null)
                      .WithTrackingName("Records.NotNull");

        bool Predicate(SyntaxNode node, CancellationToken _)
        {
            return node is RecordDeclarationSyntax;
        }

        TypeBuilderDefinition? Transformation(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            const string containingNamespace = $"{typeBuilderNamespace}.Generated";
            cancellationToken.ThrowIfCancellationRequested();
            var recordDeclarationSymbol = (RecordDeclarationSyntax)ctx.Node;

            if (ctx.SemanticModel.GetDeclaredSymbol(recordDeclarationSymbol, cancellationToken) is not INamedTypeSymbol
                symbol)
            {
                return null;
            }

            var fullTypeName = symbol.ToDisplayString();
            var symbolNamespace = symbol.GetFullNamespace();

            var @namespace = string.IsNullOrEmpty(symbolNamespace) ? containingNamespace
                                 : $"{containingNamespace}.{symbolNamespace}";

            return new(fullTypeName, $"{symbol.Name}TypeBuilder", @namespace);
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
        _ = sBuilder.AppendLine("    private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()");
        _ = sBuilder.AppendLine("    {");

        foreach (var definition in Enumerable.OfType<TypeBuilderDefinition>(typeBuilderDefinitions))
        {
            _ = sBuilder.AppendLine(definition.FormatAsMapEntry());
        }

        _ = sBuilder.AppendLine("    };");

        return sBuilder.ToString();
    }
}
