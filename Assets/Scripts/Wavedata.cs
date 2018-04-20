using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> 波的数据 —— 数据起始的链表节点 和 数据终止的链表节点 </summary>
internal class WaveData : IEnumerable<WaveAttribute> {
    WaveDataNode First;
    WaveDataNode Last;

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

    #region IEnumerable 的实现
    public IEnumerator<WaveAttribute> GetEnumerator() {
        return new WaveDataEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private class WaveDataEnumerator : IEnumerator<WaveAttribute> {
        private WaveData wd;
        private WaveDataNode node = null;

        internal WaveDataEnumerator(WaveData wd) {
            this.wd = wd;
        }

        public WaveAttribute Current {
            get {
                try {
                    return node.Value;
                } catch (NullReferenceException) {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current {
            get { return Current; }
        }

        public bool MoveNext() {
            if (node == null)
                node = wd.First;
            else
                node = node.Next;
            return node != null;
        }

        public void Reset() {
            node = null;
        }

        public void Dispose() {
            // if used and threw exception,
            // please change to provide an empty implementation

            throw new NotImplementedException();
        }
    }
    #endregion

    /// <summary> 波的数据的链表节点 </summary>
    private class WaveDataNode {
        internal WaveDataNode Prevous, Next;
        internal WaveAttribute Value;
    }
}