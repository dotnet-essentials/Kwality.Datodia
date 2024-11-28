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
namespace Kwality.Datodia.Tests;

using Xunit;

public sealed partial class ContainerTests
{
    [Fact(DisplayName = "'Register<T>': A custom factory is registered for 'T'.")]
    internal void Register_custom_factory_uses_the_custom_factory()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        container.Register<string>(() => "Hello, World!");

        // ASSERT.
        Assert.Equal("Hello, World!", container.Create<string>());
    }

    [Fact(DisplayName = "'CreateMany<T>': Returns 3 elements by default.")]
    internal void Create_multiple_returns_3_elements_by_default()
    {
        // ARRANGE.
        var container = new Container();

        // ACT.
        var result = container.CreateMany<string>();

        // ASSERT.
        Assert.Equal(3, result.Count());
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
        Assert.Equal(10, result.Count());
    }
}
