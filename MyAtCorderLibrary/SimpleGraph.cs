
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MyAtCoderLibrary.Graph
{

	/// <summary>
	/// 有向、無向の頂点と辺を結んだグラフを作成するクラス。<br/>
	/// 最大フローなどの問題ではなく、単純に頂点ごとに辺の情報を保存したり、最長パスを求めたりするのに使用する。<br/>
	/// 制約として、辺の出発点と行先が重複する辺の追加、既存の辺の編集はできない。<br/>
	/// ジェネリック <typeparamref name="T"/> は辺の長さを格納するデータ型。辺の長さが1だけだと保証されてるならIntとかでいい。<br/>
	/// 頂点を指定・返す際は1からnまでの範囲。
	/// <remarks>
	/// 機能は以下の通りで、Publicメソッドを使用してアクセスできる。
	/// <list type="bullet">
	/// <item>・AddEdge メソッド(オーバーロード二つ)による辺の追加、</item>
	/// <item>・GetMaxPath メソッドによるグラフ内の最大パスの取得</item>
	/// <item>・GetMinPath(s,t) メソッドによるs,t間の最短距離の取得</item>
	/// </list>
	/// </remarks>
	/// </summary>
	public class SimpleGraph<T> where T
		: System.Numerics.INumber<T>
		, System.Numerics.IMinMaxValue<T>
	{
		/// <summary>
		/// このフラグが真であれば無向グラフになる。<br/>
		/// 辺を追加する際に双方向になる。
		/// </summary>
		protected readonly bool isUnDirected;

		/// <summary>
		/// グラフの頂点数。
		/// </summary>
		protected readonly int vertexCount;

		/// <summary>
		/// 要素数が頂点数の配列。<br/>
		/// 各要素は頂点に対応していて、頂点が持つ要素のディクショナリは各頂点が持つ辺のそれぞれの行先の頂点番号を保持している。<br/>
		/// intは行先の頂点で、Tは辺の長さ
		/// </summary>
		protected readonly List<(int, T)>[] vertexEdges;

		/// <summary>
		/// 要素数が頂点数の配列。<br/>
		/// 各要素は頂点に対応していて、頂点が持つ要素は各頂点が持つ辺のそれぞれの行先の頂点番号を保持している。<br/>
		/// 辺の検索用のHashSet
		/// </summary>
		protected readonly HashSet<int>[] edgesHash;

		/// <summary>
		/// グラフの辺が均一な長さかどうか。<br/>
		/// このフラグが真で、なおかつ無向グラフかどうかで最短パスの求め方にダイクストラと幅優先が使い分けられる。
		/// </summary>
		protected bool isUniformRange = true;

		/// <summary>
		/// 探索時に距離を格納するための配列。
		/// </summary>
		T[] distance;

		/// <summary>
		/// グラフの基本情報を設定するコンストラクタ。
		/// </summary>
		/// <param name="isUnDirected">無向グラフであるかどうか</param>
		/// <param name="vertexCount">グラフの頂点数はいくつか</param>
		public SimpleGraph(bool isUnDirected, int vertexCount)
		{
			this.isUnDirected = isUnDirected;
			this.vertexCount = vertexCount;
			this.vertexEdges = new List<(int, T)>[vertexCount];
			this.edgesHash = new HashSet<int>[vertexCount];
			this.distance = new T[vertexCount];
		}

		#region publicメソッド

		/// <summary>
		/// グラフに辺を追加するメソッド。<br/>
		/// すべての辺が同じ長さの場合、このメソッドを使用してはならない。
		/// </summary>
		/// <param name="from">辺が伸びる元の頂点。1から始まる数値で指定。</param>
		/// <param name="to">辺が伸びる先の頂点。1から始まる数値で指定。</param>
		/// <param name="length">辺の長さ。</param>
		public void AddEdge(int from, int to, T length)
		{
			// 1から始まる頂点指定を0始点に変換。
			from--;
			to--;

			if ( edgesHash[from] == null )
			{
				edgesHash[from] = new HashSet<int>();
				vertexEdges[from] = new List<(int, T)>();
			}

			// fromとtoが同じか、すでに頂点が辺を含んでいるか、辺の長さがゼロ以下なら何もせずに戻る。
			if ( from == to || edgesHash[from].Contains(to) || length <= T.AdditiveIdentity )
			{
				return;
			}

			// このメソッドを使用した時点で辺の長さが均一であるとはみなされなくなる。
			isUniformRange = false;

			// 頂点に辺を追加する。
			edgesHash[from].Add(to);
			vertexEdges[from].Add((to, length));

			// 無向グラフであれば逆向きの辺も追加する。
			if ( isUnDirected )
			{
				if ( edgesHash[to] == null )
				{
					edgesHash[to] = new HashSet<int>();
					vertexEdges[to] = new List<(int, T)>();
				}

				// 頂点に辺を追加する。
				edgesHash[to].Add(from);
				vertexEdges[to].Add((from, length));
			}
		}

		/// <summary>
		/// グラフに辺を追加するメソッド。<br/>
		/// 辺の長さを指定しないオーバーロード。<br/>
		/// 辺の長さが均一なら絶対にこれを使用する。
		/// すべての辺の長さが3とかなら最後に取得したパスの長さを三倍してくれ。
		/// </summary>
		/// <param name="from">辺が伸びる元の頂点。1から始まる数値で指定。</param>
		/// <param name="to">辺が伸びる先の頂点。1から始まる数値で指定。</param>
		/// <param name="length">辺の長さ。</param>
		public void AddEdge(int from, int to)
		{
			// 1から始まる頂点指定を0始点に変換。
			from--;
			to--;

			if ( edgesHash[from] == null )
			{
				edgesHash[from] = new HashSet<int>();
				vertexEdges[from] = new List<(int, T)>();
			}

			// fromとtoが同じか、すでに頂点が辺を含んでいるか、辺の長さがゼロ以下なら何もせずに戻る。
			if ( from == to || edgesHash[from].Contains(to) )
			{
				return;
			}

			T length = T.One;

			// 頂点に辺を追加する。
			edgesHash[from].Add(to);
			vertexEdges[from].Add((to, length));

			// 無向グラフであれば逆向きの辺も追加する。
			if ( isUnDirected )
			{
				if ( edgesHash[to] == null )
				{
					edgesHash[to] = new HashSet<int>();
					vertexEdges[to] = new List<(int, T)>();
				}

				// 頂点に辺を追加する。
				edgesHash[to].Add(from);
				vertexEdges[to].Add((from, length));
			}
		}

		/// <summary>
		/// ある頂点からある頂点へ向かう辺を含んでいるかどうか、含んでいるとしたら長さはどれくらいかを返すメソッド。<br/>
		/// 長さゼロの辺なんてないからその辺が存在してないなら0を返す。
		/// </summary>
		/// <param name="from">辺が伸びる元の頂点。1から始まる数値で指定。</param>
		/// <param name="to">辺が伸びる先の頂点。1から始まる数値で指定。</param>
		/// <returns>返り値が正の数なら辺を含んでいて、なおかつ長さを示している。0なら含んでない</returns>
		public T IsContainEdge(int from, int to)
		{
			// 1から始まる頂点指定を0始点に変換。
			from--;
			to--;

			if ( edgesHash[from].Contains(to) )
			{
				return vertexEdges[from][to].Item2;
			}

			// 含んでないなら0を返す。
			return T.AdditiveIdentity;
		}

		/// <summary>
		/// 現在のグラフの最大パスの長さを返すメソッド。
		/// </summary>
		/// <returns>このグラフの最大パス</returns>
		public T GetMaxPath()
		{
			// 各頂点の距離を持つスパンを作成する
			Span<T> distSpan = new T[vertexCount];

			// 頂点ゼロからの最大距離を求める。
			int maxVertex = Dfs(ref distSpan, 0);

			// 前回の最大距離頂点から再び最大距離を求めることで全体でどれだけ長いのか、という点を求める。
			maxVertex = Dfs(ref distSpan, maxVertex);

			return distSpan[maxVertex];
		}

		/// <summary>
		/// 現在のグラフの二頂点間を結ぶ最小パスの長さを返すメソッド。
		/// </summary>
		/// <param name="s">スタート地点。1から始まる数値で指定。</param>
		/// <param name="t">ゴール地点。1から始まる数値で指定。</param>
		/// <returns>二点間の最短距離。パスが存在しない場合はTの最小値を返す。</returns>
		public T GetMinPath(int s, int t)
		{
			// 1から始まる頂点指定を0始点に変換。
			s--;
			t--;

			// スタート地点とゴール地点が同じなのはダメ。
			if ( s == t )
			{
				return T.Zero;
			}

			// 無向グラフで、なおかつすべての辺の長さが等しいなら幅優先探索をして求める。
			if ( isUniformRange && isUnDirected )
			{
				return BFS(s, t);
			}

			// そうでないならダイクストラ法を使う。 
			// 負の重みがある場合のためにベルマンフォードも実装するかな。
			else
			{
				return DkStr(s, t);
			}
		}

		/// <summary>
		/// 現在のグラフのある頂点から他の全頂点に対する最短距離を返すメソッド。
		/// </summary>
		/// <param name="s">スタート地点。1から始まる数値で指定。</param>
		/// <returns>スタート地点から全頂点への距離を格納するスパン</returns>
		public Span<T> GetMinPathToAllVertex(int s)
		{
			// 1から始まる頂点指定を0始点に変換。
			s--;

			// 無向グラフで、なおかつすべての辺の長さが等しいなら幅優先探索をして求める。
			if ( isUniformRange && isUnDirected )
			{
				return BFS(s);
			}

			// そうでないならダイクストラ法を使う。 
			// 負の重みがある場合のためにベルマンフォードも実装するかな。
			else
			{
				return DkStr(s);
			}
		}

		/// <summary>
		/// 現在のグラフの二頂点間を結ぶ全パターンのパスの中で、任意の条件を最も満たす物の長さ（重み）を返す。<br/>
		/// DFS（深さ優先探索）を使う。計算量が多いため頂点数が二桁でもない限り使わない。
		/// </summary>
		/// <param name="s">スタート地点。1から始まる数値で指定。</param>
		/// <param name="t">ゴール地点。1から始まる数値で指定。</param>
		/// <param name="init">初期値、Int.MinValue等に相当する、条件判断で更新される前の値</param>
		/// <param name="judgeFunc">判断用の関数。引数1は現在の一番条件に近い値、引数2は入れ替え対象か調べる値、返り値は結果</param>
		/// <param name="op">重み同士をどのような演算子で結合するかの指定（デフォは+）</param>
		/// <returns>計算結果と、そのパスをSpanにした物を返す。</returns>
		public (T, int[]) SearchPathByFunc(int s, int t, T init, Func<T, T, bool> judgeFunc, OPERATOR op = OPERATOR.加算)
		{
			// 1から始まる頂点指定を0始点に変換。
			s--;
			t--;

			// 全パスを取得する。
			Span<int[]> paths = FindPaths(s, t, ArrayPool<bool>.Shared.Rent(vertexCount), new Stack<int>(), new List<int[]>());

			// 重み計算用のFuncを取得。
			Func<T, T, T> calc = (Func<T, T, T>)ExMath.GetCalculator<T>(op);

			// 答えとなる重みの総量
			T ans = init;

			// 答えのパスのIndex
			int index = 0;

			// ここから辺の重みを取得して計算していくよ
			// 一つずつパスを確認する。
			for ( int i = 0; i < paths.Length; i++ )
			{
				int[] path = paths[i];

				// 最初の重みはそのまま処理して、二回目以降は任意の演算子を使う。
				T result = GetEdgeWeight(path[0], path[1]);

				for ( int j = 1; j < path.Length - 1; j++ )
				{
					result = calc(result, GetEdgeWeight(path[j], path[j + 1]));
				}

				// もし現在の結果と今の結果を比較して入れ替わりが起こるなら入れ替える。
				if ( judgeFunc(ans, result) )
				{
					ans = result;
					index = i;
				}
			}

			// 答えを返す。
			return (ans, paths[index]);

		}


		#endregion

		#region privateメソッド

		#region 最大パス

		/// <summary>
		/// 深さ探索でスタート地点から一番遠い頂点までの距離を求める。
		/// </summary>
		/// <param name="distSpan">各要素が各頂点に対応するspan。各頂点のスタート地点からの距離を持つ。</param>
		/// <param name="startPoint">深さ優先探索のスタート地点。</param>
		private int Dfs(ref Span<T> distSpan, int startPoint)
		{
			// スパンの内容を最小値で初期化。最小値であればまだ訪問されていない頂点。
			distSpan.Fill(T.MinValue);

			// スタート地点の距離は0で初期化する。
			distSpan[startPoint] = T.AdditiveIdentity;

			// DFSに使うスタックを作成して初期頂点を追加する。
			Stack<int> stack = new Stack<int>();
			stack.Push(startPoint);

			// 最長距離のインデックス。
			int maxVertex = -1;

			// スタックの要素が尽きるまで探索を続ける。
			while ( stack.Count > 0 )
			{
				int visit = stack.Pop();
				// スタックから取り出した頂点から伸びる辺のSpanを取り出す。
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[visit]);

				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					int toVertex = edgeSpan[i].Item1;
					T edgeDist = edgeSpan[i].Item2;

					// 未訪問なら更新する。何度も同じ頂点を通れると値が膨らみ続けるので、一度しか更新しない。
					if ( distSpan[toVertex] == T.MinValue )
					{
						distSpan[toVertex] = distSpan[visit] + edgeDist;

						stack.Push(toVertex);

						// 最大パスの頂点を更新する。
						if ( maxVertex == -1 || distSpan[toVertex] > distSpan[maxVertex] )
						{
							maxVertex = toVertex;
						}
					}
				}
			}

			// スタート地点から見た最大パスの頂点を返す。
			return maxVertex;

		}

		#endregion 最大パス

		#region 最小パス

		/// <summary>
		/// 二点間の最短距離を求めるための幅優先探索。
		/// </summary>
		/// <param name="s">スタート地点</param>
		/// <param name="t">ゴール地点</param>
		/// <returns>二点間の最短距離。</returns>
		private T BFS(int s, int t)
		{
			// 幅優先探索に使うキュー。
			Queue<int> queue = new Queue<int>();

			// BFSは各頂点を一度しか訪問しないので、訪問済みの頂点番号はHashSetに入れて照会する。
			bool[] visited = ArrayPool<bool>.Shared.Rent(vertexCount);

			// スタート地点から各頂点への距離を示す。
			// インデックスが頂点を表して、各要素の値がスタート地点からの距離。
			// 初期処理として配列を最大値で満たす。
			distance.AsSpan().Fill(T.MaxValue);

			// そしてスタート地点にはゼロを入れる。
			distance[s] = T.AdditiveIdentity;

			// スタート地点をキューに追加し、訪問済みにする。
			queue.Enqueue(s);
			visited[s] = true;

			while ( queue.Count > 0 )
			{
				// キューから次に訪問する頂点を取り出して、その頂点から出ている辺のリストを作成する。
				int currentVertex = queue.Dequeue();
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[currentVertex]);

				// 頂点から行先を一つずつ見ていく。
				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					// 行先の頂点。
					int toVertex = edgeSpan[i].Item1;

					// もし終了位置に来ていたなら値を返して終わり。
					if ( toVertex == t )
					{
						// poolに配列を返す。
						ArrayPool<bool>.Shared.Return(visited, true);

						// 行先の頂点の距離を返す。
						return distance[currentVertex] + T.One;
					}

					// 行先の頂点をすでに訪問済みなら(すでにある = より近いパスで訪れているので)飛ばす
					if ( visited[toVertex] )
					{
						continue;
					}

					// 訪問済みにする。
					visited[toVertex] = true;



					// 行先の頂点は一個プラスした距離になる。
					distance[toVertex] = distance[currentVertex] + T.One;

					// 次はこの頂点から伸びる辺をキューに入れて検査する。
					queue.Enqueue(toVertex);
				}
			}

			// 配列を返す。
			ArrayPool<bool>.Shared.Return(visited, true);

			// 到達できなかったら最小値を返す。
			return T.MinValue;

		}

		/// <summary>
		/// 二点間の最短距離を求めるためのダイクストラ法。<br/>
		/// 軽く原理を記録しておくと、ある頂点から到達可能な頂点を次々に優先度付きキューに追加して、近い順に頂点が取り出されるようにするというところ。
		/// </summary>
		/// <param name="s">スタート地点</param>
		/// <param name="t">ゴール地点</param>
		/// <returns>二点間の最短距離。</returns>
		private T DkStr(int s, int t)
		{
			// ダイクストラ法に使う優先付きキュー。
			PriorityQueue<int, T> prQueue = new();

			// BFSは各頂点を一度しか訪問しないので、訪問済みの頂点番号はHashSetに入れて照会する。
			bool[] visited = ArrayPool<bool>.Shared.Rent(vertexCount);

			// スタート地点から各頂点への距離を示す。
			// インデックスが頂点を表して、各要素の値がスタート地点からの距離。
			// 初期処理として配列を最大値で満たす。
			distance.AsSpan().Fill(T.MaxValue);

			// スタート地点をキューに追加する。
			// 重み、こと距離はもちろんゼロ。
			prQueue.Enqueue(s, T.AdditiveIdentity);

			// 次に辺を調査する頂点の番号とその頂点にたどり着くまでにたどってきた距離（優先度）を取り出す。
			while ( prQueue.TryDequeue(out int currentVertex, out T edgeDistance) )
			{

				// もし終了位置に来ていたなら値を返して終わり。
				if ( currentVertex == t )
				{
					// poolに配列を返す。
					ArrayPool<bool>.Shared.Return(visited, true);

					// 現在の頂点までの距離を返す。
					return edgeDistance;
				}

				// 行先の頂点をすでに訪問済みなら(すでにある = より近いパスで訪れているので)飛ばす
				if ( visited[currentVertex] )
				{
					continue;
				}

				// 距離を確定してその頂点を訪れたことにする。
				distance[currentVertex] = edgeDistance;
				visited[currentVertex] = true;

				// キューから次に訪問する頂点を取り出して、その頂点から出ている辺のリストを作成する。
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[currentVertex]);

				// 現在の頂点から繋がる辺の行先を一つずつ見ていく。
				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					// 行先の頂点。
					int toVertex = edgeSpan[i].Item1;

					// キューに辺の総距離を優先度として添えつつ追加する。
					// こうすると距離が近い頂点から吐き出されるということ。
					prQueue.Enqueue(toVertex, edgeDistance + edgeSpan[i].Item2);
				}
			}

			// poolに配列を返す。
			ArrayPool<bool>.Shared.Return(visited, true);

			// 到達できなかったら最小値を返す。
			return T.MinValue;

		}


		/// <summary>
		/// 二点間の最短距離を求めるための幅優先探索。<br/>
		/// スタート地点からグラフ内の全頂点への最短距離を記録したSpanを返すオーバーロード。<br/>
		/// 目的地t に関する条件分岐を消せるので、同じコードの繰り返しにはなるがここで再定義する。
		/// </summary>
		/// <param name="s">スタート地点</param>
		/// <returns>各頂点への最短距離のSpan</returns>
		private Span<T> BFS(int s)
		{
			// 幅優先探索に使うキュー。
			Queue<int> queue = new Queue<int>();

			// BFSは各頂点を一度しか訪問しないので、訪問済みの頂点番号はHashSetに入れて照会する。
			bool[] visited = ArrayPool<bool>.Shared.Rent(vertexCount);

			// スタート地点から各頂点への距離を示す。
			// インデックスが頂点を表して、各要素の値がスタート地点からの距離。
			// 初期処理として配列を最大値で満たす。
			distance.AsSpan().Fill(T.MaxValue);

			// そしてスタート地点にはゼロを入れる。
			distance[s] = T.AdditiveIdentity;

			// スタート地点をキューに追加し、訪問済みにする。
			queue.Enqueue(s);
			visited[s] = true;

			while ( queue.Count > 0 )
			{
				// キューから次に訪問する頂点を取り出して、その頂点から出ている辺のリストを作成する。
				int currentVertex = queue.Dequeue();
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[currentVertex]);

				// 頂点から行先を一つずつ見ていく。
				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					// 行先の頂点。
					int toVertex = edgeSpan[i].Item1;

					// 行先の頂点をすでに訪問済みなら(すでにある = より近いパスで訪れているので)飛ばす
					if ( visited[toVertex] )
					{
						continue;
					}

					// 訪問済みにする。
					visited[toVertex] = true;

					// 行先の頂点は一個プラスした距離になる。
					distance[toVertex] = distance[currentVertex] + T.One;

					// 次はこの頂点から伸びる辺をキューに入れて検査する。
					queue.Enqueue(toVertex);
				}
			}

			// poolに配列を返す。
			ArrayPool<bool>.Shared.Return(visited, true);

			// 距離配列を返す。
			return distance;

		}

		/// <summary>
		/// 二点間の最短距離を求めるためのダイクストラ法。<br/>
		/// 軽く原理を記録しておくと、ある頂点から到達可能な頂点を次々に優先度付きキューに追加して、近い順に頂点が取り出されるようにするというところ。
		/// スタート地点からグラフ内の全頂点への最短距離を記録したSpanを返すオーバーロード。<br/>
		/// 目的地t に関する条件分岐を消せるので、同じコードの繰り返しにはなるがここで再定義する。
		/// </summary>
		/// <param name="s">スタート地点</param>
		/// <returns>各頂点への最短距離のSpan</returns>
		private Span<T> DkStr(int s)
		{
			// ダイクストラ法に使う優先付きキュー。
			PriorityQueue<int, T> prQueue = new();

			// BFSは各頂点を一度しか訪問しないので、訪問済みの頂点番号はHashSetに入れて照会する。
			bool[] visited = ArrayPool<bool>.Shared.Rent(vertexCount);

			// スタート地点から各頂点への距離を示す。
			// インデックスが頂点を表して、各要素の値がスタート地点からの距離。
			// 初期処理として配列を最大値で満たす。
			distance.AsSpan().Fill(T.MaxValue);

			// スタート地点をキューに追加する。
			// 重み、こと距離はもちろんゼロ。
			prQueue.Enqueue(s, T.AdditiveIdentity);

			// 次に辺を調査する頂点の番号とその頂点にたどり着くまでにたどってきた距離（優先度）を取り出す。
			while ( prQueue.TryDequeue(out int currentVertex, out T edgeDistance) )
			{

				// 行先の頂点をすでに訪問済みなら(すでにある = より近いパスで訪れているので)飛ばす
				if ( visited[currentVertex] )
				{
					continue;
				}

				// 距離を確定してその頂点を訪れたことにする。
				distance[currentVertex] = edgeDistance;
				visited[currentVertex] = true;

				// キューから次に訪問する頂点を取り出して、その頂点から出ている辺のリストを作成する。
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[currentVertex]);

				// 現在の頂点から繋がる辺の行先を一つずつ見ていく。
				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					// 行先の頂点。
					int toVertex = edgeSpan[i].Item1;

					// キューに辺の総距離を優先度として添えつつ追加する。
					// こうすると距離が近い頂点から吐き出されるということ。
					prQueue.Enqueue(toVertex, edgeDistance + edgeSpan[i].Item2);
				}
			}

			// poolに配列を返す。
			ArrayPool<bool>.Shared.Return(visited, true);

			// 距離配列を返す。
			return distance;

		}

		#endregion 最小パス

		#region 全パス列挙

		/// <summary>
		/// そのグラフに存在するある地点から<paramref name="target"/>までのパスを全列挙する。<br/>
		/// </summary>
		/// <param name="current">現在の地点。スタート地点であり、どこまで探索したかという値でもある。</param>
		/// <param name="target">目的地。</param>
		/// <param name="visited"></param>
		/// <param name="path"></param>
		/// <param name="paths"></param>
		/// <param name="isFirst">再帰呼び出しではない、最初の呼び出しであるかということ。</param>
		/// <returns></returns>
		private Span<int[]> FindPaths(int current, int target, bool[] visited, Stack<int> path, List<int[]> paths, bool isFirst = true)
		{
			visited[current] = true;
			path.Push(current);

			if ( current == target )
			{
				// 現在のパスを結果に追加
				paths.Add(path.ToArray());
			}
			else
			{
				// キューから次に訪問する頂点を取り出して、その頂点から出ている辺のリストを作成する。
				Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[current]);

				for ( int i = 0; i < edgeSpan.Length; i++ )
				{
					if ( !visited[edgeSpan[i].Item1] )
					{
						FindPaths(edgeSpan[i].Item1, target, visited, path, paths, false);
					}
				}
			}

			// 再帰呼び出しでなければ返す。
			if ( isFirst )
			{
				ArrayPool<bool>.Shared.Return(visited, true);
				return CollectionsMarshal.AsSpan(paths);
			}
			else
			{
				// バックトラック
				path.Pop();
				visited[current] = false;
				return null;
			}
		}


		/// <summary>
		/// vertexEdgesからある頂点から頂点へと向かう辺の重みを取り出すメソッド。
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns>頂点fromからtoへの辺の重み</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetEdgeWeight(int from, int to)
		{
			// その頂点から出ている辺のリストを探索する。
			Span<(int, T)> edgeSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan<(int, T)>(vertexEdges[from]);
			for ( int i = 0; i < edgeSpan.Length; i++ )
			{
				if ( edgeSpan[i].Item1 == to )
				{
					return edgeSpan[i].Item2;
				}
			}
			return T.MinValue;
		}

		#endregion 全パス列挙

		#endregion privateメソッド

	}
}
