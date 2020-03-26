using System.Collections;
using System.Collections.Generic;

namespace netDxf {

    partial class AciColor {

        /// <summary>
        /// Wraps <see cref="AciColor.LookUpRgb"/> function in <see cref="IReadOnlyDictionary{TKey,TValue}"/>
        /// of Autocad Color Indexes (ACI) palette. Keys are ACI color index,
        /// values are array of bytes, where red == byte[0], green == byte[1] and blue = byte[2].
        /// </summary>
        private class AciColorDictionary : IReadOnlyDictionary<byte, byte[]>
        {
            private struct Enumerator : IEnumerator<KeyValuePair<byte, byte[]>>
            {
                private byte index;

                public void Dispose() {}

                public bool MoveNext()
                {
                    if (this.index == byte.MaxValue)
                        return false;

                    this.index++;
                    return true;
                }

                public void Reset() { this.index = 0; }
                public KeyValuePair<byte, byte[]> Current
                {
                    get {
                        return this.index == 0
                            ? default(KeyValuePair<byte, byte[]>)
                            : new KeyValuePair<byte, byte[]>(this.index, RgbToArray(this.index));
                    }
                }

                object IEnumerator.Current
                {
                    get { return this.Current; }
                }

            }

            public IEnumerator<KeyValuePair<byte, byte[]>> GetEnumerator()
            {
                return new Enumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator();
            }

            public int Count
            {
                get { return byte.MaxValue; }
            }

            public bool ContainsKey(byte key) { return key != 0; }

            public bool TryGetValue(byte key, out byte[] value)
            {
                if (key == 0)
                {
                    value = null;
                    return false;
                }

                value = RgbToArray(key);
                return true;
            }

            private static byte[] RgbToArray(byte colorIndex)
            {
                if (colorIndex == 0)
                    throw new KeyNotFoundException("ACI index shold be greater then zero.");

                int rgb = LookUpRgb(colorIndex);

                return new[] {
                    (byte)(rgb >> 16),
                    (byte)(rgb >> 8),
                    (byte)rgb
                };
            }

            public byte[] this[byte key]
            {
                get { return RgbToArray(key); }
            }

            public IEnumerable<byte> Keys
            {
                get {
                    for (int i = 1; i <= byte.MaxValue; i++)
                        yield return (byte)i;
                }
            }

            public IEnumerable<byte[]> Values
            {
                get {
                    for (int i = 1; i <= byte.MaxValue; i++)
                        yield return this[(byte)i];
                }
            }

        }
    }

}
