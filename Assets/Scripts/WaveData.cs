using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 波的数据 —— 数据起始的链表节点 和 数据终止的链表节点 </summary>
internal class WaveData {
    /// <summary> 波形数据的蒙版的列表 </summary> // TODO: 链表？
    private List<WaveDataMask> waveDataMasks;

    /// <summary>
    /// 获取波在横坐标为 x 时的函数值
    /// </summary>
    /// <param name="x"> 求值用到的横坐标 x </param>
    /// <returns> 返回横坐标为 x 时波的函数值 </returns>
    internal float FunctionValueAt(float x) {
        float y = 0;
        // 累加获得 y，每次累加的值为此次循环得到的 WaveAttribute 被蒙版修改后的值
        foreach (WaveDataMask mask in waveDataMasks) {
            WaveAttribute mod = mask.Modification;
            foreach (WaveAttribute wa in mask)
                y += (wa.A * mod.A) *
                Mathf.Sin((wa.Omega * mod.Omega) *
                    (x + wa.Phi + mod.Phi));
        }
        return y;
    }

    #region Maybe usefull functions from laptop
    // /// <summary> 初始化构造函数 </summary>
    // internal WaveData(WaveDataNode First, WaveDataNode Last) {
    //     this.First = First;
    //     this.Last = Last;
    // }

    // /// <summary> 拷贝构造函数 </summary>
    // internal WaveData(WaveData other) {
    //     First = other.First;
    //     Last = other.Last;
    // }

    // /// <param name="firstWA"> 链表要包含的第一个 WaveAttribute </param>
    // /// <param name="list"> 链表要包含的后续所有 WaveAttribute </param>
    // internal WaveData(WaveAttribute firstWA, params WaveAttribute[] list) {
    //     // 初始化首节点和末节点
    //     First = new WaveDataNode { Value = firstWA };
    //     Last = First;

    //     // 如有更多 WaveAttribute 继续添加
    //     foreach (WaveAttribute wa in list)
    //         AddLast(wa);
    // }

    // /// <summary> 在链表最后插入节点 </summary>
    // void AddLast(WaveAttribute wa) {
    //     Last = Last.Next = new WaveDataNode {
    //         Prevous = Last,
    //         Next = null,
    //         Value = wa
    //     };
    // }
    #endregion

    /// <summary> 覆盖并影响连续一串波参数三元组的蒙版 </summary>
    private class WaveDataMask : IEnumerable<WaveAttribute> {
        /// <summary> 蒙版所属波参数节点之首 </summary>
        private WaveDataNode first;
        /// <summary> 蒙版所属波参数节点之尾 </summary>
        private WaveDataNode last;
        /// <summary> 蒙版所记录的对其下所有波参数的修改 </summary>
        internal WaveAttribute Modification;

        #region 以下为 IEnumerable接口 的实现，遍历将会返回蒙版其下!!原始!!的 WaveAttribute
        public IEnumerator<WaveAttribute> GetEnumerator() {
            return new WaveDataMaskEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private class WaveDataMaskEnumerator : IEnumerator<WaveAttribute> {
            private WaveDataMask wdm;
            private WaveDataNode node = null;

            internal WaveDataMaskEnumerator(WaveDataMask wdm) {
                this.wdm = wdm;
            }

            public WaveAttribute Current {
                get { return node.Value; }
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                if (node == null)
                    node = wdm.first;
                else
                    node = node.Next;
                return node != null;
            }

            public void Reset() {
                node = null;
            }

            public void Dispose() { }
        }
        #endregion
    }

    /// <summary> 波的数据的链表节点 </summary>
    private class WaveDataNode {
        internal WaveDataNode Prevous, Next;
        internal WaveAttribute Value;
    }
}