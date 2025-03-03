using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAtCoderLibrary
{
    /// <summary>
    /// N個の要素をグループ分けするライブラリ。<br></br>
    /// 各グループ間でのメンバー入れ替え、そしてグループの中身を入れ替える操作を実装。
    /// </summary>
    public class GroupMapping
    {

        /// <summary>
        /// 各メンバーが所属するグループのIDの配列。<br></br>
        /// メンバーの番号から所属グループのIDを返す。
        /// </summary>
        int[] groupOfMember;

        /// <summary>
        /// グループのIDの配列。<br></br>
        /// グループの名前からグループのIDを返す。
        /// </summary>
        int[] nameByID;

        /// <summary>
        /// グループの名前の配列。<br></br>
        /// グループのIDからグループ名を返す。
        /// </summary>
        int[] idByName;

        /// <summary>
        /// コンストラクタでメンバー数を設定する。<br></br>
        /// 最初はメンバーiはグループiに所属している。（無所属はない）
        /// </summary>
        public GroupMapping(int n)
        {
            Contract.Assert(n > 0, $"要素数は0以上である必要がある。");

            groupOfMember = Enumerable.Range(0, n).ToArray();
            nameByID = Enumerable.Range(0, n).ToArray();
            idByName = Enumerable.Range(0, n).ToArray();
        }

        /// <summary>
        /// あるメンバー<paramref name="member"/>の所属を<paramref name="nGrp"/>というグループに移動する。
        /// </summary>
        /// <param name="member">所属を変更するメンバー。</param>
        /// <param name="nGrp">移動先のグループの名前。</param>
        public void BelongUpdate(int member, int nGrp)
        {
            // 配列のインデックスは0からなので1引いた数を使う。
            member--;
            nGrp--;

            // 所属するグループの識別用IDを書き変える。
            groupOfMember[member] = idByName[nGrp];
        }

        /// <summary>
        /// 2つのグループ<paramref name="aGrp"/>と<paramref name="bGrp"/>の間でメンバーを入れ替えるメソッド。
        /// </summary>
        /// <param name="aGrp">入れ替え対象のグループの名前。</param>
        /// <param name="bGrp">もう片方の入れ替え対象のグループの名前。</param>
        public void SwapMember(int aGrp, int bGrp)
        {
            // 配列のインデックスは0からなので1引いた数を使う。
            aGrp--;
            bGrp--;

            // 名前からIDを参照する配列の要素を入れ替える。
            (idByName[aGrp], idByName[bGrp]) = (idByName[bGrp], idByName[aGrp]);

            // IDから名前を参照する配列の要素を入れ替える。
            (nameByID[idByName[aGrp]], nameByID[idByName[bGrp]]) = (nameByID[idByName[bGrp]], nameByID[idByName[aGrp]]);

            // 名前を入れ替えるためにIDが必要なので、先にIDの配列を編集してる。
        }

        /// <summary>
        /// あるメンバー<paramref name="member"/>の所属を返すメソッド。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public int GetBelong(int member)
        {
            // 配列のインデックスはゼロからで、人間が使用する番号振りとの間のギャップを埋めるために足しひきしている。
            return nameByID[groupOfMember[member - 1]] + 1;
        }

    }
}
