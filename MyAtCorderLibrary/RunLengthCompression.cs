using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
    /// <summary>
    /// ランレングス圧縮は連続データの性質、特徴をまとめるだけ。<br/>
    /// パターンマッチングなどはこれを利用して改めて行う。<br/>
    /// 検出した連続や関係の境界や長さを使って分析を行う。
    /// </summary>
    public static class RunLengthCompression
    {

        /// <summary>
        /// 値連続ベースの圧縮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<(T value, int count)> GroupByEquality<T>(Span<T> source)
    where T : IEquatable<T>
        {
            var result = new List<(T, int)>();

            if ( source.Length == 0 )
                return result;

            int i = 0;
            while ( i < source.Length )
            {
                T currentValue = source[i];
                int count = 1;

                // 同じ値が連続する間カウントを増やす
                int j = i + 1;
                while ( j < source.Length && currentValue.Equals(source[j]) )
                {
                    count++;
                    j++;
                }

                // 結果を追加
                result.Add((currentValue, count));

                // 次の異なる値の位置から再開
                i = j;
            }

            return result;
        }

        /// <summary>
        /// 関係性ベースの圧縮<br/>
        /// 連続する二値の比較結果で関係性を求めて数える。
        /// </summary>
        /// <typeparam name="T">連続データの値の型</typeparam>
        /// <typeparam name="TRelation">値同士の関係性を示すデータの型</typeparam>
        /// <param name="source"></param>
        /// <param name="relationFunc"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<(TRelation relation, int count)> GroupByRelational<T, TRelation>(Span<T> source, Func<T, T, TRelation> relationFunc)
            where T : IComparable<T>
            where TRelation : IEquatable<TRelation>
        {
            // 圧縮結果
            var result = new List<(TRelation, int)>();

            // 要素数2以下なら戻る
            if ( source.Length < 2 )
                return result;

            // 最初の値だけ判定して初期値とする。
            // 同じ判定最初にやるけど分岐条件減らせるから一回くらいは重複していい。
            TRelation currentRelation = relationFunc(source[0], source[1]);
            int count = 0;

            // 隣り合う値の関係性を見ていく。
            // 3,4,2の 3,4と4,2のように同じ文字が別のパターンの構成要素になりうることに注意。
            for ( int i = 0; i < source.Length - 1; i++ )
            {
                // 今の値と次の値で関係性チェック
                TRelation relation = relationFunc(source[i], source[i + 1]);

                // 一致していれば関係性の長さを加算。
                if ( relation.Equals(currentRelation) )
                {
                    count++;
                }

                // そうでなければ次の関係性を計算。
                else
                {
                    result.Add((currentRelation, count));
                    currentRelation = relation;
                    count = 1;
                }
            }

            // 最後まで続いていた関係性を最後に追加。
            result.Add((currentRelation, count));

            // 結果を返す。
            return result;
        }

        /// <summary>
        /// 連続値圧縮後の復元用メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encoded"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> DecodeBySequential<T>(IEnumerable<(T value, int count)> encoded)
        {
            var result = new List<T>();
            foreach ( var (value, count) in encoded )
            {
                for ( int i = 0; i < count; i++ )
                {
                    result.Add(value);
                }
            }
            return result;
        }
    }
}
