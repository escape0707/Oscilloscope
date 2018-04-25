using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 波的数据 —— 数据起始的链表节点 和 数据终止的链表节点 </summary>
internal class WaveData {
    private List<WaveDataMask> waveDataMasks;

    /// <summary>
    /// 获取波在横坐标为 x 时的函数值
    /// </summary>
    /// <param name="x"> 求值用到的横坐标 x </param>
    /// <returns> 返回横坐标为 x 时波的函数值 </returns>
    internal float FunctionValueAt(float x) {
        float y = 0;
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

    private class WaveDataMask : IEnumerable<WaveAttribute> {
        private WaveDataNode first;
        private WaveDataNode last;
        internal WaveAttribute Modification;

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
    }

    /// <summary> 波的数据的链表节点 </summary>
    private class WaveDataNode {
        internal WaveDataNode Prevous, Next;
        internal WaveAttribute Value;
    }

    /// <summary> 代表一条正弦波的参数组，由三个 float: a, omega, phi 组成 </summary>
    private class WaveAttribute {
        internal float A, Omega, Phi;

        internal WaveAttribute(float a = 1, float omega = 1, float phi = 0) {
            this.A = a;
            this.Omega = omega;
            this.Phi = phi;
        }
    }
}