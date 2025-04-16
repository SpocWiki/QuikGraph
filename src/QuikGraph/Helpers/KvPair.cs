using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuikGraph.Helpers
{
    /// <inheritdoc cref="Create{TK,TV}"/>
    public static class KvPair
    {
        /// <summary> Creates a new <see cref="KeyValuePair{TK, TV}"/> instance. </summary>
        public static KeyValuePair<TK, TV> Create<TK, TV>(TK key, TV value) => new KeyValuePair<TK, TV>(key, value);

        /// <summary> Writes a list of items to the <paramref name="writer"/> in a JSON-like format. </summary>
        public static void WriteList<T>(this IEnumerable<T> items, TextWriter writer, string prefix = "{\"", string infix = "\",\"", string suffix = "\"}", string empty = "{}")
        {
            WriteList(items.GetEnumerator(), writer, prefix, infix, suffix, empty);
        }

        /// <summary> Writes a Dictionary of items to the <paramref name="writer"/> in C# format. </summary>
        /// <remarks>
        /// </remarks>
        public static void WriteDict<TK, TV>(this IEnumerable<KeyValuePair<TK,TV>> items, TextWriter writer
            , string prefix = "{\"" //, string infix1 = "\", new Dictionary<string, int> { { \""
            , string infixPair = "\", "
            , string infixDict = " }, { \"", string suffix = "}", string empty = "{}")
        {
            WriteDict(items.GetEnumerator(), writer, prefix, infixPair, infixDict, suffix, empty);
        }

        /// <summary> Compares if both dictionaries are equal </summary>
        public static bool IsEqualTo<TKey, TValue>(
            this IDictionary<TKey, TValue> d1,
            IDictionary<TKey, TValue> d2) => d1.Count == d2.Count && !d1.Except(d2).Any(); //comp

        private static readonly Dictionary<string, Dictionary<string, int>> GraphRoots = new Dictionary<string, Dictionary<string, int>>()
        {
            { "Empty", new Dictionary<string, int> { } },
};
        /// <summary> Writes a Dictionary of items to the <paramref name="writer"/> in C# format. </summary>
/// <remarks>             { "graph", new Dictionary {string, int} { { "node", 4 }, { "node", 4 }, } }, </remarks>
public static void WriteDict<TK, TV>(this IEnumerator<KeyValuePair<TK, TV>> items, TextWriter writer
            , string prefix = "{\"" //, string infix1 = "\", new Dictionary<string, int> { { \""
            , string infixPair = "\", "
            , string infixDict = " }, { \"", string suffix = "}", string empty = "{}")
        {
            if (items.MoveNext())
            {
                writer.Write(prefix);
                writer.Write((items.Current.Key + "").Replace(@"\", @"\\").Replace("\"", "\"\""));
                writer.Write(infixPair);
                writer.Write(items.Current.Value);
                while (items.MoveNext())
                {
                    writer.Write(infixDict);
                    writer.Write((items.Current.Key + "").Replace(@"\", @"\\").Replace("\"", "\"\""));
                    writer.Write(infixPair);
                    writer.Write(items.Current.Value);
                }
                writer.Write(suffix);
            }
            else
            {
                writer.Write(empty);
            }
        }

        /// <summary> Writes a list of items to the <paramref name="writer"/> in a JSON-like format. </summary>
        public static void WriteList<T>(this IEnumerator<T> items, TextWriter writer, string prefix = "{\"", string infix = "\",\"", string suffix = "\"}", string empty = "{}")
        {
            if (items.MoveNext())
            {
                writer.Write(prefix);
                writer.Write(items.Current);
                while (items.MoveNext())
                {
                    writer.Write(infix);
                    writer.Write((items.Current + "").Replace(@"\", @"\\").Replace("\"", "\"\""));
                }
                writer.Write(suffix);
            }
            else
            {
                writer.Write(empty);
            }
        }

    }
}
