using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MyAtCoderLibrary
{

    #region 計算用定義

    public enum OPERATOR
    {
        加算 = 0,
        減算 = 1,
        乗算 = 2,
        除算 = 3,
        剰余 = 4,
        論理積 = 5,
        論理和 = 6,
        論理排他 = 7,// XOR

    }

    public class Calculator
    {
        public Func<T, T, T>[] calc<T>() where T : INumber<T>
        {
            Func<T, T, T>[] operators = new Func<T, T, T>[3];

            operators[0] = (a, b) => a + b; // 加算
            operators[1] = (a, b) => a - b; // 減算
            operators[2] = (a, b) => a * b; // 乗算

            return operators;
        }

    }

    #endregion 計算用定義

    #region 自作データ型

    /// <summary>
    /// 数値型の値二つのペアの構造体。<br></br>
    /// Vector2に速度で敗北してる。Vector2を拡張メソッドで扱いやすくした方がいいかも
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct MyVector2<T> : IEquatable<MyVector2<T>> where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
    {
        #region 型定義

        /// <summary>
        /// 比較のタイプを定義する列挙型。
        /// </summary>
        public enum SortType
        {
            // 降順をケアして1空けてる。
            標準 = 0,// X→Yの比較
            X要素順 = 1,
            Y要素順 = 2,
            平面座標_線の長さ順 = 3,
        }

        #endregion 型定義



        /// <summary>
        /// 一つ目の要素。
        /// </summary>
        public T x;

        /// <summary>
        /// 二つ目の要素
        /// </summary>
        public T y;

        #region コンストラクタ

        /// <summary>
        /// 通常のコントラクタ。
        /// </summary>
        /// <param name="value1">xの値</param>
        /// <param name="value2">yの値</param>
        public MyVector2(T value1, T value2)
        {
            x = value1;
            y = value2;
        }

        /// <summary>
        /// 値の大小で入れる値を変更するコントラクタ。
        /// </summary>
        /// <param name="value1">一つ目の値</param>
        /// <param name="value2">二つ目の値</param>
        /// <param name="isLargeX">Xに大きい方の値を入れるか。真なら大きい、偽なら小さい</param>
        public MyVector2(T value1, T value2, bool isLargeX)
        {
            // 大きい方をXに入れるか、小さい方をxに入れるかを判断する。
            if ( value1 > value2 == isLargeX )
            {
                x = value1;
                y = value2;
            }
            else
            {
                y = value1;
                x = value2;
            }
        }

        /// <summary>
        /// デフォルト値のコンストラクタ
        /// </summary>
        public MyVector2()
        {
            x = T.Zero;
            y = T.Zero;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="other"></param>
        public MyVector2(MyVector2<T> other)
        {
            x = other.x;
            y = other.y;
        }

        #endregion コンストラクタ

        #region プロパティ

        /// <summary>
        /// (0,0)の座標を表す原点ベクトルを取得します。
        /// </summary>
        public static MyVector2<T> Zero
        {
            get => default;
        }

        /// <summary>
        /// (1,0)の座標を表す右方向の単位ベクトルを取得します。
        /// </summary>
        public static MyVector2<T> Right
        {
            get => new MyVector2<T>(T.One, T.Zero);
        }

        /// <summary>
        /// (-1,0)の座標を表す左方向の単位ベクトルを取得します。
        /// </summary>
        public static MyVector2<T> Left
        {
            get => new MyVector2<T>(-T.One, T.Zero);
        }

        /// <summary>
        /// (0,1)の座標を表す上方向の単位ベクトルを取得します。
        /// </summary>
        public static MyVector2<T> Up
        {
            get => new MyVector2<T>(T.Zero, T.One);
        }

        /// <summary>
        /// (0,-1)の座標を表す下方向の単位ベクトルを取得します。
        /// </summary>
        public static MyVector2<T> Down
        {
            get => new MyVector2<T>(T.Zero, -T.One);
        }

        /// <summary>
        /// 各要素が型Tの最大値である(MaxValue,MaxValue)座標のベクトルを取得します。
        /// </summary>
        public static MyVector2<T> MaxValue
        {
            get => new MyVector2<T>(T.MaxValue, T.MaxValue);
        }

        /// <summary>
        /// 各要素が型Tの最小値である(MinValue,MinValue)座標のベクトルを取得します。
        /// </summary>
        public static MyVector2<T> MinValue
        {
            get => new MyVector2<T>(T.MinValue, T.MinValue);
        }
        #endregion プロパティ

        /// <summary>
        /// オプション付きソート用メソッド
        /// </summary>
        /// <param name="vectors">ソートする対象のSpan</param>
        /// <param name="sType">ソートタイプ。</param>
        /// <param name="isDescending">真なら降順とする</param>
        public static void Sort(Span<MyVector2<T>> vectors, SortType sType = SortType.標準, bool isDescending = false)
        {
            // 比較用デリゲート。ComparisonはActionみたいな感じの既定デリゲートで、比較に使うぽい。
            Comparison<MyVector2<T>> comparison = null;

            if ( sType == SortType.標準 )
            {
                // 昇順
                if ( !isDescending )
                {
                    // 標準はx→y順に比較する。
                    comparison = (a, b) =>
                    {
                        // xの値で比較し、決着がつかなければyで比較。
                        int xComparison = Comparer<T>.Default.Compare(a.x, b.x);
                        return xComparison != 0
                            ? xComparison
                            : Comparer<T>.Default.Compare(a.y, b.y);
                    };
                }
                // 降順
                else
                {
                    comparison = (a, b) =>
                    {
                        int xComparison = Comparer<T>.Default.Compare(b.x, a.x); // 降順なのでbとaを入れ替え
                        return xComparison != 0
                            ? xComparison
                            : Comparer<T>.Default.Compare(b.y, a.y); // 降順なのでbとaを入れ替え
                    };
                }
            }
            else if ( sType == SortType.X要素順 )
            {
                // 昇順
                if ( !isDescending )
                {
                    // xの値で比較
                    comparison = (a, b) =>
                    {
                        int xComparison = Comparer<T>.Default.Compare(a.x, b.x);
                        return xComparison;
                    };
                }
                // 降順
                else
                {
                    // xの値で比較
                    comparison = (a, b) =>
                    {
                        int xComparison = Comparer<T>.Default.Compare(b.x, a.x);// 降順なのでbとaを入れ替え
                        return xComparison;
                    };
                }
            }
            else if ( sType == SortType.Y要素順 )
            {
                // 昇順
                if ( !isDescending )
                {
                    // yの値で比較
                    comparison = (a, b) =>
                    {
                        int yComparison = Comparer<T>.Default.Compare(a.y, b.y);
                        return yComparison;
                    };
                }
                // 降順
                else
                {
                    // yの値で比較
                    comparison = (a, b) =>
                    {
                        int yComparison = Comparer<T>.Default.Compare(b.y, a.y);// 降順なのでbとaを入れ替え
                        return yComparison;
                    };
                }
            }
            else// if ( sType == SortType.平面座標_線の長さ順 )
            {
                // 昇順
                if ( !isDescending )
                {
                    // 計算量を考慮してマンハッタン距離で比較
                    comparison = (a, b) =>
                    {
                        int distanceComparison = Comparer<T>.Default.Compare(a.ManhattanDistance(), b.ManhattanDistance());
                        return distanceComparison;
                    };
                }
                // 降順
                else
                {
                    // 計算量を考慮してマンハッタン距離で比較
                    comparison = (a, b) =>
                    {
                        int distanceComparison = Comparer<T>.Default.Compare(b.ManhattanDistance(), a.ManhattanDistance());// 降順なのでbとaを入れ替え
                        return distanceComparison;
                    };
                }
            }

            // Sort実行。
            vectors.Sort(comparison);
        }

        #region 演算子

        /// <summary>
        /// 2つのベクトルを加算します。
        /// </summary>
        /// <param name="left">加算する1つ目のベクトル</param>
        /// <param name="right">加算する2つ目のベクトル</param>
        /// <returns>加算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator +(in MyVector2<T> left, in MyVector2<T> right)
        {
            // 性能重視のため直接計算
            return new MyVector2<T>(left.x + right.x, left.y + right.y);
        }

        /// <summary>
        /// 1つ目のベクトルを2つ目のベクトルで要素ごとに除算します。
        /// </summary>
        /// <param name="left">被除数のベクトル</param>
        /// <param name="right">除数のベクトル</param>
        /// <returns>除算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator /(in MyVector2<T> left, in MyVector2<T> right)
        {
            return new MyVector2<T>(left.x / right.x, left.y / right.y);
        }

        /// <summary>
        /// ベクトルをスカラー値で除算します。
        /// </summary>
        /// <param name="vector">被除数のベクトル</param>
        /// <param name="scalar">除数のスカラー値</param>
        /// <returns>除算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator /(in MyVector2<T> vector, T scalar)
        {
            return new MyVector2<T>(vector.x / scalar, vector.y / scalar);
        }

        /// <summary>
        /// 2つのベクトルが等しいかどうかを判定します。
        /// </summary>
        /// <param name="left">比較する1つ目のベクトル</param>
        /// <param name="right">比較する2つ目のベクトル</param>
        /// <returns>両方のベクトルの要素が等しい場合はtrue、そうでない場合はfalse</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in MyVector2<T> left, in MyVector2<T> right)
        {
            return (left.x == right.x) && (left.y == right.y);
        }

        /// <summary>
        /// 2つのベクトルが等しくないかどうかを判定します。
        /// </summary>
        /// <param name="left">比較する1つ目のベクトル</param>
        /// <param name="right">比較する2つ目のベクトル</param>
        /// <returns>いずれかの要素が異なる場合はtrue、すべて等しい場合はfalse</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in MyVector2<T> left, in MyVector2<T> right)
        {
            return (left.x != right.x) || (left.y != right.y);
        }

        /// <summary>
        /// 2つのベクトルを要素ごとに乗算します。
        /// </summary>
        /// <param name="left">乗算する1つ目のベクトル</param>
        /// <param name="right">乗算する2つ目のベクトル</param>
        /// <returns>乗算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator *(in MyVector2<T> left, in MyVector2<T> right)
        {
            return new MyVector2<T>(left.x * right.x, left.y * right.y);
        }

        /// <summary>
        /// ベクトルをスカラー値で乗算します。
        /// </summary>
        /// <param name="vector">乗算するベクトル</param>
        /// <param name="scalar">乗算するスカラー値</param>
        /// <returns>乗算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator *(in MyVector2<T> vector, T scalar)
        {
            return new MyVector2<T>(vector.x * scalar, vector.y * scalar);
        }

        /// <summary>
        /// スカラー値とベクトルを乗算します。
        /// </summary>
        /// <param name="scalar">乗算するスカラー値</param>
        /// <param name="vector">乗算するベクトル</param>
        /// <returns>乗算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator *(T scalar, in MyVector2<T> vector)
        {
            return new MyVector2<T>(vector.x * scalar, vector.y * scalar);
        }

        /// <summary>
        /// 1つ目のベクトルから2つ目のベクトルを減算します。
        /// </summary>
        /// <param name="left">被減数のベクトル</param>
        /// <param name="right">減数のベクトル</param>
        /// <returns>減算結果のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator -(in MyVector2<T> left, in MyVector2<T> right)
        {
            return new MyVector2<T>(left.x - right.x, left.y - right.y);
        }

        /// <summary>
        /// ベクトルの符号を反転します。
        /// </summary>
        /// <param name="vector">符号を反転するベクトル</param>
        /// <returns>符号が反転したベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> operator -(in MyVector2<T> vector)
        {
            return new MyVector2<T>(-vector.x, -vector.y);
        }

        #endregion 演算子

        #region 基礎機能（Equals()など）

        /// <summary>
        /// このベクトルと指定されたベクトルが等しいかどうかを判定します。
        /// </summary>
        /// <param name="other">比較するベクトル</param>
        /// <returns>両方のベクトルの要素が等しい場合はtrue、そうでない場合はfalse</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(MyVector2<T> other)
        {
            return (x == other.x) && (y == other.y);
        }

        /// <summary>
        /// このベクトルと指定されたオブジェクトが等しいかどうかを判定します。
        /// </summary>
        /// <param name="obj">比較するオブジェクト</param>
        /// <returns>objがMyVector2で、要素が等しい場合はtrue、そうでない場合はfalse</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object? obj)
        {
            return (obj is Vector2 other) && Equals(other);
        }

        /// <summary>
        /// ハッシュセットとかで使うハッシュ値を作る。<br></br>
        /// これがないと普段使いは無理なレベルで重くなるから今後も注意。<br></br>
        /// ゲームのデータ型もDictionaryとか使うならやった方がいいな。
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => HashCode.Combine(x, y);


        #endregion 基礎機能（Equals()など）


        #region 計算用メソッド。高速化したいときに使う。演算子より速い。

        /// <summary>
        /// 足し算メソッド。
        /// </summary>
        /// <param name="other">計算対象のベクトル</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in MyVector2<T> other)
        {
            x += other.x;
            y += other.y;
        }

        /// <summary>
        /// 引き算メソッド。
        /// </summary>
        /// <param name="other">計算対象のベクトル</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtraction(in MyVector2<T> other)
        {
            x -= other.x;
            y -= other.y;
        }

        /// <summary>
        /// かけ算メソッド。
        /// </summary>
        /// <param name="other">計算対象のベクトル</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiplication(in MyVector2<T> other)
        {
            x *= other.x;
            y *= other.y;
        }

        /// <summary>
        /// 割り算メソッド。
        /// </summary>
        /// <param name="other">計算対象のベクトル</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Divide(in MyVector2<T> other)
        {
            x /= other.x;
            y /= other.y;
        }

        #endregion 計算用メソッド。

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> Add(MyVector2<T> left, MyVector2<T> right)
        {
            return new MyVector2<T>(left.x + right.x, left.y + right.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MyVector2<T> Subtract(MyVector2<T> left, MyVector2<T> right)
        {
            return new MyVector2<T>(left.x - right.x, left.y - right.y);
        }
    }

    #endregion 自作データ型

    /// <summary>
    /// 計算用クラス。
    /// 難しい計算をメソッドに起こしておく。
    /// </summary>
    public static class MathExtension
    {

        /// <summary>
        /// 時間経過による円上でのポイントを求めることができる。<br/>
        /// あくまで 2D なので円がXY平面かZY平面のどちらにあるかでxとzを互換可能。<br/>
        /// あとXZ平面なんてものがあればZがYに互換される。
        /// </summary>
        /// <param name="radius">円の半径</param>
        /// <param name="time">現在の時間</param>
        /// <param name="lapTime">円を一周するのに必要な時間</param>
        /// <returns></returns>
        public static (double, double) CirclePoint2D(double radius, double time, double lapTime)
        {
            double cx = radius - radius * Math.Cos(time / lapTime * 2.0 * Math.PI);
            double cy = -radius * Math.Sin(time / lapTime * 2.0 * Math.PI);

            return (cx, cy);
        }

        /// <summary>
        /// 時間経過による円上でのポイントを求めることができる。<br/>
        /// あくまで 2D なので円がXY平面かZY平面のどちらにあるかでxとzを互換可能。<br/>
        /// float型で計算するオーバーロード。計算量が多そうなときにでもつかう？
        /// </summary>
        /// <param name="radius">円の半径</param>
        /// <param name="time">現在の時間</param>
        /// <param name="lapTime">円を一周するのに必要な時間</param>
        /// <returns></returns>
        public static (float, float) CirclePoint2D(float radius, float time, float lapTime)
        {
            float cx = (float)(radius - radius * Math.Cos(time / lapTime * 2.0 * Math.PI));
            float cy = (float)(-radius * Math.Sin(time / lapTime * 2.0 * Math.PI));

            return (cx, cy);
        }

        /// <summary>
        /// <paramref name="a"/>と<paramref name="b"/>の最大公約数を返す。<br/>
        /// ちなみに最小公倍数は最大公約数でa*bを割ればわかる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static T EuclidGcd<T>(T a, T b) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            // 大きい方をaに入れる。
            if ( b > a )
            {
                (a, b) = (b, a);
            }

            while ( true )
            {
                // 次のbはあまり、次のaはb。
                T b2 = a % b;
                a = b;

                // 割ったあまりゼロならおわり。
                if ( b2 == T.AdditiveIdentity )
                {
                    return b;
                }

                b = b2;

            }
        }

        /// <summary>
        /// 等比数列の総和を求める。
        /// 初項<paramref name="a"/>、等比<paramref name="r"/>、項数<paramref name="n"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        //public static T GeometricSum<T>(T a, T r, T n) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        //{
        //    return a * ((r.Pow(r, n) - T.One) / (r - T.One));
        //}

        /// <summary>
        /// Tの型の計算用Funcを返すメソッド。<br/>
        /// ライブラリに設定から動的に計算の内容を変更したいときに使う。<br/>
        /// 注意として、型判別部分ではintとuintを区別できない。typeOfは遅そうなので控えた。
        /// </summary>
        /// <typeparam name="T">返す計算メソッド。Func<T,T,T>への明示キャスト必須。 </typeparam>
        /// <returns></returns>
        public static Delegate GetCalculator<T>(OPERATOR op) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            if ( op == OPERATOR.加算 )
            {
                return (T a, T b) => a + b;
            }
            else if ( op == OPERATOR.減算 )
            {
                return (T a, T b) => a - b;
            }
            else if ( op == OPERATOR.乗算 )
            {
                return (T a, T b) => a * b;
            }
            else if ( op == OPERATOR.除算 )
            {
                return (T a, T b) => a / b;
            }
            else if ( op == OPERATOR.論理積 )
            {
                unsafe
                {
                    int size = Unsafe.SizeOf<T>();

                    if ( size == Unsafe.SizeOf<byte>() )
                    {
                        return (byte a, byte b) => a & b;
                    }
                    else if ( size == Unsafe.SizeOf<int>() )
                    {
                        return (int a, int b) => a & b;
                    }
                    else if ( size == Unsafe.SizeOf<long>() )
                    {
                        return (long a, long b) => a & b;
                    }
                    else
                    {
                        throw new ArgumentException("無効な型指定");
                    }
                }
            }
            else if ( op == OPERATOR.論理和 )
            {
                unsafe
                {
                    int size = Unsafe.SizeOf<T>();

                    if ( size == Unsafe.SizeOf<byte>() )
                    {
                        return (byte a, byte b) => a | b;
                    }
                    else if ( size == Unsafe.SizeOf<int>() )
                    {
                        return (int a, int b) => a | b;
                    }
                    else if ( size == Unsafe.SizeOf<long>() )
                    {
                        return (long a, long b) => a | b;
                    }
                    else
                    {
                        throw new ArgumentException("無効な型指定");
                    }
                }
            }
            else if ( op == OPERATOR.論理排他 )
            {
                unsafe
                {
                    int size = Unsafe.SizeOf<T>();

                    if ( size == Unsafe.SizeOf<byte>() )
                    {
                        return (byte a, byte b) => a ^ b;
                    }
                    else if ( size == Unsafe.SizeOf<int>() )
                    {
                        return (int a, int b) => a ^ b;
                    }
                    else if ( size == Unsafe.SizeOf<long>() )
                    {
                        return ((long a, long b) => a ^ b);
                    }
                    else
                    {
                        throw new ArgumentException("無効な型指定");
                    }
                }
            }

            return null;
        }


        #region Vector関連

        #region 計算関数

        /// <summary>
        /// 原点からのマンハッタン距離を割り出す。
        /// </summary>
        /// <param name="vec">対象のVector</param>
        /// <returns>マンハッタン距離</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ManhattanDistance<T>(this MyVector2<T> vec) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            return (T.Abs(vec.x) + T.Abs(vec.y));
        }

        /// <summary>
        /// x^2+y^2の距離を割り出す。
        /// </summary>
        /// <param name="vec">対象のVector</param>
        /// <returns>平方根してない距離</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MagnitudeDistance<T>(this MyVector2<T> vec) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            return (vec.x * vec.x) + (vec.y * vec.y);
        }

        /// <summary>
        /// x^2+y^2の距離を割り出す。
        /// </summary>
        /// <param name="vec">対象のVector</param>
        /// <returns>平方根してない距離</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Distance<T>(this MyVector2<T> vec, MyVector2<T> other) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            T x = (vec.x - other.x);
            T y = (vec.y - other.y);
            return x * x + y * y;
        }

        /// <summary>
        /// 絶対値を割り出す拡張メソッド。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vec"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs<T>(this MyVector2<T> vec) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
        {
            vec.x = T.Abs(vec.x);
            vec.y = T.Abs(vec.y);
        }

        #endregion 計算関数

        #endregion Vector関連

        #region 配列操作

        /// <summary>
        /// System.Numerics.Vectorを使用して配列の全要素に値を足す（int配列専用）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddValueVector(this int[] array, int value)
        {
            if ( array == null || array.Length == 0 )
                return;

            // Vector<int>で処理できる要素数
            int vectorSize = Vector<int>.Count;

            // 足す値をVectorに複製
            Vector<int> valueVector = new Vector<int>(value);

            int i = 0;

            // ベクトル処理できる部分を処理
            if ( array.Length >= vectorSize )
            {
                // 処理対象の終了インデックス（ベクトルサイズの倍数まで）
                int vectorizedEnd = array.Length - (array.Length % vectorSize);

                // ベクトル化処理
                for ( ; i < vectorizedEnd; i += vectorSize )
                {
                    // 配列から現在位置のベクトルを取得
                    Vector<int> vec = new Vector<int>(array, i);

                    // 加算
                    vec += valueVector;

                    // 結果を配列に書き戻す
                    vec.CopyTo(array, i);
                }
            }

            // 残りの要素を通常処理
            for ( ; i < array.Length; i++ )
            {
                array[i] += value;
            }
        }


        /// <summary>
        /// ジェネリック版の全要素加算（あらゆる数値型に対応）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddValueVector<T>(this T[] array, T value) where T : struct, INumber<T>
        {
            if ( array == null || array.Length == 0 )
                return;

            int vectorSize = Vector<T>.Count;
            Vector<T> valueVector = new Vector<T>(value);

            int i = 0;

            // ベクトル処理
            if ( array.Length >= vectorSize )
            {
                int vectorizedEnd = array.Length - (array.Length % vectorSize);

                for ( ; i < vectorizedEnd; i += vectorSize )
                {
                    Vector<T> vec = new Vector<T>(array, i);
                    vec += valueVector;
                    vec.CopyTo(array, i);
                }
            }

            // 残りの要素
            for ( ; i < array.Length; i++ )
            {
                array[i] += value;
            }
        }

        /// <summary>
        /// System.Numerics.Vectorを使用して配列の全要素の値を更新する（int配列専用）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UpdateValueVector(this int[] array, int value)
        {
            if ( array == null || array.Length == 0 )
                return;

            // Vector<int>で処理できる要素数
            int vectorSize = Vector<int>.Count;

            // 上書きする値をVectorに複製
            Vector<int> valueVector = new Vector<int>(value);

            int i = 0;

            // ベクトル処理できる部分を処理
            if ( array.Length >= vectorSize )
            {
                // 処理対象の終了インデックス（ベクトルサイズの倍数まで）
                int vectorizedEnd = array.Length - (array.Length % vectorSize);

                // ベクトル化処理
                for ( ; i < vectorizedEnd; i += vectorSize )
                {
                    // 値を配列に上書きする
                    valueVector.CopyTo(array, i);
                }
            }

            // 残りの要素を通常処理
            for ( ; i < array.Length; i++ )
            {
                array[i] = value;
            }
        }

        #endregion 配列操作

    }
}
