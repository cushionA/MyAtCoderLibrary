using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary.Graph
{
    /// <summary>
    /// 二次元累積和のコード。<br/>
    /// 二次元空間での物の重なりの問題に使用する<br/>
    /// このコードでは横方向→縦方向の累積和を使用する。<br/>
    /// 制約として座標は(0,0)から始まる。同じ重なりの高さの部分がバラバラに複数あったらどうしよう。
    /// </summary>
    public class CumulativeSumGrid
    {
        /// <summary>
        /// グリッドの本体。<br/>
        /// 外から参照する必要はない。
        /// </summary>
        private int[][] grid;

        /// <summary>
        /// すでに集計を行っているというフラグ。<br/>
        /// 一つのインスタンスにつき一度しか集計はできない。
        /// </summary>
        private bool isDone;

        private int height;
        private int width;

        /// <summary>
        /// コンストラクタでグリッドを初期化する。
        /// </summary>
        /// <param name="height">グリッドの高さ</param>
        /// <param name="width">グリッドの幅</param>
        public CumulativeSumGrid(int height, int width)
        {
            // ジャグ配列の初期化
            grid = new int[height + 1][];
            grid.AsSpan().Fill(new int[width + 1]);

            // 縦横の長さも記録。
            this.height = height;
            this.width = width;
        }

        #region publicメソッド。

        /// <summary>
        /// 指定した範囲に変化を適用する。<br/>
        /// 始点と終点は列、行の(x，y)形式で指定する。
        /// </summary>
        /// <param name="startRangePoint">指定範囲の左上、始点</param>
        /// <param name="endRangePoint">指定範囲の右下、終点</param>
        /// <param name="changeValue">範囲内で変化する値。負の数もオーケー</param>
        public void UpdateRange((uint, uint) startRangePoint, (uint, uint) endRangePoint, int changeValue)
        {
            // 範囲の始まりに値を追加する。
            grid[startRangePoint.Item2][startRangePoint.Item1] += changeValue;

            // 範囲の外側に累積加算の影響が及ぶのを止めるための防波堤になる減算を行う。
            grid[startRangePoint.Item2][endRangePoint.Item1 + 1] -= changeValue;
            grid[endRangePoint.Item2 + 1][startRangePoint.Item1] -= changeValue;

            // 二つの減算の影響が二重に及んでしまう部分に加算を行う。
            grid[endRangePoint.Item2][endRangePoint.Item1] += changeValue;
        }

        /// <summary>
        /// グリッド内を走査して最大値とその座標を返す。
        /// </summary>
        /// <returns>最大値,x座標,y座標</returns>
        public (int, int, int) GetMaxValue()
        {
            // 集計実行。
            SumAct();

            (int, int, int) ans = (int.MinValue, 0, 0);

            // グリッド全体をチェック。
            // Lengthを-1するのは一番外側は不要だから。
            for ( int i = 0; i < grid.Length - 1; i++ )
            {
                Span<int> row = grid[i].AsSpan();

                for ( int s = 0; s < row.Length - 1; s++ )
                {
                    // 最大値を更新したら最大値と座標を入れる。
                    if ( row[s] > ans.Item1 )
                    {
                        // 最大値、列、行の順に格納。
                        ans = (row[s], s, i);
                    }
                }
            }

            return ans;
        }


        /// <summary>
        /// グリッド内を走査して最小値とその座標を返す。
        /// 使わない気がしますね。
        /// </summary>
        /// <returns>最小値,x座標,y座標</returns>
        public (int, int, int) GetMinValue()
        {
            // 集計実行。
            SumAct();

            (int, int, int) ans = (int.MaxValue, 0, 0);

            // グリッド全体をチェック。
            // Lengthを-1するのは一番外側は不要だから。
            for ( int i = 0; i < grid.Length - 1; i++ )
            {
                Span<int> row = grid[i].AsSpan();

                for ( int s = 0; s < row.Length - 1; s++ )
                {
                    // 最小値を更新したら最小値と座標を入れる。
                    if ( row[s] < ans.Item1 )
                    {
                        // 最小値、列、行の順に格納。
                        ans = (row[s], s, i);
                    }
                }
            }

            return ans;
        }

        /// <summary>
        /// ある座標を始点とする範囲の面積を返すメソッド。<br/>
        /// 縦横に検査する。
        /// </summary>
        /// <param name="startPoint">x,y座標形式の始点。列、行の形式。</param>
        /// <returns>その範囲の面積。</returns>
        public int GetRangeSize((uint, uint) startPoint)
        {
            // 集計実行
            SumAct();

            // 範囲の幅
            int areaWidth = 0;
            int i = (int)startPoint.Item1;
            // スタート地点から横に検査。
            {
                Span<int> row = grid[startPoint.Item2].AsSpan();


                // 右端につくまで、または違う値に行き当たるまでループする。
                while ( i < row.Length - 1 )
                {
                    // 同じ値が出る限りは一つ先へ進める。
                    i++;

                    if ( row[i] != row[i - 1] )
                    {
                        break;
                    }
                }
                // エリアの幅を求める。
                areaWidth = i - (int)startPoint.Item1;
            }

            // 範囲の高さ
            int areaHeight = 0;
            int s = (int)startPoint.Item2;
            {
                // 縦方向の長さをはかる。
                while ( s < grid.Length - 1 )
                {
                    s++;
                    if ( grid[s][i - 1] != grid[s - 1][i - 1] )
                    {
                        break;
                    }
                }
                areaHeight = s - (int)startPoint.Item2;
            }

            // 面積を返す
            return areaHeight * areaWidth;
        }

        #endregion publicメソッド。

        #region privateメソッド。

        /// <summary>
        /// 加工したグリッドをもとに集計を行う。<br/>
        /// 一つのインスタンスにつき一度きりの操作。
        /// </summary>
        private void SumAct()
        {
            // すでに集計済みなら戻る。
            if ( isDone )
            {
                return;
            }

            // 横集計。
            for ( int i = 0; i < grid.Length - 1; i++ )
            {
                Span<int> row = grid[i].AsSpan();
                // 一個先の要素に今の要素を足していく。
                for ( int s = 0; s < row.Length - 2; s++ )
                {
                    row[s + 1] += row[s];
                }
            }
            // 縦集計。
            for ( int i = 0; i < width; i++ )
            {
                // 一個下の要素に今の要素を足していく。
                for ( int s = 0; s < grid.Length - 2; s++ )
                {
                    grid[s + 1][i] = grid[s][i];
                }
            }

            // すでに集計は行ったとして以後に再度集計はしない。
            isDone = true;
        }

        #endregion privateメソッド。
    }
}
