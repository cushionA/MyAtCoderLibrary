using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
    /// <summary>
    /// 文字列系の拡張メソッドクラス。
    /// </summary>
    public static class StringExtension
    {

        /// <summary>
        /// 文字列の回文判定処理。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPalindromeVectorized(this string input)
        {
            ReadOnlySpan<char> span = input.AsSpan();

            int length = span.Length;
            if ( length <= 1 )
                return true;

            nint elementsToCheck = length / 2;
            nint offset = 0;

            ref char buf = ref Unsafe.AsRef(ref MemoryMarshal.GetReference(span));

            // Vector512が利用可能かつ十分な長さがある場合
            if ( Vector512.IsHardwareAccelerated && elementsToCheck >= Vector512<ushort>.Count )
            {
                while ( offset + Vector512<ushort>.Count <= elementsToCheck )
                {
                    // 先頭側からのVectorロード
                    ref ushort first = ref Unsafe.As<char, ushort>(ref Unsafe.Add(ref buf, offset));
                    Vector512<ushort> forwardVector = Vector512.LoadUnsafe(ref first);

                    // 末尾側からのVectorロード
                    nint lastOffset = length - offset - Vector512<ushort>.Count;
                    ref ushort last = ref Unsafe.As<char, ushort>(ref Unsafe.Add(ref buf, lastOffset));
                    Vector512<ushort> reverseVector = Vector512.LoadUnsafe(ref last);

                    // 末尾側のVectorを内部で反転
                    reverseVector = Vector512.Shuffle(reverseVector, Vector512.Create(
                        (ushort)31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16,
                        15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));

                    // 両者が一致するか確認
                    if ( !Vector512.EqualsAll(forwardVector, reverseVector) )
                        return false;

                    offset += Vector512<ushort>.Count;
                }
            }
            // Vector256 (AVX2) が利用可能かつ十分な長さがある場合
            else if ( Avx2.IsSupported && elementsToCheck >= Vector256<ushort>.Count )
            {
                Vector256<byte> reverseMask = Vector256.Create(
                    (byte)14, 15, 12, 13, 10, 11, 8, 9, 6, 7, 4, 5, 2, 3, 0, 1,
                    14, 15, 12, 13, 10, 11, 8, 9, 6, 7, 4, 5, 2, 3, 0, 1);

                while ( offset + Vector256<ushort>.Count <= elementsToCheck )
                {
                    // 先頭側からのVectorロード
                    ref byte first = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref buf, offset));
                    Vector256<byte> forwardVector = Vector256.LoadUnsafe(ref first);

                    // 末尾側からのVectorロード
                    nint lastOffset = length - offset - Vector256<ushort>.Count;
                    ref byte last = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref buf, lastOffset));
                    Vector256<byte> reverseVector = Vector256.LoadUnsafe(ref last);

                    // 末尾側のVectorをAVX2命令で反転
                    reverseVector = Avx2.Shuffle(reverseVector, reverseMask);
                    reverseVector = Avx2.Permute2x128(reverseVector, reverseVector, 0b00_01);

                    // 両者が一致するか確認
                    if ( !Vector256.EqualsAll(forwardVector, reverseVector) )
                        return false;

                    offset += Vector256<ushort>.Count;
                }
            }
            // Vector128が利用可能かつ十分な長さがある場合
            else if ( Vector128.IsHardwareAccelerated && elementsToCheck >= Vector128<ushort>.Count )
            {
                while ( offset + Vector128<ushort>.Count <= elementsToCheck )
                {
                    // 先頭側からのVectorロード
                    ref ushort first = ref Unsafe.As<char, ushort>(ref Unsafe.Add(ref buf, offset));
                    Vector128<ushort> forwardVector = Vector128.LoadUnsafe(ref first);

                    // 末尾側からのVectorロード
                    nint lastOffset = length - offset - Vector128<ushort>.Count;
                    ref ushort last = ref Unsafe.As<char, ushort>(ref Unsafe.Add(ref buf, lastOffset));
                    Vector128<ushort> reverseVector = Vector128.LoadUnsafe(ref last);

                    // 末尾側のVectorを内部で反転
                    reverseVector = Vector128.Shuffle(reverseVector, Vector128.Create(
                        (ushort)7, 6, 5, 4, 3, 2, 1, 0));

                    // 両者が一致するか確認
                    if ( !Vector128.EqualsAll(forwardVector, reverseVector) )
                        return false;

                    offset += Vector128<ushort>.Count;
                }
            }

            // Vectorからあふれた要素を比較
            int remaining = (int)(elementsToCheck - offset);

            if ( remaining > 1 )
            {
                ref char first = ref Unsafe.Add(ref buf, offset);
                ref char last = ref Unsafe.Add(ref buf, length - offset - 1);

                do
                {
                    if ( last != first )
                    {
                        return false;
                    }

                    first = ref Unsafe.Add(ref first, 1);
                    last = ref Unsafe.Subtract(ref last, 1);
                } while ( Unsafe.IsAddressLessThan(ref first, ref last) );
            }

            return true;
        }


    }
}
