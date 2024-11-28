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
    private readonly Dictionary<Type, Func<object>> typeBuilders = new()
    {
        { typeof(string), () => Guid.NewGuid().ToString() },
    };

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
}
