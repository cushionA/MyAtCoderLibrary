using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
    /// <summary>
    /// 重みのない、有向のパス問題のライブラリ。<br></br>
    /// 制約：辺に重み無し、有向、頂点からは入口、出口の二つ以上の辺は出せない。
    /// <list type="bullet">
    /// <item>パスが閉路であるかの判定。</item>
    /// <item>ある頂点が属するパスの構成要素を返す。</item>
    /// <item>ある頂点が属するパスの先頭を返す。</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// メモ：タスクの処理順の場合、返ってきたパスに閉路がないなら解があり、末尾から処理していけば回答を得られる。
    /// </remarks>
    public class PoolSimplePath
    {

        /// <summary>
        /// ある頂点の前に連結されている頂点の番号。<br></br>
        /// なければ-1。
        /// </summary>
        int[] front;

        /// <summary>
        /// ある頂点の後ろに連結されている頂点の番号。<br></br>
        /// なければ-1。
        /// </summary>
        int[] back;

        /// <summary>
        /// 頂点名が文字列の場合、名前を数字で扱うための対応表。
        /// </summary>
        Dictionary<string, int> nameTable;
        string[] numberTable;


        /// <summary>
        /// 頂点の数と、文字列名で頂点を扱うかを設定するコンストラクタ。<br></br>
        /// 文字列名を使わない場合は名前の対応表などがインスタンス生成されないため、名前系のメソッドは容赦なくCEする。
        /// </summary>
        /// <param name="n">頂点の数</param>
        /// <param name="useStringName">真なら頂点を文字列の名前で呼ぶ</param>
        public PoolSimplePath(int n, bool useStringName)
        {
            //Contract.Assert(n > 0, $"要素数は0以上である必要がある。");

            // 最初は全て独立なので-1で満たす。
            front = new int[n];
            front.AsSpan().Fill(-1);
            back = new int[n];
            back.AsSpan().Fill(-1);

            // 文字列名を使う場合は対応表を作成。
            if ( useStringName )
            {
                nameTable = new Dictionary<string, int>();
                numberTable = new string[n];
            }
        }

        #region プロパティ

        /// <summary>
        /// パスの総数を取得
        /// </summary>
        public int PathCount => GetParents().Length;

        /// <summary>
        /// 最長パスの長さを取得
        /// </summary>
        public int MaxLength => -front[GetMaxParent()];

        /// <summary>
        /// 最短パスの長さを取得
        /// </summary>
        public int MinLength => -front[GetMinParent()];

        #endregion プロパティ


        #region Public関数

        /// <summary>
        /// 頂点と頂点を結んで辺を作成する。
        /// </summary>
        /// <param name="a">親になる頂点</param>
        /// <param name="b">子になる頂点。追加時点では独立した頂点でなければならない。</param>
        public void AddEdge(int a, int b)
        {
            //Contract.Assert(a > 0 && b > 0, $"要素数は0以上である必要がある。");
            // Contract.Assert(a <= rootItem.Length && b <= rootItem.Length, $"頂点数以上の頂点番号");

            // パスのルート頂点を取得。
            int parent = GetRoot(a);

            // 1始動の番号に対応。
            a--;
            b--;

            // ルートオブジェクトの情報も書き換える。
            // -1*パスの長さでパスの長さを表現
            // bの長さを足す形。
            front[parent] += front[b];

            // bの親も書き換える。
            front[b] = a;
            back[a] = b;
        }

        /// <summary>
        /// 頂点と頂点を結んで辺を作成する。<br></br>
        /// 文字列を使用するオーバーロード。
        /// </summary>
        /// <param name="a">親になる頂点</param>
        /// <param name="b">子になる頂点。</param>
        public void AddEdge(String a, string b)
        {
            // 内部で使用する数字。
            int a2, b2;

            Contract.Assert(nameTable != null, $"文字列使用設定でインスタンス化してください。");

            //　文字列が名前に含まれていれば対応する番号を取る。
            if ( nameTable.ContainsKey(a) )
            {
                a2 = nameTable[a];
            }
            // 名前に含まれていなければ追加する。
            else
            {
                a2 = nameTable.Count;
                nameTable.Add(a, a2);
                numberTable[a2] = a;
            }

            //　文字列が名前に含まれていれば対応する番号を取る。
            if ( nameTable.ContainsKey(b) )
            {
                b2 = nameTable[b];
            }
            // 名前に含まれていなければ追加する。
            else
            {
                b2 = nameTable.Count;
                nameTable.Add(b, b2);
                numberTable[b2] = b;
            }

            // 数値で辺を追加。
            AddEdge(a2, b2);

        }

        /// <summary>
        /// ある頂点aを含むパスを頂点aで切断して分離し二つに分ける。
        /// </summary>
        /// <param name="a">切り取る頂点。親になる</param>
        public void SliceEdge(int a)
        {
            // 1始動の番号に対応。
            a--;

            // 切り取る頂点の親
            int b = front[a];

            // すでに頂点なら切らずに戻る。
            if ( b < 0 )
            {
                return;
            }

            // aのパス内での位置
            int position = 1;

            // 配列の値が0以下になるまで回す。
            // bに旧パスの親を取る。
            while ( front[b] >= 0 )
            {
                position++;
                b = front[b];
            }

            int oldLength = front[b];

            // 分離。長さも減らす
            front[b] = -position;
            back[front[a]] = -1;

            // 切断パスに長さを入れる。
            front[a] = oldLength + position;
        }

        /// <summary>
        /// ある頂点aを含むパスを頂点aで切断して二つに分ける。
        /// </summary>
        /// <param name="a">切り取る頂点。親になる</param>
        public void SliceEdge(string a)
        {
            // 頂点aのパス内での位置を探る
            SliceEdge(nameTable[a + 1]);
        }

        /// <summary>
        /// ある頂点を含むパスの親を返す。<br></br>
        /// </summary>
        /// <param name="a">あるパス内の頂点。</param>
        /// <returns>頂点aが属するパスの親</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRoot(int a)
        {
            a--;

            // 配列の値が0以下になるまで回す。
            while ( front[a] >= 0 )
            {
                a = front[a];
            }

            return a;
        }

        /// <summary>
        /// ある頂点を含むパスの親を返す。<br></br>
        /// </summary>
        /// <param name="a">あるパス内の頂点。</param>
        /// <returns>頂点aが属するパスの親の文字列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetRoot(string a)
        {
            int a2 = nameTable[a];

            return numberTable[GetRoot(a2 + 1)];
        }

        /// <summary>
        /// ある頂点を含むパスを返す。<br></br>
        /// </summary>
        /// <param name="a">あるパス内の頂点。</param>
        /// <returns>頂点aが属するパスの親</returns>
        public AnsArray<int> GetPath(int a)
        {
            int parent = GetRoot(a);
            a--;

            // 配列を作る。
            int pathLength = front[parent] * -1;
            int[] path = ArrayPool<int>.Shared.Rent(pathLength);
            Span<int> path2 = path.AsSpan().Slice(0, pathLength);


            for ( int i = 0; i < path2.Length; i++ )
            {
                // 1から始まるインデックスに対応
                path2[i] = parent + 1;
                parent = back[parent];
            }

            return new AnsArray<int>(path, pathLength);
        }

        /// <summary>
        /// ある頂点を含むパスを文字列名で返す。<br></br>
        /// </summary>
        /// <param name="a">あるパス内の頂点。</param>
        /// <returns>頂点aが属するパスの親</returns>
        public AnsArray<string> GetPath(string a)
        {
            int a2 = nameTable[a];
            int parent = GetRoot(a2 + 1);
            int pathLength = front[parent] * -1;

            // 配列を作る。
            string[] path = ArrayPool<string>.Shared.Rent(pathLength);
            Span<string> path2 = path.AsSpan().Slice(0, pathLength);

            for ( int i = 0; i < path2.Length; i++ )
            {
                path2[i] = numberTable[parent];
                parent = back[parent];
            }

            // 値を返す。
            return new AnsArray<string>(path, pathLength);
        }

        /// <summary>
        /// 最も長いパスを返すメソッド。
        /// </summary>
        /// <returns>最大のパス</returns>
        public AnsArray<int> GetMaxPath()
        {
            // 最大の長さのパスの頂点。
            int p = GetMaxParent();

            // 最大パスを返す。
            return GetPath(p);
        }

        /// <summary>
        /// 最も長いパスを返すメソッド。
        /// </summary>
        /// <returns>最大のパス</returns>
        public AnsArray<String> GetMaxPathByName()
        {
            // 最大の長さのパスの頂点。
            int p = GetMaxParent();

            // 最大パスを返す。
            return GetPath(numberTable[p]);
        }

        /// <summary>
        /// 最も短いパスを返すメソッド。
        /// </summary>
        /// <returns>最小のパス。</returns>
        public AnsArray<int> GetMinPath()
        {
            // 最小の長さのパスの頂点。
            int p = GetMinParent();

            // 最小パスを返す。
            return GetPath(p);
        }

        /// <summary>
        /// 最も短いパスを返すメソッド。
        /// </summary>
        /// <returns>最小のパス。</returns>
        public AnsArray<string> GetMinPathByName()
        {
            // 最小の長さのパスの頂点。
            int p = GetMinParent();

            // 最小パスを返す。
            return GetPath(numberTable[p]);
        }

        /// <summary>
        /// 頂点aを含むパスをタスクの連続とみなした時、重複しないようなタスクの処理順を返す。<br></br>
        /// </summary>
        /// <param name="a">頂点aを含むパスのタスクを返す。</param>
        /// <returns>タスクを処理すべき順番に返す。あるいは重複する場合は何も返さない。</returns>
        public AnsArray<int> GetTaskOrder(int a)
        {
            // まずは親を取得。
            int parent = GetRoot(a);
            a--;

            // 閉路を含む、つまり重複しない処理順がないときは何も返さない。
            if ( IsContainCycle(parent) != -1 )
            {
                return null;
            }

            // そうでなければ逆順のパスを返す。
            AnsArray<int> ans = GetPath(a);
            ans.GetSpan.Reverse();
            return ans;
        }

        /// <summary>
        /// 頂点aを含むパスをタスクの連続とみなした時、重複しないようなタスクの処理順を返す。<br></br>
        /// </summary>
        /// <param name="a">頂点aを含むパスのタスクを返す。</param>
        /// <returns>タスクを処理すべき順番に返す。あるいは重複する場合は何も返さない。</returns>
        public AnsArray<string> GetTaskOrder(string a)
        {
            // まずは親を取得。
            int parent = GetRoot(nameTable[a] + 1);

            // 閉路を含む、つまり重複しない処理順がないときは何も返さない。
            if ( IsContainCycle(parent) != -1 )
            {
                return null;
            }

            // そうでなければ逆順のパスを返す。
            AnsArray<string> ans = GetPath(numberTable[parent]);
            ans.GetSpan.Reverse();
            return ans;
        }

        /// <summary>
        /// 頂点aを含むパスが閉路を含む時、閉路部分を抜き出して渡す。<br></br>
        /// </summary>
        /// <param name="a">頂点aを含むパスの閉路を返す。</param>
        /// <returns>閉路があれば返す。存在しなければ何も返さない。</returns>
        public int[] GetCycle(int a)
        {
            // まずは親を取得。
            int parent = GetRoot(a);
            a--;

            // 配列を作る。
            int[] path = ArrayPool<int>.Shared.Rent(front[parent] * -1);
            int pathLength = front[parent] * -1;
            HashSet<int> checker = new HashSet<int>();
            Span<int> path2 = path.AsSpan().Slice(0, pathLength);

            // サイクルが閉じる箇所と、引き起こす番号を記録。
            int cyclePoint = -1;
            int cycleNum = 0;

            for ( int i = 0; i < path2.Length; i++ )
            {
                // 1から始まるインデックスに対応
                path2[i] = parent + 1;
                parent = back[parent];

                // 同じ頂点を複数回通るならサイクルを含んでるよ。
                if ( checker.Contains(path[i]) )
                {
                    cyclePoint = i;
                    cycleNum = path[i];
                    break;
                }
                checker.Add(path2[i]);
            }

            // 破棄
            checker = null;

            // 閉路を含まないときは何も返さない。
            if ( cyclePoint == -1 )
            {
                return null;
            }

            int start = 0;

            for ( int i = 0; i < path2.Length; i++ )
            {
                if ( path2[i] == cycleNum )
                {
                    start = i;
                    break;
                }
            }

            int[] ans = path.AsSpan().Slice(start, cycleNum - start).ToArray();
            ArrayPool<int>.Shared.Return(path);

            // 閉路を返す。
            return ans;
        }

        /// <summary>
        /// 頂点aを含むパスが閉路を含む時、閉路部分を抜き出して渡す。<br></br>
        /// </summary>
        /// <param name="a">頂点aを含むパスの閉路を返す。</param>
        /// <returns>閉路があれば返す。存在しなければ何も返さない。</returns>
        public AnsArray<string> GetAnsCycle(string a)
        {
            // 数値の閉路を取得。
            int[] numCycle = GetCycle(nameTable[a]);

            if ( numCycle == null )
            {
                return null;
            }

            // 文字列閉路を作成。
            string[] sCycle = ArrayPool<string>.Shared.Rent(numCycle.Length);
            int cycleLength = numCycle.Length;

            for ( int i = 0; i < sCycle.Length; i++ )
            {
                sCycle[i] = numberTable[numCycle[i] - 1];
            }

            // 文字列閉路を返す。
            return new AnsArray<string>(sCycle, cycleLength);
        }

        #endregion Public関数

        #region Private関数

        /// <summary>
        /// 頂点<paramref name="a"/>を含むパスが閉路、あるいはサイクルを持つかどうかを確認する関数。
        /// </summary>
        /// <param name="a">調べる頂点</param>
        /// <returns>閉路を持つかどうかを返す。持たなければ-1、持っていれば交差地点の頂点を返す</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IsContainCycle(int a)
        {
            a--;
            // パスを取得し、ハッシュセットで頂点重複確認。
            AnsArray<int> pathSource = GetPath(a);
            ReadOnlySpan<int> path = pathSource.GetReadOnlySpan;
            HashSet<int> checker = new HashSet<int>();

            for ( int i = 0; i < path.Length; i++ )
            {
                // 同じ頂点を複数回通るならサイクルを含んでるよ。
                if ( checker.Contains(path[i]) )
                {
                    return i;
                }
                checker.Add(path[i]);
            }

            pathSource.Dispose();

            // 重複なければOK
            return -1;
        }

        /// <summary>
        /// 最大の長さのパスの親を返す。
        /// </summary>
        /// <returns>最大パスの親となる頂点。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetMaxParent()
        {
            // 最大の長さのパスの頂点と長さ。
            int max = 0;
            int maxLen = -1;

            // 各frontの長さを検査。
            for ( int i = 0; i < front.Length; i++ )
            {
                if ( front[i] < maxLen )
                {
                    max = i;
                    maxLen = front[i];
                }
            }

            // 最長パスの親を返す。
            return max;
        }

        /// <summary>
        /// 最小の長さのパスの親を返す。
        /// </summary>
        /// <returns>最小パスの親となる頂点。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetMinParent()
        {
            // 最小の長さのパスの頂点と長さ。
            int min = 0;
            int minLen = int.MinValue;

            // 各frontの長さを検査。
            for ( int i = 0; i < front.Length; i++ )
            {
                if ( front[i] < -1 && front[i] > minLen )
                {
                    min = i;
                    minLen = front[i];
                }
            }

            // 最小パスの親を返す。
            return min;
        }

        /// <summary>
        /// パスのルートである親頂点のSpanを返すメソッド。<br></br>
        /// 全てのパスを調べたい時、準備として親を取り出したりする。
        /// </summary>
        /// <returns>親頂点である頂点の集合。</returns>
        private Span<int> GetParents()
        {
            List<int> pList = new List<int>();

            // パスリストがあれば親なのでリストに追加。
            for ( int i = 0; i < front.Length; i++ )
            {
                if ( front[i] < 0 )
                {
                    pList.Add(i);
                }
            }

            // 親の頂点のSpanを返す。
            return CollectionsMarshal.AsSpan(pList);
        }

        #endregion Private関数


        #region デバッグ用

        /// <summary>
        /// 全パスの詳細情報を出力
        /// </summary>
        public void PrintAllPathsInfo()
        {
            var parents = GetParents();
            foreach ( int parent in parents )
            {
                var path = GetPath(parent);
                bool hasCycle = IsContainCycle(parent) != -1;

                Console.WriteLine($"Path (Parent: {parent}):");
                Console.WriteLine($"  Length: {path.Length}");
                Console.WriteLine($"  Has Cycle: {hasCycle}");
                // Console.WriteLine($"  Vertices: {string.Join(" -> ", path.ToArray())}");
            }
        }

        #endregion デバッグ用


    }
}
