using System.Collections.Generic;
using System.IO;

namespace QuikGraph.Helpers
{
    /// <inheritdoc cref="Create{TK,TV}"/>
    public static class KVPair
    {
        /// <summary> Creates a new <see cref="KeyValuePair{TK, TV}"/> instance. </summary>
        public static KeyValuePair<TK, TV> Create<TK, TV>(TK key, TV value) => new KeyValuePair<TK, TV>(key, value);

        /// <summary> Writes a list of items to the <paramref name="writer"/> in a JSON-like format. </summary>
        public static void WriteList<T>(this IEnumerable<T> items, TextWriter writer, string prefix = "{\"", string infix = "\",\"", string suffix = "\"}", string empty = "{}")
        {
            WriteList(items.GetEnumerator(), writer, prefix, infix, suffix, empty);
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
