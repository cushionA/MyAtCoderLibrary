using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary.Graph
{
    /// <summary>
    /// シンプルなグリッド。<br/>
    /// 列数と行数と文字配列の配列を受け取ってグリッドを作成し、いくつかの操作を行う
    /// あるマスとあるマスへの最短経路（障害物アリ）とか塗りつぶしとか
    /// 区間和もありだな
    /// </summary>
    public static class GridLib<T> where T
        : System.Numerics.INumber<T>,
          IEquatable<T>,
          ISpanFormattable,
          IBinaryInteger<T>,
          IMinMaxValue<T>
    {

        #region グリッド回転


        /// <summary>
        /// 時計回りに90度回転させたグリッドを返す
        /// </summary>
        /// <returns>回転後の新しいグリッド</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] RotateClockwise90(T[][] grid)
        {
            T[][] rotatedGrid = new T[grid[0].Length][];
            int originalHeight = grid.Length;
            for ( int i = 0; i < rotatedGrid.Length; i++ )
            {
                rotatedGrid[i] = new T[originalHeight];
                for ( int j = 0; j < rotatedGrid[i].Length; j++ )
                {
                    rotatedGrid[i][j] = grid[originalHeight - 1 - j][i];
                }
            }
            return rotatedGrid;
        }

        /// <summary>
        /// 180度回転させたグリッドを返す
        /// </summary>
        /// <returns>回転後の新しいグリッド</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Rotate180(T[][] grid)
        {
            int originalHeight = grid.Length;
            int originalWidth = grid[0].Length;

            T[][] rotatedGrid = new T[originalHeight][];

            for ( int i = 0; i < rotatedGrid.Length; i++ )
            {
                rotatedGrid[i] = new T[originalWidth];

                for ( int j = 0; j < rotatedGrid[i].Length; j++ )
                {
                    rotatedGrid[i][j] = grid[originalHeight - 1 - i][originalWidth - 1 - j];
                }
            }

            return rotatedGrid;
        }

        /// <summary>
        /// 反時計回りに90度（時計回りに270度）回転させたグリッドを返す
        /// </summary>
        /// <returns>回転後の新しいグリッド</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] RotateCounterClockwise90(T[][] grid)
        {
            int width = grid[0].Length;

            T[][] rotatedGrid = new T[width][];
            for ( int i = 0; i < rotatedGrid.Length; i++ )
            {
                rotatedGrid[i] = new T[grid.Length];

                for ( int j = 0; j < rotatedGrid[i].Length; j++ )
                {
                    rotatedGrid[i][j] = grid[j][width - 1 - i];
                }
            }
            return rotatedGrid;
        }

        #endregion グリッド回転

        /// <summary>
        /// グリッド同士の比較用メソッド。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GridEquals(T[][] grid, T[][] other)
        {
            // 幅と高さ確認。
            if ( grid.Length != other.Length || grid[0].Length != other[0].Length )
            {
                return false;
            }

            for ( int i = 0; i < grid.Length; i++ )
            {
                Span<T> row = grid[i];
                Span<T> otherRow = other[i];

                if ( !row.SequenceEqual(otherRow) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// グリッド同士の比較用メソッド。
        /// </summary>
        /// <param name="other"></param>
        /// <returns>二つのグリッドの合わない要素数をチェック。-1は無効</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int UnmatchedCount(T[][] grid, T[][] other)
        {
            // 幅と高さ確認。
            if ( grid.Length != other.Length || grid[0].Length != other[0].Length )
            {
                return -1;
            }

            int wrongCount = 0;

            for ( int i = 0; i < grid.Length; i++ )
            {
                Span<T> row = grid[i];
                Span<T> otherRow = other[i];

                for ( int j = 0; j < row.Length; j++ )
                {
                    if ( row[j] != otherRow[j] )
                    {
                        wrongCount++;
                    }
                }
            }

            return wrongCount;
        }

    }
}
