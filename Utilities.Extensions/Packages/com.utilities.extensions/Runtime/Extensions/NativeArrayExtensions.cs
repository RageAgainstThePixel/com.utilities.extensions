// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Unity.Collections;

#if !UNITY_2022_2_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Utilities.Extensions
{
    public static class NativeArrayExtensions
    {
#if !UNITY_2022_2_OR_NEWER
        public static unsafe ReadOnlySpan<T> AsSpan<T>(this NativeArray<T> nativeArray) where T : unmanaged
            => new(nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
#endif
        public static unsafe NativeArray<byte> ToNativeArray(this MemoryStream stream, int? start = null, long? length = null, Allocator allocator = Allocator.Temp)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var startValue = start.GetValueOrDefault(0);

            if (startValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start must be >= 0.");
            }

            var lengthValue = length ?? stream.Length - startValue;

            switch (lengthValue)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException(nameof(length), "Length must be >= 0.");
                case 0:
                    return new NativeArray<byte>(0, allocator);
                case > int.MaxValue:
                    throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds maximum supported size.");
            }

            if (startValue + lengthValue > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Start + length exceeds the available data in MemoryStream.");
            }

            if (!stream.TryGetBuffer(out ArraySegment<byte> seg) || seg.Array == null)
            {
                throw new InvalidOperationException("MemoryStream internal buffer is not accessible.");
            }

            var srcOffset = seg.Offset + startValue;
            var segEnd = seg.Offset + seg.Count;

            if (srcOffset + lengthValue > segEnd)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Requested range extends beyond the accessible buffer segment.");
            }

            var bytes = (int)lengthValue;
            var native = new NativeArray<byte>(bytes, allocator);
            try
            {
                fixed (byte* srcPtr = &seg.Array[srcOffset])
                {
                    UnsafeUtility.MemCpy(native.GetUnsafePtr(), srcPtr, bytes);
                }

                return native;
            }
            catch
            {
                native.Dispose();
                throw;
            }
        }

        public static unsafe NativeArray<T> CopyFrom<T>(this NativeArray<T> dest, NativeArray<T> src, int start, int length) where T : unmanaged
        {
            if (start < 0 || length < 0 || start + length > src.Length || length > dest.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Invalid start or length for copying NativeArray.");
            }

            var destPtr = (T*)dest.GetUnsafePtr();
            var srcPtr = (T*)src.GetUnsafeReadOnlyPtr();
            UnsafeUtility.MemCpy(destPtr, srcPtr + start, length * UnsafeUtility.SizeOf<T>());
            return dest;
        }

        public static unsafe NativeArray<byte> FromBase64String(string input, Allocator allocator = Allocator.Temp)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var base64Length = 0;
            var paddingCount = 0;
            var paddingStarted = false;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (IsIgnorableWhitespace(c))
                {
                    continue;
                }

                var quartetPosition = base64Length % 4;

                if (c == '=')
                {
                    if (quartetPosition < 2)
                    {
                        throw new FormatException("Invalid padding character position in Base64 string.");
                    }

                    paddingCount++;
                    paddingStarted = true;

                    if (paddingCount > 2)
                    {
                        throw new FormatException("Invalid Base64 padding.");
                    }
                }
                else
                {
                    if (paddingStarted)
                    {
                        throw new FormatException("Unexpected character after padding in Base64 string.");
                    }

                    if (!TryGetBase64Value(c, out _))
                    {
                        throw new FormatException("Invalid character found in Base64 string.");
                    }
                }

                base64Length++;
            }

            if (base64Length == 0)
            {
                return new NativeArray<byte>(0, allocator);
            }

            if ((base64Length & 3) != 0)
            {
                throw new FormatException("Base64 string length must be a multiple of four.");
            }

            var outputLength = base64Length / 4 * 3 - paddingCount;
            var nativeArray = new NativeArray<byte>(outputLength, allocator);

            if (outputLength == 0)
            {
                return nativeArray;
            }

            try
            {
                Span<int> quartet = stackalloc int[4];
                var quartetIndex = 0;
                var outputIndex = 0;
                var paddingPhase = false;

                for (var i = 0; i < input.Length; i++)
                {
                    var c = input[i];

                    if (IsIgnorableWhitespace(c))
                    {
                        continue;
                    }

                    if (c == '=')
                    {
                        quartet[quartetIndex++] = -1;
                        paddingPhase = true;
                    }
                    else
                    {
                        if (paddingPhase)
                        {
                            throw new FormatException("Unexpected character after padding in Base64 string.");
                        }

                        if (!TryGetBase64Value(c, out var value))
                        {
                            throw new FormatException("Invalid character found in Base64 string.");
                        }

                        quartet[quartetIndex++] = value;
                    }

                    if (quartetIndex != 4)
                    {
                        continue;
                    }

                    var val0 = quartet[0];
                    var val1 = quartet[1];
                    var val2 = quartet[2];
                    var val3 = quartet[3];

                    if (val0 < 0 || val1 < 0)
                    {
                        throw new FormatException("Invalid Base64 quartet.");
                    }

                    var paddingInBlock = 0;

                    if (val3 < 0)
                    {
                        paddingInBlock++;
                        val3 = 0;
                    }

                    if (val2 < 0)
                    {
                        paddingInBlock++;

                        if (paddingInBlock == 1)
                        {
                            throw new FormatException("Invalid Base64 quartet padding order.");
                        }

                        val2 = 0;
                    }

                    var block = (uint)((val0 << 18) | (val1 << 12) | (val2 << 6) | val3);

                    nativeArray[outputIndex++] = (byte)((block >> 16) & 0xFF);

                    if (paddingInBlock < 2)
                    {
                        nativeArray[outputIndex++] = (byte)((block >> 8) & 0xFF);

                        if (paddingInBlock == 0)
                        {
                            nativeArray[outputIndex++] = (byte)(block & 0xFF);
                        }
                    }

                    quartetIndex = 0;
                }

                if (quartetIndex != 0)
                {
                    throw new FormatException("Incomplete Base64 quartet.");
                }

                if (outputIndex != outputLength)
                {
                    throw new FormatException("Decoded Base64 length mismatch.");
                }
            }
            catch
            {
                nativeArray.Dispose();
                throw;
            }

            return nativeArray;

            static bool TryGetBase64Value(char c, out int value)
            {
                if (c >= base64DecodeMap.Length)
                {
                    value = -1;
                    return false;
                }

                var mapped = base64DecodeMap[c];

                if (mapped < 0)
                {
                    value = -1;
                    return false;
                }

                value = mapped;
                return true;
            }

            static bool IsIgnorableWhitespace(char c) => char.IsWhiteSpace(c);
        }

        private static readonly sbyte[] base64DecodeMap = Base64DecodeMap();

        private static sbyte[] Base64DecodeMap()
        {
            var map = new sbyte[256];

            for (var i = 0; i < map.Length; i++)
            {
                map[i] = -1;
            }

            for (var i = 0; i < 26; i++)
            {
                map['A' + i] = (sbyte)i;
                map['a' + i] = (sbyte)(26 + i);
            }

            for (var i = 0; i < 10; i++)
            {
                map['0' + i] = (sbyte)(52 + i);
            }

            map['+'] = 62;
            map['/'] = 63;

            return map;
        }

        public static string ToBase64String(NativeArray<byte> nativeArray)
        {
            if (nativeArray.Length == 0)
            {
                return string.Empty;
            }

            var inputSpan = nativeArray.AsSpan();
            var outputLength = ((nativeArray.Length + 2) / 3) * 4;
            var outputChars = new NativeArray<char>(outputLength, Allocator.Temp);

            try
            {
                var inputIndex = 0;
                var outputIndex = 0;

                while (inputIndex < inputSpan.Length)
                {
                    var remaining = inputSpan.Length - inputIndex;
                    var hasSecondByte = remaining > 1;
                    var hasThirdByte = remaining > 2;

                    var byte0 = inputSpan[inputIndex++];
                    var byte1 = hasSecondByte ? inputSpan[inputIndex++] : (byte)0;
                    var byte2 = hasThirdByte ? inputSpan[inputIndex++] : (byte)0;
                    var block = (uint)((byte0 << 16) | (byte1 << 8) | byte2);

                    outputChars[outputIndex++] = GetBase64Char((block >> 18) & 0x3F);
                    outputChars[outputIndex++] = GetBase64Char((block >> 12) & 0x3F);
                    outputChars[outputIndex++] = hasSecondByte ? GetBase64Char((block >> 6) & 0x3F) : '=';
                    outputChars[outputIndex++] = hasThirdByte ? GetBase64Char(block & 0x3F) : '=';
                }

                return new string(outputChars.ToArray());
            }
            finally
            {
                outputChars.Dispose();
            }

            static char GetBase64Char(uint value)
            {
                return value switch
                {
                    < 26 => (char)('A' + value),
                    < 52 => (char)('a' + (value - 26)),
                    < 62 => (char)('0' + (value - 52)),
                    62 => '+',
                    _ => '/'
                };
            }
        }
    }
}
