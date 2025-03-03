using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
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



    }
}
