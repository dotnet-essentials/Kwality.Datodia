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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member - Reason: NOT Public.
namespace Kwality.Datodia.Tests.Generators;

using Kwality.Datodia.Roslyn.Generators;
using Kwality.Datodia.Tests.Verifiers;

using Xunit;

public sealed class ContainerGeneratorTests
{
    private const string markerAttributeSource = """
                                                 [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                                                 public sealed class TypeBuilderAttribute : System.Attribute
                                                 {
                                                     // NOTE: Intentionally left blank.
                                                     //       Used as a "marker" attribute.
                                                 }
                                                 """;

    [Fact(DisplayName = "The 'Container' is added (always).")]
    internal void The_container_is_added()
    {
        // Arrange.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources = [],
            ExpectedGeneratedSources =
            [
                markerAttributeSource,
                """
                namespace Kwality.Datodia;

                using System;
                using System.Collections.Generic;

                using Kwality.Datodia.Exceptions;

                /// <summary>
                ///     Container used to create objects.
                /// </summary>
                public sealed class Container
                {
                    private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                
                    private readonly Dictionary<Type, Func<object>> typeBuilders = new()
                    {
                        { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                        { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                        { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                    };
                
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
                """,
            ],
        };

        // Act & assert.
        sut.Verify();
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is added for a 'record' in the 'global' namespace.")]
    internal void A_builder_is_generated_for_a_record_in_the_global_namespace()
    {
        // Arrange.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources = ["public sealed record Person;"],
            ExpectedGeneratedSources =
            [
                markerAttributeSource,
                """
                namespace Kwality.Datodia;

                using System;
                using System.Collections.Generic;

                using Kwality.Datodia.Exceptions;

                /// <summary>
                ///     Container used to create objects.
                /// </summary>
                public sealed class Container
                {
                    private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.Generated.PersonTypeBuilder Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.PersonTypeBuilder();
                
                    private readonly Dictionary<Type, Func<object>> typeBuilders = new()
                    {
                        { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                        { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                        { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                        { typeof(Person), Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance.Create },
                    };
                
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
                """,
                """
                namespace Kwality.Datodia.Builders.Generated;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Person>
                {
                    /// <inheritdoc />
                    public object Create()
                    {
                        return new global::Person();
                    }
                }
                """,
            ],
        };

        // Act & assert.
        sut.Verify();
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is added for a 'record' in a namespace.")]
    internal void A_builder_is_generated_for_a_record_in_a_namespace()
    {
        // Arrange.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                namespace Samples;
                
                public sealed record Person;
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource,
                """
                namespace Kwality.Datodia;

                using System;
                using System.Collections.Generic;

                using Kwality.Datodia.Exceptions;

                /// <summary>
                ///     Container used to create objects.
                /// </summary>
                public sealed class Container
                {
                    private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                    private static readonly Kwality.Datodia.Builders.Generated.Samples.PersonTypeBuilder Kwality_Datodia_Builders_Generated_Samples_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.Samples.PersonTypeBuilder();
                
                    private readonly Dictionary<Type, Func<object>> typeBuilders = new()
                    {
                        { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                        { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                        { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                        { typeof(Samples.Person), Kwality_Datodia_Builders_Generated_Samples_PersonTypeBuilder_Instance.Create },
                    };
                
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
                """,
                """
                namespace Kwality.Datodia.Builders.Generated.Samples;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Samples.Person>
                {
                    /// <inheritdoc />
                    public object Create()
                    {
                        return new global::Samples.Person();
                    }
                }
                """,
            ],
        };

        // Act & assert.
        sut.Verify();
    }
}
