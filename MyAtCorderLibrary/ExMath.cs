using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    /// <summary>
    /// 計算用クラス。
    /// 難しい計算をメソッドに起こしておく。
    /// </summary>
    public static class ExMath
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
        /// <paramref name="a"/>と<paramref name="b"/>の最大公約数を返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static T Euclid<T>(T a, T b) where T : System.Numerics.INumber<T>, System.Numerics.IMinMaxValue<T>
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


    }
}
