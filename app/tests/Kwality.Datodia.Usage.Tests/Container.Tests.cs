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
namespace Kwality.Datodia.Usage.Tests;

using FluentAssertions;

using Xunit;

public sealed partial class ContainerTests
{
    [Fact(DisplayName = "'Create<T>': When 'T' is a 'record' an instance of the 'record' is returned.")]
    internal void Create_record_returns_a_record()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        var result = container.Create<Person>();

        // ASSERT.
        _ = result.Should().NotBeNull();
    }
    
    // @formatter:off
    [Fact(DisplayName = "'Create<T>': When 'T' is a 'record' with a primary constructor an instance of the 'record' is returned.")]
    // @formatter:on
    internal void Create_record_with_primary_constructor_returns_a_record()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        var c1 = container.Create<City>();
        var c2 = container.Create<City>();

        // ASSERT.
        _ = c1.Should().NotBe(c2);
    }
    
    // @formatter:off
    [Fact(DisplayName = "'Create<T>': When 'T' is a 'record' with a primary constructor and fields an instance of the 'record' is returned.")]
    // @formatter:on
    internal void Create_record_with_primary_constructor_and_fields_returns_a_record()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        var fn1 = container.Create<FullName>();
        var fn2 = container.Create<FullName>();

        // ASSERT.
        _ = fn1.Should().NotBe(fn2);
    }

    [Fact(DisplayName = "'Create<T>': An instance of a type created by a custom 'builder' is returned.")]
    internal void Create_type_defined_by_custom_builder()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        (var arg1, var arg2) = container.Create<(int, int)>();

        // ASSERT.
        _ = arg1.Should().NotBe(arg2);
    }

    [Fact(DisplayName = "'Register<T>': A custom builder is registered for 'T'.")]
    internal void Register_custom_builder_uses_the_custom_factory()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        container.Register<string>(_ => "Hello, World!");

        // ASSERT.
        _ = container.Create<string>().Should().BeEquivalentTo("Hello, World!");
    }

    [Fact(DisplayName = "'CreateMany<T>': Returns 3 elements by default.")]
    internal void Create_multiple_returns_3_elements_by_default()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        var result = container.CreateMany<string>();

        // ASSERT.
        _ = result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "'CreateMany<T>': Returns predefined amount of elements.")]
    internal void Create_multiple_returns_predefined_amount_of_elements()
    {
        // ARRANGE.
        var container = new Container
        {
            RepeatCount = 10,
        };

        // ACT.
        var result = container.CreateMany<string>();

        // ASSERT.
        _ = result.Should().HaveCount(10);
    }

    internal sealed record Person;
    internal sealed record City(int ZipCode, string Name);

    internal sealed record FullName(string FirstName, string LastName)
    {
        public string FirstName
        {
            get;
        } = FirstName;

        public string LastName
        {
            get;
        } = LastName;
    }
}
