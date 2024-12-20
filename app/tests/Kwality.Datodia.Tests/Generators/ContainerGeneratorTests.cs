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
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources = [],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify();
    }

    [Fact(DisplayName = "A marked 'ITypeBuilder<T>' is added to the container.")]
    internal void Marked_builders_are_added_to_the_container()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                [global::TypeBuilder]
                public sealed class FixedSquareTypeBuilder : Kwality.Datodia.Builders.Abstractions.ITypeBuilder<(int, int)>
                {
                    public object Create()
                    {
                        return (0, 0);
                    }
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly FixedSquareTypeBuilder FixedSquareTypeBuilder_Instance = new FixedSquareTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof((int, int)), FixedSquareTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Provider.InitialExtraction", "Provider.NotNull"]);
    }

    [Fact(DisplayName = "A marked (namespaced) 'ITypeBuilder<T>' is added to the container.")]
    internal void Marked_namespaced_builders_are_added_to_the_container()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                namespace Builders;

                [global::TypeBuilder]
                public sealed class FixedSquareTypeBuilder : Kwality.Datodia.Builders.Abstractions.ITypeBuilder<(int, int)>
                {
                    public object Create()
                    {
                        return (0, 0);
                    }
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Builders.FixedSquareTypeBuilder Builders_FixedSquareTypeBuilder_Instance = new Builders.FixedSquareTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof((int, int)), Builders_FixedSquareTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
            ],
        };

        // ARRANGE.
        sut.Verify(["Provider.InitialExtraction", "Provider.NotNull"]);
    }

    [Fact(DisplayName = "A marked (nested) 'ITypeBuilder<T>' is added to the container.")]
    internal void Marked_nested_builders_are_added_to_the_container()
    {
        // ACT & ASSERT.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                public sealed class Builders
                {
                    [global::TypeBuilder]
                    public sealed class FixedSquareTypeBuilder : Kwality.Datodia.Builders.Abstractions.ITypeBuilder<(int, int)>
                    {
                        public object Create()
                        {
                            return (0, 0);
                        }
                    }
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Builders.FixedSquareTypeBuilder Builders_FixedSquareTypeBuilder_Instance = new Builders.FixedSquareTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof((int, int)), Builders_FixedSquareTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Provider.InitialExtraction", "Provider.NotNull"]);
    }

    [Fact(DisplayName = "A marked (nested, namespaced) 'ITypeBuilder<T>' is added to the container.")]
    internal void Marked_nested_namespaced_builders_are_added_to_the_container()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                namespace Sample;

                public sealed class Builders
                {
                    [global::TypeBuilder]
                    public sealed class FixedSquareTypeBuilder : Kwality.Datodia.Builders.Abstractions.ITypeBuilder<(int, int)>
                    {
                        public object Create()
                        {
                            return (0, 0);
                        }
                    }
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Sample.Builders.FixedSquareTypeBuilder Sample_Builders_FixedSquareTypeBuilder_Instance = new Sample.Builders.FixedSquareTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof((int, int)), Sample_Builders_FixedSquareTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Provider.InitialExtraction", "Provider.NotNull"]);
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is generated for a 'record'.")]
    internal void Builder_is_generated_for_records()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources = ["public sealed record Person;"],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.Generated.PersonTypeBuilder Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.PersonTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof(Person), Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
                """
                namespace Kwality.Datodia.Builders.Generated;
                
                using global::Kwality.Datodia.Abstractions;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Person>
                {
                    /// <inheritdoc />
                    public object Create(IContainer container)
                    {
                        return new global::Person();
                    }
                }
                """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Records.InitialExtraction", "Records.NotNull"]);
    }
    
    [Fact(DisplayName = "An 'ITypeBuilder<T>' for a 'record' uses the 'container' for the fields.")]
    internal void Builder_for_records_invokes_the_container_for_the_fields()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources = ["public sealed record Person(string FirstName, string LastName);"],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.Generated.PersonTypeBuilder Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.PersonTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof(Person), Kwality_Datodia_Builders_Generated_PersonTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
                """
                namespace Kwality.Datodia.Builders.Generated;
                
                using global::Kwality.Datodia.Abstractions;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Person>
                {
                    /// <inheritdoc />
                    public object Create(IContainer container)
                    {
                        return new global::Person(container.Create<string>(), container.Create<string>());
                    }
                }
                """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Records.InitialExtraction", "Records.NotNull"]);
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is generated for a (namespaced) 'record'.")]
    internal void Builder_is_generated_for_namespaced_records()
    {
        // ARRANGE.
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
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.Generated.Samples.PersonTypeBuilder Kwality_Datodia_Builders_Generated_Samples_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.Samples.PersonTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof(Samples.Person), Kwality_Datodia_Builders_Generated_Samples_PersonTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
                """
                namespace Kwality.Datodia.Builders.Generated.Samples;

                using global::Kwality.Datodia.Abstractions;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Samples.Person>
                {
                    /// <inheritdoc />
                    public object Create(IContainer container)
                    {
                        return new global::Samples.Person();
                    }
                }
                """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Records.InitialExtraction", "Records.NotNull"]);
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is generated for a (nested) 'record'.")]
    internal void Builder_is_generated_for_nested_records()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                public sealed class Models
                {
                    public sealed record Person;
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.Generated.Models.PersonTypeBuilder Kwality_Datodia_Builders_Generated_Models_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.Models.PersonTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof(Models.Person), Kwality_Datodia_Builders_Generated_Models_PersonTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
                """
                namespace Kwality.Datodia.Builders.Generated.Models;

                using global::Kwality.Datodia.Abstractions;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Models.Person>
                {
                    /// <inheritdoc />
                    public object Create(IContainer container)
                    {
                        return new global::Models.Person();
                    }
                }
                """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Records.InitialExtraction", "Records.NotNull"]);
    }

    [Fact(DisplayName = "An 'ITypeBuilder<T>' is generated for a (nested, namespaced) 'record'.")]
    internal void Builder_is_generated_for_nested_namespaced_records()
    {
        // ARRANGE.
        var sut = new SourceGeneratorVerifier<ContainerGenerator>
        {
            InputSources =
            [
                """
                namespace Sample;

                public sealed class Models
                {
                    public sealed record Person;
                }
                """,
            ],
            ExpectedGeneratedSources =
            [
                markerAttributeSource, """
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
                                           private static readonly Kwality.Datodia.Builders.System.StringTypeBuilder Kwality_Datodia_Builders_System_StringTypeBuilder_Instance = new Kwality.Datodia.Builders.System.StringTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.GuidTypeBuilder Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance = new Kwality.Datodia.Builders.System.GuidTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.BoolTypeBuilder Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance = new Kwality.Datodia.Builders.System.BoolTypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.System.Int32TypeBuilder Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance = new Kwality.Datodia.Builders.System.Int32TypeBuilder();
                                           private static readonly Kwality.Datodia.Builders.Generated.Sample.Models.PersonTypeBuilder Kwality_Datodia_Builders_Generated_Sample_Models_PersonTypeBuilder_Instance = new Kwality.Datodia.Builders.Generated.Sample.Models.PersonTypeBuilder();
                                       
                                           private readonly Dictionary<Type, Func<IContainer, object>> typeBuilders = new()
                                           {
                                               { typeof(string), Kwality_Datodia_Builders_System_StringTypeBuilder_Instance.Create },
                                               { typeof(System.Guid), Kwality_Datodia_Builders_System_GuidTypeBuilder_Instance.Create },
                                               { typeof(bool), Kwality_Datodia_Builders_System_BoolTypeBuilder_Instance.Create },
                                               { typeof(int), Kwality_Datodia_Builders_System_Int32TypeBuilder_Instance.Create },
                                               { typeof(Sample.Models.Person), Kwality_Datodia_Builders_Generated_Sample_Models_PersonTypeBuilder_Instance.Create },
                                           };
                                       
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
                                       """,
                """
                namespace Kwality.Datodia.Builders.Generated.Sample.Models;

                using global::Kwality.Datodia.Abstractions;
                
                [global::TypeBuilder]
                public sealed class PersonTypeBuilder : global::Kwality.Datodia.Builders.Abstractions.ITypeBuilder<global::Sample.Models.Person>
                {
                    /// <inheritdoc />
                    public object Create(IContainer container)
                    {
                        return new global::Sample.Models.Person();
                    }
                }
                """,
            ],
        };

        // ACT & ASSERT.
        sut.Verify(["Records.InitialExtraction", "Records.NotNull"]);
    }
}
