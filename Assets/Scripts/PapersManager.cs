using System.Collections.Generic;
using UnityEngine;

/// <summary> 波的数据类型 </summary>
using WaveData = System.Collections.Generic.LinkedList<WaveController.WaveAttribute>;
/// <summary> 代表一条正弦波的参数组，由 三个float: a, omega, phi 组成 </summary>
using WaveAttribute = WaveController.WaveAttribute;

/// <summary>
/// 管理板子上纸片的生成、展示、修改等操作，
/// 存储数据结构是多叉树。
/// </summary>
public class PapersManager : MonoBehaviour {

    /// <summary> 纸片 <c>Prefab</c> </summary>
    public GameObject paperPrefab;
    /// <summary> 纸片组最左上点的位置 </summary>
    public Vector3 topLeftPosition;
    /// <summary> PapersManager's Singleton </summary>
    internal static PapersManager instance = null;
    /// <summary> 纸片组的列数 </summary>
    private const int columns = 3;
    /// <summary> 纸片间的间隔 </summary>
    private const float spacingX = .5f;
    private const float spacingY = .5f;
    private const float testRatio = 0.8f;


    /// <summary> 纸片对应的树中的节点 </summary>
    private class PaperNode {
        /// <summary> 父节点 </summary>
        internal PaperNode father;
        /// <summary> 子节点的链表 </summary>
        internal LinkedList<PaperNode> children;
        /// <summary> 当前节点的波形数据 </summary>
        private WaveData data;
        /// <summary> 节点对应纸片的脚本 </summary>
        private WaveController waveScript;
        // /// <summary> 懒标签：是否仍有“修改”未被下放到当前节点的子节点中去 </summary>
        // internal bool lazy = false;
        // /// <summary> 对于当前节点的、对波形数据的修改 </summary>
        // internal WaveAttribute changes;

        /// <summary> 空白默认构造函数 </summary>
        internal PaperNode() { }

        /// <summary> 按照 other.data 生成一个克隆 </summary>
        internal PaperNode(PaperNode other) {
            data = new WaveData(other.data);
        }

        /// <param name="wa"> 用来初始化 data 的 一个WaveAttribute  </param>
        internal PaperNode(WaveAttribute wa) {
            data = new WaveData();
            data.AddLast(wa);
        }

        /// <summary> 实例化纸片 </summary>
        /// <param name="positionIndex"> 新纸片的位置 </param>
        internal void InstantiatePaperAt(Vector3 position) {
            // 实例化纸片，命名，连接好节点到 WaveController 的对应，连接好纸片的 waveData
            GameObject paper = Instantiate(
                PapersManager.instance.paperPrefab,
                position,
                Quaternion.identity,
                PapersManager.instance.papersHolderTransform
            );
            paper.name = (nodeCount++).ToString();
            waveScript = paper.GetComponent<WaveController>();
            waveScript.waveData = data;
        }

        /// <summary> 析构纸片 </summary>
        internal void DestroyPaper() {
            Destroy(waveScript.gameObject);
        }

        /// <summary> 重设纸片位置 </summary>
        /// <param name="position"> 纸片的新位置 </param>
        internal void RepositionPaperTo(Vector3 position) {
            waveScript.transform.localPosition = position;
        }

        /// <summary> 合并节点的 data </summary>
        /// <param name="other"> 被合并 data 的节点 </param>
        internal void MergeDataWith(PaperNode other) {
            // var start = to.data.Last;
            foreach (var waveAttribute in other.data)
                data.AddLast(waveAttribute);
            // start = start.Next;
            // var end = to.data.Last;
        }

        /// <summary> 刷新波形 </summary>
        internal void RefreshWave() {
            waveScript.Refresh();
        }

        /// <summary> 总共创建过的节点的总数 </summary>
        private static int nodeCount = 0;
    }

    /// <summary> 用于在 Hierarchy 中收纳众多节点的 Holder 的 Transform </summary>
    private Transform papersHolderTransform;
    /// <summary> 多叉树的根节点 </summary>
    private PaperNode rootNode;
    /// <summary> 当前被展开观察节点 </summary>
    private PaperNode expandedNode;

    /// <summary> 初始化 Hierarchy 中的 Holder </summary>
    private void InitialzieHolder() {
        papersHolderTransform = new GameObject("Papers").transform;
        papersHolderTransform.position = topLeftPosition;
        expandedNode = rootNode = new PaperNode {
            children = new LinkedList<PaperNode>()
        };
    }

    // 计算屏幕上 第count张 纸片应有的 localPosition
    private static Vector3 CalcPosition(int count) {
        float x = count % columns * (WaveController.paperWeight + spacingX);
        float y = count / columns * -(WaveController.paperHeight + spacingY);
        return new Vector3(x, y, 0);
    }

    // 备注：所有的操作必须以 PaperNode 为中心，以 WaveController 和纸片为外挂。
    //       否则会将一个可以单向映射的问题，复杂为双向映射。

    /// <summary> 重新整理并更新纸片们的位置 </summary>
    /// <param name="LLNode">整理开始的位置（expandedNode.children.LinkedListNode）</param>
    private void RepositionPapersStartFrom(LinkedListNode<PaperNode> LLNode) {
        int count = 0;
        for (var i = expandedNode.children.First; i != LLNode; i = i.Next)
            ++count;
        for (var i = LLNode; i != null; i = i.Next)
            i.Value.RepositionPaperTo(CalcPosition(count++));
    }

    /// <summary> 创建新节点 </summary>
    private void CreateNode() {
        // 新建一个节点，将新节点的波形数据设为 y = sin(x)
        PaperNode newNode = new PaperNode(new WaveAttribute(WaveController.paperHeight * testRatio / 2, 3));

        // 记录新节点的父节点
        newNode.father = expandedNode;

        // 生成对应纸片
        newNode.InstantiatePaperAt(CalcPosition(expandedNode.children.Count));

        // 将新节点添加到 当前展开节点expandedNode 的 子节点列表children 中
        expandedNode.children.AddLast(newNode);
    }

    /// <summary> 根据节点删除纸片 </summary>
    /// <param name="LLNode"> 被删除的纸片在所处 children 中对应的 LinkedListNode </param>
    /// <remarks> 如果这个节点在其他地方有引用，则根据 C# 的垃圾回收规则，该节点的数据不会被彻底删除；
    /// 反之，当没有任何一处引用指向这个节点时，即其实我们永远不能找回这个节点时，该节点会被自动垃圾回收。
    /// 本例中，当一个非 rootNode 的节点没有父节点时，它才会被回收。
    /// 这样恰好也方便了合并操作。 </remarks>
    private void DeleteNode(LinkedListNode<PaperNode> LLNode) {
        // 存下 LLNode.Next 以便后面重设纸片位置
        var next = LLNode.Next;

        // 从父节点（当前被展开观察节点）的孩子列表中删除
        expandedNode.children.Remove(LLNode);

        // 析构 paper 对象  P.S.: C# 真奇怪，留 Value 不留 Next。。。。
        LLNode.Value.DestroyPaper();

        // 重新整理并更新纸片们的位置
        if (next != null)
            RepositionPapersStartFrom(next);
    }

    /// <summary> 合并节点 </summary>
    /// <param name="fromNode"> 被拖动的纸片在原所处 children 中对应的 LinkedListNode </param>
    /// <param name="toNode"> 被添加的纸片在所处 children 中对应的 LinkedListNode </param>
    private void MergeNodes(LinkedListNode<PaperNode> fromNode, LinkedListNode<PaperNode> toNode) {
        PaperNode from = fromNode.Value;
        PaperNode to = toNode.Value;

        // 如果被合并节点是叶子节点，晋升为一个纸片组的代表（非叶子节点）
        if (to.children == null) {
            to.children = new LinkedList<PaperNode>();
            // 生成一个有相同 data 的克隆，添到子节点
            PaperNode clone = new PaperNode(to);
            clone.father = to;
            to.children.AddFirst(clone);
        }

        // 被并购数据，认人为父
        to.MergeDataWith(from);
        to.RefreshWave();
        from.father = to;
        to.children.AddLast(from);

        // 不堪其辱：“我选择死亡！”
        // 注： from 仍在 to.children 中有引用，故数据不会被删除，
        //      只会 Destroy 对应纸片、重排、并从原父节点下移除。
        DeleteNode(fromNode);

    }

    /// <summary> 展开（观察）节点 </summary>
    /// <param name="node"> 要展开的纸片对应的 PaperNode </param>
    private void ExpandNode(PaperNode node) {
        // 析构当前可见纸片
        foreach (var child in expandedNode.children)
            child.DestroyPaper();

        // 将 LLNode 设置为 展开的纸片expandedNode
        expandedNode = node;

        // 生成新一波纸片
        int count = 0;
        foreach (var child in expandedNode.children)
            child.InstantiatePaperAt(CalcPosition(count++));
    }

    /// <summary> 折叠正在观察的节点并返回上一层 </summary>
    private void CollapseNode() {
        ExpandNode(expandedNode.father);
    }


    void OnGUI() {
        // Make a background box
        GUI.Box(new Rect(10, 10, 100, 180), "PapersManager");

        // Make the first button. If it is pressed, create a new papar
        if (GUI.Button(new Rect(20, 40, 80, 20), "Create")) {
            Debug.Log("Creating...");
            CreateNode();
            Debug.Log("Created");
        }

        // Make the second button. If it is pressed, delete the first and the last paper
        if (GUI.Button(new Rect(20, 70, 80, 20), "Delete")) {
            Debug.Log("Deleting...");
            if (expandedNode.children.Count == 0)
                Debug.Log("Already empty!");
            else {
                DeleteNode(expandedNode.children.First);
                if (expandedNode.children.Count == 0)
                    Debug.Log("Only deleted one!");
                else {
                    DeleteNode(expandedNode.children.Last);
                    Debug.Log("Deleted");
                }
            }
        }

        // Make the third button. If it is pressed, merge from the first to the last paper
        if (GUI.Button(new Rect(20, 100, 80, 20), "Merge")) {
            Debug.Log("Merging...");
            if (expandedNode.children.Count < 2)
                Debug.Log("Not enough nodes!");
            else {
                MergeNodes(expandedNode.children.First, expandedNode.children.Last);
                Debug.Log("Merged");
            }
        }

        // Make the forth button. If it is pressed, expand the last paper
        if (GUI.Button(new Rect(20, 130, 80, 20), "Expand")) {
            Debug.Log("Expanding");
            try {
                PaperNode lastChild = expandedNode.children.Last.Value;
                if (lastChild.children.Count < 2)
                    Debug.Log("Not enough children!");
                else {
                    ExpandNode(lastChild);
                    Debug.Log("Expanded");
                }
            } catch (System.NullReferenceException) {
                Debug.Log("No child!");
            }
        }

        // Make the fifth button. If it is pressed, collapse and revert to previous view
        if (GUI.Button(new Rect(20, 160, 80, 20), "Collapse")) {
            Debug.Log("Collapsing");
            if (expandedNode.father == null)
                Debug.Log("Already at rootNode!");
            else {
                CollapseNode();
                Debug.Log("Collapsed");
            }
        }
    }

    void Awake() { // TODO
        InitialzieHolder();
    }
}