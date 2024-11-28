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
namespace Kwality.Datodia;

using Kwality.Datodia.Exceptions;

/// <summary>
///     Container used to create objects.
/// </summary>
public sealed class Container
{
    private static readonly Lazy<Container> instance = new(() => new());

    private readonly Dictionary<Type, Func<object>> typeBuilders = new()
    {
        { typeof(string), () => Guid.NewGuid().ToString() },
    };

    /// <summary>
    ///     Returns the Container instance used to create objects.
    /// </summary>
    public static Container Instance => instance.Value;

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
