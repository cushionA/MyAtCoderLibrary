
namespace MyAtCoderLibrary.Graph
{

    /// <summary>
    /// 無向グラフの頂点を入れ替えるクラス。<br/>
    /// あるいは順列を入れ替えて全パターンを作成する。<br/>
    /// N個の頂点の番号を　0-(N-1) で表現する。<br/>
    /// だから読み取った頂点番号からは1引く。<br/>
    /// そしてこのクラスでは 0-(N-1) の頂点番号の配列を作り、それを並び変えてすべてのパターンを作ることで、二つの無向グラフ間で頂点の対応入れ替え順列を作る。<br/>
    /// つまりint P[] の配列内の頂点の位置をバラバラに入れ替えることで、 P[i] の時 頂点i が 頂点P[i] となる。
    /// </summary>
    public class PermutationSwap
    {
        /// <summary>
        /// 頂点を格納する配列。
        /// </summary>
        public int[] vertices;

        /// <summary>
        /// コンストラクタ。<br/>
        /// 頂点情報を持つ配列を初期化する。
        /// </summary>
        /// <param name="vertexCount">無向グラフの頂点数</param>
        public PermutationSwap(int vertexCount)
        {
            // 0-頂点数 の数値を昇順に格納した配列を作成する。
            vertices = Enumerable.Range(0, vertexCount).ToArray();
        }

        /// <summary>
        /// 頂点を入れ替えるパターンが残っていれば入れ替えて真を返す。<br/>
        /// このメソッドが真を返す限り頂点の入れ替えのパターンは尽きていないということ。
        /// </summary>
        /// <returns>返り値が真であればまだ頂点を入れ替えられる</returns>
        public bool IsContinueSwap()
        {
            // 配列の後ろからチェックしていくためにiを初期化する。 
            int i = vertices.Length - 1;


            // 後ろから一番大きい要素の位置を見つける。
            // 最初は完全な昇順なので、後ろから見ていけば一番大きい要素はすぐに見つかる。
            // 前の要素が今の要素より大きければ先を探る。
            while ( i > 0 && vertices[i - 1] >= vertices[i] )
            {
                i--;
            }

            // iがゼロになるまでループが続く、つまりすでに全て降順になっていた場合はもう入れ替えられないのでfalseを返す。
            if ( i <= 0 )
            {
                return false;
            }

            // i-1 つまり一番大きい要素の手前の数値より大きい要素を探す。
            // これは最大の数を入れ替えにより前へ前へと運びながら、それ以降のインデックスで大きい要素と入れ替えるため。
            int j = vertices.Length - 1;
            while ( vertices[j] <= vertices[i - 1] )
            {
                j--;
            }

            // 入れ替え処理。
            int swap = vertices[i - 1];
            vertices[i - 1] = vertices[j];
            vertices[j] = swap;

            // 入れ替えた部分を昇順に並べて、辞書順で順列を取得できるようにする。
            vertices.AsSpan().Slice(i, vertices.Length - i).Reverse();

            // まだ並び替えが可能であるという返り値を返す。
            return true;
        }

    }
}

