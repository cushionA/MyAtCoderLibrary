using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace MyAtCoderLibrary
{

    /// <summary>
    /// 競技プログラミング用のビット操作ライブラリ
    /// </summary>
    public static class BitLib
    {
        #region 基本ビット操作

        /// <summary>
        /// 整数内の1のビット数を返す（ポピュレーションカウント）<br></br>
        /// たとえば1001の場合は2が返ってくる。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(int x)
        {
            // ビルトイン関数。CPU命令を使用するため速い。
            return System.Numerics.BitOperations.PopCount((uint)x);
        }

        /// <summary>
        /// 最下位ビット（最も右の1のビット：Least Significant Bit）を取得
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LSB(int x)
        {
            return x & -x;
        }

        /// <summary>
        /// 最上位ビット（最も左の1のビット：Most Significant Bit）の位置を取得。<br></br>
        /// 0を省くためのラッパー関数。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MSBPosition(int x)
        {
            if ( x == 0 )
                return -1;

            // ビルトイン関数。CPU命令を使用するため速い。
            return 31 - System.Numerics.BitOperations.LeadingZeroCount((uint)x);
        }

        /// <summary>
        /// 末尾の0の数を数える 。<br></br>
        /// 0を省くためのラッパー関数。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountTrailingZeros(int x)
        {
            if ( x == 0 )
                return 32;

            // ビルトイン関数。CPU命令を使用するため速い。
            return System.Numerics.BitOperations.TrailingZeroCount(x);
        }

        /// <summary>
        /// 特定の位置のビットが立っているかチェック
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(int x, int pos)
        {
            return (x & (1 << pos)) != 0;
        }

        /// <summary>
        /// 特定の位置のビットをセット
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(int x, int pos)
        {
            return x | (1 << pos);
        }

        /// <summary>
        /// 特定の位置のビットをクリア
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int x, int pos)
        {
            return x & ~(1 << pos);
        }

        /// <summary>
        /// 特定の位置のビットを反転
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FlipBit(int x, int pos)
        {
            return x ^ (1 << pos);
        }

        /// <summary>
        /// 下位n個のビットがすべて1のマスクを生成
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LowerMask(int n)
        {
            return (1 << n) - 1;
        }

        /// <summary>
        /// 指定範囲[l, r)のビットを取得（0-indexed）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBitRange(int x, int l, int r)
        {
            int mask = LowerMask(r) & ~LowerMask(l);
            return (x & mask) >> l;
        }

        /// <summary>
        /// 偶数判定処理。
        /// 1との論理積が0であれば最下位ビットがない数（= 偶数）である。
        /// </summary>
        /// <param name="num">検査対象</param>
        /// <returns>真なら偶数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEvenNumber(int num)
        {
            return (num & 1) == 0;
        }

        #endregion

        #region 高度なビット操作

        /// <summary>
        /// 整数の2のべき乗判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int x)
        {
            return x > 0 && (x & (x - 1)) == 0;
        }

        /// <summary>
        /// 整数をグレイコードに変換。<br></br>
        /// グレイコードとは、ビット全探索で生成されるビットをループごとに変換するための物。<br></br>
        /// 変換後ビットは前のループのビットから一つだけしかビットが変化していない。<br></br>
        /// これにより変更部分だけ（前のループから1つだけ1が増えたか減ったかだけ）見ればよくなる。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToGrayCode(int x)
        {
            return x ^ (x >> 1);
        }

        /// <summary>
        /// グレイコードを整数に戻す。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromGrayCode(int x)
        {
            int result = 0;
            while ( x > 0 )
            {
                result ^= x;
                x >>= 1;
            }
            return result;
        }

        /// <summary>
        /// 循環左シフト<br></br>
        /// 循環シフトはパターンマッチングなどに使う。<br></br>
        /// 1101を循環させて、横にずらしていくことで調査対象に一致する位置があるかを確認するとか。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateLeft(int x, int shift, int bits = 32)
        {
            shift %= bits;
            return ((x << shift) | (x >> (bits - shift))) & ((1 << bits) - 1);
        }

        /// <summary>
        /// 循環右シフト
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateRight(int x, int shift, int bits = 32)
        {
            shift %= bits;
            return ((x >> shift) | (x << (bits - shift))) & ((1 << bits) - 1);
        }

        /// <summary>
        /// 2つの整数の平均値（オーバーフロー回避）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Average(int a, int b)
        {
            return (a & b) + ((a ^ b) >> 1);
        }

        /// <summary>
        /// 絶対値（分岐なし）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int x)
        {
            int mask = x >> 31;
            return (x + mask) ^ mask;
        }

        /// <summary>
        /// 最大値（分岐なし）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b)
        {
            return a ^ ((a ^ b) & -(a < b ? 1 : 0));
        }

        /// <summary>
        /// 最小値（分岐なし）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b)
        {
            return b ^ ((a ^ b) & -(a < b ? 1 : 0));
        }

        #endregion

        #region 集合操作

        /// <summary>
        /// 部分集合の列挙（BitSet表現を使用）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<int> AllSubsets(int set)
        {
            int subset = set;
            yield return subset;  // 自分自身も含む

            while ( true )
            {
                subset = (subset - 1) & set;
                if ( subset == set )
                    break;
                yield return subset;
            }
        }

        /// <summary>
        /// n要素の集合のうち、k要素の組み合わせをすべて列挙（Gosperのハック）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<int> Combinations(int n, int k)
        {
            if ( k > n )
                yield break;

            // 初期状態: 下位k桁が1
            int mask = (1 << k) - 1;

            while ( (mask & (1 << n)) == 0 )
            {
                yield return mask;

                // Gosperのハック: 次のk要素の組み合わせを生成
                int smallest = mask & -mask;
                int ripple = mask + smallest;
                int ones = ((ripple ^ mask) >> 2) / smallest;
                mask = ripple | ones;
            }
        }

        /// <summary>
        /// 集合の要素を文字列として取得
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetToString(int set, int universeSize = 32)
        {
            List<int> elements = new List<int>();
            for ( int i = 0; i < universeSize; i++ )
            {
                if ( IsBitSet(set, i) )
                {
                    elements.Add(i);
                }
            }

            return "{" + string.Join(", ", elements) + "}";
        }

        #endregion

        #region 競技プログラミング特有の関数

        /// <summary>
        /// ビット全探索: 整数配列から部分集合を選んで条件を満たすかチェック
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SubsetSum(int[] arr, int targetSum)
        {
            int n = arr.Length;

            for ( int bit = 0; bit < (1 << n); bit++ )
            {
                int sum = 0;
                for ( int i = 0; i < n; i++ )
                {
                    if ( IsBitSet(bit, i) )
                    {
                        sum += arr[i];
                    }
                }

                if ( sum == targetSum )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// N-Queensの解法カウント（ビットを使用した高速化）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountNQueensSolutions(int n)
        {
            return NQueensHelper(0, 0, 0, 0, n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int NQueensHelper(int row, int ld, int col, int rd, int n)
        {
            if ( row == n )
                return 1;

            int count = 0;
            // 置ける場所のビットマスク
            int mask = ((1 << n) - 1) & ~(ld | col | rd);

            while ( mask > 0 )
            {
                // 最も右の置ける位置
                int bit = mask & -mask;
                mask -= bit;

                // 次の行へ
                count += NQueensHelper(row + 1,
                                      (ld | bit) << 1,
                                      col | bit,
                                      (rd | bit) >> 1,
                                      n);
            }

            return count;
        }

        /// <summary>
        /// 最大クリーク列挙（ビット全探索）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<int> FindMaximalCliques(int[,] graph, int n)
        {
            // 隣接行列からビットマスクに変換
            int[] adjacentMask = new int[n];
            for ( int i = 0; i < n; i++ )
            {
                for ( int j = 0; j < n; j++ )
                {
                    if ( graph[i, j] == 1 )
                    {
                        adjacentMask[i] |= (1 << j);
                    }
                }
            }

            int fullMask = (1 << n) - 1;
            int maxSize = 0;
            List<int> maxCliques = new List<int>();

            // すべての部分集合をチェック
            for ( int subset = 1; subset < (1 << n); subset++ )
            {
                bool isClique = true;

                for ( int i = 0; i < n; i++ )
                {
                    if ( IsBitSet(subset, i) )
                    {
                        // i番目の頂点が他のすべての選択された頂点と隣接しているか確認
                        if ( (subset & adjacentMask[i]) != (subset & ~(1 << i)) )
                        {
                            isClique = false;
                            break;
                        }
                    }
                }

                if ( isClique )
                {
                    int size = PopCount(subset);
                    if ( size > maxSize )
                    {
                        maxSize = size;
                        maxCliques.Clear();
                        maxCliques.Add(subset);
                    }
                    else if ( size == maxSize )
                    {
                        maxCliques.Add(subset);
                    }
                }
            }

            return maxCliques;
        }

        #endregion
    }

    /// <summary>
    /// 効率的なビットセット（ビットベクトル）の実装
    /// .BitVector32使うべきかも
    /// </summary>
    public class BitVector
    {
        private ulong[] data;
        private int size;
        private const int BITS_PER_WORD = 64;

        /// <summary>
        /// 指定したサイズのビットベクトルを初期化
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitVector(int size)
        {
            this.size = size;
            int words = (size + BITS_PER_WORD - 1) / BITS_PER_WORD;
            data = new ulong[words];
        }

        /// <summary>
        /// 指定した位置のビットを設定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int pos)
        {

            int wordIndex = pos / BITS_PER_WORD;
            int bitIndex = pos % BITS_PER_WORD;
            data[wordIndex] |= 1UL << bitIndex;
        }

        /// <summary>
        /// 指定した位置のビットをクリア
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(int pos)
        {

            int wordIndex = pos / BITS_PER_WORD;
            int bitIndex = pos % BITS_PER_WORD;
            data[wordIndex] &= ~(1UL << bitIndex);
        }

        /// <summary>
        /// 指定した位置のビットを反転
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flip(int pos)
        {

            int wordIndex = pos / BITS_PER_WORD;
            int bitIndex = pos % BITS_PER_WORD;
            data[wordIndex] ^= 1UL << bitIndex;
        }

        /// <summary>
        /// 指定した位置のビットが設定されているか確認
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int pos)
        {
            if ( pos < 0 || pos >= size )
                throw new ArgumentOutOfRangeException(nameof(pos));

            int wordIndex = pos / BITS_PER_WORD;
            int bitIndex = pos % BITS_PER_WORD;
            return (data[wordIndex] & (1UL << bitIndex)) != 0;
        }

        /// <summary>
        /// すべてのビットをセット
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll()
        {
            for ( int i = 0; i < data.Length; i++ )
            {
                data[i] = ~0UL;
            }

            // サイズが64の倍数でない場合、使用していないビットをクリア
            if ( size % BITS_PER_WORD != 0 )
            {
                int lastWordIndex = data.Length - 1;
                int usedBitsInLastWord = size % BITS_PER_WORD;
                ulong mask = (1UL << usedBitsInLastWord) - 1;
                data[lastWordIndex] &= mask;
            }
        }

        /// <summary>
        /// すべてのビットをクリア
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAll()
        {
            Array.Clear(data, 0, data.Length);
        }

        /// <summary>
        /// 設定されているビット（1）の数を取得
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            int count = 0;
            for ( int i = 0; i < data.Length; i++ )
            {
                count += BitLib.PopCount((int)(data[i] & 0xFFFFFFFF));
                count += BitLib.PopCount((int)(data[i] >> 32));
            }
            return count;
        }

        /// <summary>
        /// 次に設定されているビットの位置を取得
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextSetBit(int fromIndex)
        {
            if ( fromIndex < 0 )
                fromIndex = 0;
            if ( fromIndex >= size )
                return -1;

            int wordIndex = fromIndex / BITS_PER_WORD;
            int bitIndex = fromIndex % BITS_PER_WORD;

            // fromIndexより右のビットをマスク
            ulong word = data[wordIndex] & (~0UL << bitIndex);

            while ( true )
            {
                if ( word != 0 )
                {
                    // 最も右の1を見つける
                    int lsb = BitLib.CountTrailingZeros((int)(word & 0xFFFFFFFF));
                    if ( lsb == 32 ) // 下位32ビットに1がない
                    {
                        lsb = 32 + BitLib.CountTrailingZeros((int)(word >> 32));
                    }

                    int result = wordIndex * BITS_PER_WORD + lsb;
                    return result < size ? result : -1;
                }

                // 次のワードに移動
                wordIndex++;
                if ( wordIndex >= data.Length )
                    return -1;

                word = data[wordIndex];
            }
        }

        /// <summary>
        /// ビットベクトルのAND操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void And(BitVector other)
        {
            if ( other.size != this.size )
                throw new ArgumentException("BitVector sizes do not match");

            int minLength = Math.Min(data.Length, other.data.Length);
            for ( int i = 0; i < minLength; i++ )
            {
                data[i] &= other.data[i];
            }

            // 残りのビットをクリア
            for ( int i = minLength; i < data.Length; i++ )
            {
                data[i] = 0;
            }
        }

        /// <summary>
        /// ビットベクトルのOR操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Or(BitVector other)
        {
            if ( other.size != this.size )
                throw new ArgumentException("BitVector sizes do not match");

            int minLength = Math.Min(data.Length, other.data.Length);
            for ( int i = 0; i < minLength; i++ )
            {
                data[i] |= other.data[i];
            }
        }

        /// <summary>
        /// ビットベクトルのXOR操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(BitVector other)
        {
            if ( other.size != this.size )
                throw new ArgumentException("BitVector sizes do not match");

            int minLength = Math.Min(data.Length, other.data.Length);
            for ( int i = 0; i < minLength; i++ )
            {
                data[i] ^= other.data[i];
            }
        }

        /// <summary>
        /// ビットベクトルのNOT操作
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Not()
        {
            for ( int i = 0; i < data.Length; i++ )
            {
                data[i] = ~data[i];
            }

            // サイズが64の倍数でない場合、使用していないビットをクリア
            if ( size % BITS_PER_WORD != 0 )
            {
                int lastWordIndex = data.Length - 1;
                int usedBitsInLastWord = size % BITS_PER_WORD;
                ulong mask = (1UL << usedBitsInLastWord) - 1;
                data[lastWordIndex] &= mask;
            }
        }
    }

}