using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
    /// <summary>
    /// 解答用の配列を管理するラッパークラス。<br></br>
    /// カンマ区切りとか一行ごととかで文字列を作成。<br></br>
    /// 使用後にプールに配列を返す。<br></br>
    /// 大体必要な長さより長いからちゃんと実際の長さも持つ。
    /// </summary>
    public class AnsArray<T> : IDisposable
    {

        /// <summary>
        /// 配列の長さを返すプロパティ。
        /// </summary>
        public int Length { get { return length; } }

        /// <summary>
        /// 読み取り専用のスパンを返す。
        /// </summary>
        public ReadOnlySpan<T> GetReadOnlySpan { get { return array.AsSpan().Slice(0, length); } }

        /// <summary>
        /// スパンを返す。
        /// </summary>
        public Span<T> GetSpan { get { return array.AsSpan().Slice(0, length); } }

        /// <summary>
        /// コンストラクタ。配列を設定。
        /// </summary>
        /// <param name="ans"></param>
        public AnsArray(T[] ans, int arrayLength, bool useReverse = false)
        {
            array = ans;
            length = arrayLength;
        }

        /// <summary>
        /// 解答となる配列。
        /// </summary>
        T[] array;

        /// <summary>
        /// 配列の実際の長さ。
        /// </summary>
        private readonly int length;

        public string OutPutAns()
        {

            StringBuilder s = new StringBuilder();

            ReadOnlySpan<T> values = GetReadOnlySpan;

            s.Append(values[0].ToString());
            for ( int i = 1; i < values.Length; i++ )
            {
                s.Append(' ');
                s.Append(values[i].ToString());
            }


            ArrayPool<T>.Shared.Return(array);

            return s.ToString();
        }

        /// <summary>
        /// 配列を返す。
        /// </summary>
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }
}
