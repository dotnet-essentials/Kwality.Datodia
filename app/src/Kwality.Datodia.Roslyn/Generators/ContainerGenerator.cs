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

using Accessibility = Kwality.Datodia.Roslyn.Code.Enumerations.Accessibility;

[Generator]
public sealed class ContainerGenerator : IIncrementalGenerator
{
    private const string typeBuilderInterfaceNamespace = "Kwality.Datodia.Builders.Abstractions";
    private const string typeBuilderInterfaceName = "ITypeBuilder";
    private const string typeBuilderInterfaceFqName = $"{typeBuilderInterfaceNamespace}.{typeBuilderInterfaceName}<T>";
    private const string systemTypeBuildersNamespace = "Kwality.Datodia.Builders.System";
    private const string generatedTypeBuildersNamespace = "Kwality.Datodia.Builders.Generated";

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

    private readonly TypeBuilderDefinition[] systemTypeBuilders =
    [
        new("string", "StringTypeBuilder", systemTypeBuildersNamespace),
        new("System.Guid", "GuidTypeBuilder", systemTypeBuildersNamespace),
        new("bool", "BoolTypeBuilder", systemTypeBuildersNamespace),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        AddTypeBuilderMarkerAttribute(context);

        var typeBuildersProvider = context.SyntaxProvider
                                          .ForAttributeWithMetadataName("TypeBuilderAttribute", TypeBuildersPredicate,
                                                                        TypeBuildersTransformation)
                                          .WithTrackingName("InitialExtraction")
                                          .Where(typeBuilder => typeBuilder is not null).WithTrackingName("NotNull")
                                          .Collect();

        var recordsProvider = context.SyntaxProvider
                                     .CreateSyntaxProvider(RecordsPredicate, RecordsDeclarationSyntaxTransformation)
                                     .Where(typeBuilder => typeBuilder is not null);

        var flattenedProvider = typeBuildersProvider.Combine(recordsProvider.Collect())
                                                    .SelectMany((tuple, _) => tuple.Left.Concat(tuple.Right)).Collect();

        context.RegisterSourceOutput(recordsProvider, GenerateRecordTypeBuilder);
        context.RegisterSourceOutput(flattenedProvider, GenerateContainerSource);

        bool TypeBuildersPredicate(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
        }

        bool RecordsPredicate(SyntaxNode node, CancellationToken _)
        {
            return node is RecordDeclarationSyntax;
        }

        TypeBuilderDefinition? TypeBuildersTransformation(GeneratorAttributeSyntaxContext ctx,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            foreach (var attributeData in classSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.Name != "TypeBuilderAttribute"
                 || attributeData.AttributeClass.ToDisplayString() != "TypeBuilderAttribute")
                {
                }
            }

            var @interface = classSymbol.GetInterface(typeBuilderInterfaceFqName);

            if (@interface == null)
            {
                return null;
            }

            var symbolNamespace = classSymbol.GetFullNamespace();
            var typeBuilderNamespace = string.IsNullOrEmpty(symbolNamespace) ? string.Empty : symbolNamespace;

            return new(@interface.TypeArguments[0].ToDisplayString(), classSymbol.Name, typeBuilderNamespace);
        }

        TypeBuilderDefinition? RecordsDeclarationSyntaxTransformation(GeneratorSyntaxContext ctx,
            CancellationToken cancellationToken)
        {
            var recordDeclarationSymbol = (RecordDeclarationSyntax)ctx.Node;

            if (ctx.SemanticModel.GetDeclaredSymbol(recordDeclarationSymbol, cancellationToken) is not INamedTypeSymbol
                symbol)
            {
                return null;
            }

            var fullTypeName = symbol.ToDisplayString();
            var symbolNamespace = symbol.GetFullNamespace();

            var typeBuilderNamespace = string.IsNullOrEmpty(symbolNamespace) ? generatedTypeBuildersNamespace
                                           : $"{generatedTypeBuildersNamespace}.{symbolNamespace}";

            var typeBuilderName = $"{symbol.Name}TypeBuilder";

            return new(fullTypeName, typeBuilderName, typeBuilderNamespace);
        }

        void GenerateContainerSource(SourceProductionContext ctx,
            ImmutableArray<TypeBuilderDefinition?> typeBuilderDefinitions)
        {
            var allTypeBuilders = this.systemTypeBuilders.Concat(typeBuilderDefinitions).ToImmutableArray();
            var typeBuildersInstance = CreateTypeBuilderInstances(allTypeBuilders);
            var typeBuilderMap = CreateTypeBuilderMap(allTypeBuilders);

            var generatedContainerSource = containerSource
                                          .Replace("[[#[[TYPE_BUILDERS_INSTANCES]]#]]", typeBuildersInstance)
                                          .Replace("[[#[[TYPE_BUILDERS_MAP]]#]]", typeBuilderMap);

            ctx.AddSource("Container.g.cs", SourceText.From(generatedContainerSource, Encoding.UTF8));
        }

        void GenerateRecordTypeBuilder(SourceProductionContext ctx, TypeBuilderDefinition? definition)
        {
            if (definition == null)
            {
                return;
            }

            // @formatter:off
            var typeBuilderSource = $$"""
                                      namespace {{definition.Namespace}};
                                      
                                      [global::TypeBuilder]
                                      public sealed class {{definition.BuilderName}} : global::{{typeBuilderInterfaceNamespace}}.{{typeBuilderInterfaceName}}<global::{{definition.FullTypeName}}>
                                      {
                                          /// <inheritdoc />
                                          public object Create()
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
        Attribute markerAttribute = new("TypeBuilderAttribute", AttributeTargets.Class, Accessibility.Public);

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("Kwality.Datodia.TypeBuilder.Attribute.g.cs",
                                                                      markerAttribute.ToString()));
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
}
