namespace System.Collections.Generic
{
#if NET45_OR_GREATER
#elif NETSTANDARD1_3
#elif NETSTANDARD2_0
#elif NET40
    /// <summary> Polyfill for a minimal Extension of <see cref="IEnumerable{T}"/> </summary>
    public interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        /// <summary> The Number of Elements in this Collection </summary>
        int Count { get; }
    }

    /// <summary>Represents a <see cref="IReadOnlyCollection{T}"/> of elements that can be accessed by index.</summary>
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        T this[int index] { get; }
    }
#else
    /// <summary> Polyfill for a minimal Extension of <see cref="IEnumerable{T}"/> </summary>
    public interface IReadOnlyCollection<T> : IEnumerable<T>
    {
        /// <summary> The Number of Elements in this Collection </summary>
        int Count { get; }
    }

    /// <summary>Represents a <see cref="IReadOnlyCollection{T}"/> of elements that can be accessed by index.</summary>
    public interface IReadOnlyList<T> : IReadOnlyCollection<T>
    {
        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        T this[int index] { get; }
    }
#endif //NET40
}
