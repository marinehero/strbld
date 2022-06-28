// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System;

namespace System.Text
{
    internal static class ConvertZ {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int digits10(ushort v) {
            int result = 1;
            for (;;) {
                if (v < 10) return result;
                if (v < 100) return result + 1;
                if (v < 1000) return result + 2;
                if (v < 10000) return result + 3;
                // Skip ahead by 4 orders of magnitude
                v /= 10000;
                result += 4;
            }
        }

        private static readonly char[] digits = new char[] {
            '0','0','0','1','0','2','0','3','0','4','0','5','0','6','0','7','0','8','0','9',
            '1','0','1','1','1','2','1','3','1','4','1','5','1','6','1','7','1','8','1','9',
            '2','0','2','1','2','2','2','3','2','4','2','5','2','6','2','7','2','8','2','9',
            '3','0','3','1','3','2','3','3','3','4','3','5','3','6','3','7','3','8','3','9',
            '4','0','4','1','4','2','4','3','4','4','4','5','4','6','4','7','4','8','4','9',
            '5','0','5','1','5','2','5','3','5','4','5','5','5','6','5','7','5','8','5','9',
            '6','0','6','1','6','2','6','3','6','4','6','5','6','6','6','7','6','8','6','9',
            '7','0','7','1','7','2','7','3','7','4','7','5','7','6','7','7','7','8','7','9',
            '8','0','8','1','8','2','8','3','8','4','8','5','8','6','8','7','8','8','8','9',
            '9','0','9','1','9','2','9','3','9','4','9','5','9','6','9','7','9','8','9','9',
        };

        private static readonly string[] s_numbers 
        = (Enumerable.Range(0, ushort.MaxValue).Select(index => index.ToString()).ToArray());

        private static readonly string[] s_singleDigitStringCache = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToAsciiTableOld(ushort value) {

            int length = digits10(value);

            // For single-digit values that are very common, especially 0 and 1, just return cached strings.
            if (length == 1)
            {
                return s_singleDigitStringCache[value];
            }

            var dst = new ValueStringBuilder(stackalloc char[5]);

            int next = length - 1;

            while (value >= 100) {

                var i = (value % 100) * 2;

                value /= 100;

                dst[next] = digits[i + 1];

                dst[next - 1] = digits[i];

                next -= 2;

            }

            // Handle last 1-2 digits

            if (value < 10) {

                dst[next] = (char)(0x30 | value);

            } else {

                var i = (uint)value * 2;

                dst[next] = digits[i + 1];

                dst[next - 1] = digits[i];

            }

            return dst.AsSpan().ToString();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(ushort value) {

            return s_numbers[value];
            // int length = digits10(value);

            // // For single-digit values that are very common, especially 0 and 1, just return cached strings.
            // if (length == 1)
            // {
            //     return s_singleDigitStringCache[value];
            // }

            // Span<char> dst = stackalloc char[length];

            // int next = length - 1;

            // while (value >= 100) {

            //     /*
            //     (ushort quotient, ushort remainder) = Math.DivRem(value,(ushort)100);
            //     value = quotient;
            //     var i = remainder << 1;
            //     */

            //     var i = (value % 100) << 1;
            //     value /= 100;

            //     dst[next] = digits[i + 1];

            //     dst[next - 1] = digits[i];

            //     next -= 2;

            // }

            // // Handle last 1-2 digits

            // if (value < 10) {

            //     dst[next] = (char)(0x30 | value);

            // } else {

            //     var i = (uint)value << 1;

            //     dst[next] = digits[i + 1];

            //     dst[next - 1] = digits[i];

            // }

            // return new string(dst.Slice(0,length));

        }

    }
}