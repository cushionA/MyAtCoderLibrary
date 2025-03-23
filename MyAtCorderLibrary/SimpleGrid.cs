using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SimpleGrid
    {
        /// <summary>
        /// グリッドで障害物として扱われる文字。
        /// </summary>
        const char BLOCK = '#';

        /// <summary>
        /// グリッドで空白として扱われる文字。
        /// </summary>
        const char EMPTY = '.';

        /// <summary>
        /// 幅の初期化はコンストラクタでのみ行う。
        /// </summary>
        private readonly char[][] grid;

        private int height;
        private int width;

        /// <summary>
        /// コンストラクタで初期化を行う。<br/>
        /// このクラスの外で入力を取るなりして作成したグリッドを受け取る。
        /// </summary>
        public SimpleGrid(int height, int width, char[][] useGrid)
        {
            this.grid = useGrid;
            this.height = height;
            this.width = width;
        }


    }
}
