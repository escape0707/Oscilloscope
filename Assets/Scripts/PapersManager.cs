using System.Collections;
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
    /// <summary> 纸片组的列数 </summary>
    public int columns = 5;
    /// <summary> 纸片间的间隔 </summary>
    public float spacingX = .5f, spacingY = .5f;
    

    /// <summary> 纸片对应的树中的节点 </summary>
    private class PaperNode {
        /// <summary> 当前节点的波形数据 </summary>
        internal WaveData data;
        /// <summary> 节点对应纸片的脚本 </summary>
        internal WaveController waveScript;
        /// <summary> 子节点的链表 </summary>  
        internal LinkedList<PaperNode> children;
        // /// <summary> 懒标签：是否仍有“修改”未被下放到当前节点的子节点中去 </summary>  
        // internal bool lazy = false;
        // /// <summary> 对于当前节点的、对波形数据的修改 </summary>  
        // internal WaveAttribute changes;
    }
    /// <summary> 用于在 Hierarchy 中收纳众多节点的 Holder 的 Transform </summary>
    private Transform papersHolderTransform;
    /// <summary> 多叉树的根节点 </summary>  
    private PaperNode rootNode;
    /// <summary> 当前被展开观察节点 </summary>  
    private PaperNode expandedNode;

    /// <summary> 初始化 Hierarchy 中的 Holder </summary>
    void InitialzieHolder() {
        papersHolderTransform = new GameObject("Papers").transform;
        papersHolderTransform.position = topLeftPosition;
        expandedNode = rootNode = new PaperNode {
            children = new LinkedList<PaperNode>()
        };
    }

    // 计算屏幕上 第count张 纸片应有的 localPosition
    Vector3 CalcPosition(int count) {
        float x = count % columns *  (WaveController.paperWeight + spacingX);
        float y = count / columns * -(WaveController.paperHeight + spacingY);
        return new Vector3(x, y, 0);
    }

    // 备注：所有的操作必须以 PaperNode 为中心，以 WaveController 和纸片为外挂。
    //       否则会将一个可以单向映射的问题，复杂为双向映射。

    /// <summary> 创建新纸片 </summary>
    void CreateNode() {
        // 新建一个节点，将新节点的波形数据设为 y = sin(x) 
        PaperNode newNode = new PaperNode {
            data = new WaveData(),
            children = new LinkedList<PaperNode>()// TODO 更好的写法？ 如何方便的给自定义域分配地址？如何避免手动写出 new 中类型？
        };
        newNode.data.AddLast(new WaveAttribute(WaveController.paperHeight / 2, 3)); // TODO：以后如果需要创建不同大小的纸片，何如？
        
        // 找到纸片的位置，实例化纸片，连接好节点到 WaveController 的对应，连接好纸片的 waveData
        Vector3 position = CalcPosition(expandedNode.children.Count);
        GameObject paper = Instantiate(paperPrefab, position, Quaternion.identity, papersHolderTransform);
        newNode.waveScript = paper.GetComponent<WaveController>();
        newNode.waveScript.waveData = newNode.data;

        // 将新节点添加到 当前展开节点expandedNode 的 子节点列表children 中
        expandedNode.children.AddLast(newNode);
    }

    ///  <summary> 重新整理并更新纸片们的位置 </summary>
    /// <param name="listNode">整理开始的位置（expandedNode.children.LinkedListNode）</param>
    void RepositionPapersStartFrom(LinkedListNode<PaperNode> listNode) {
        int count = 0;
        for (var i = expandedNode.children.First; i != listNode; i = i.Next)
            ++count;
        for (var i = listNode; i != null; i = i.Next)
            i.Value.waveScript.transform.localPosition = CalcPosition(count++);
    }

    /// <summary> 删除纸片 </summary>
    /// <remarks> 如果这个节点在其他地方有副本，则根据 C# 的垃圾回收规则，该节点的数据不会被彻底删除，
    /// 这样也方便了合并操作。 </remarks>
    /// <param name="listNode"> 被删除的纸片在所处 children 中对应的 LinkedListNode </param>
    void DeleteNode(LinkedListNode<PaperNode> listNode) {
        // 析构纸片对象
        Destroy(listNode.Value.waveScript.gameObject);

        // 从父节点（当前被展开观察节点）的孩子列表中删除
        LinkedListNode<PaperNode> nextNode = listNode.Next;
        expandedNode.children.Remove(listNode);

        // 重新整理并更新纸片们的位置
        if (nextNode != null)
            RepositionPapersStartFrom(nextNode);
    }

    /// <summary> 合并纸片 </summary>
    /// <param name="sonLLNode"> 被拖动的纸片在原所处 children 中对应的 LinkedListNode </param>
    /// <param name="fatherLLNode"> 被添加的纸片在所处 children 中对应的 LinkedListNode </param>
    void MergeNodes(LinkedListNode<PaperNode> sonLLNode, LinkedListNode<PaperNode> fatherLLNode) {
        PaperNode son = sonLLNode.Value;
        PaperNode father = fatherLLNode.Value;

        // // 被并购
        // var start = to.data.Last;
        foreach (var waveAttribute in son.data)
            father.data.AddLast(waveAttribute);
        // start = start.Next;
        // var end = to.data.Last;
        father.waveScript.Refresh();

        // // 认人为父
        // father.children.AddLast(son);

        // 不堪其辱：“我选择死亡！”
        // // 注：son 仍在 father.children 中有引用，故数据不会被彻底删除，
        // //     只会 Destroy 对应纸片、重排、并从原父节点下移除。
        DeleteNode(sonLLNode); 
    }

    void OnGUI() {
        // Make a background box
        GUI.Box(new Rect(10,10,100,120), "PapersManager");
    
        // Make the first button. If it is pressed, create a new papar
        if(GUI.Button(new Rect(20,40,80,20), "Create")) {
            // Debug.Log("Create");
            CreateNode();
        }
    
        // Make the second button. If it is pressed, delete the first and the last paper
        if(GUI.Button(new Rect(20,70,80,20), "Delete")) {
            // Debug.Log("Delete");
            DeleteNode(expandedNode.children.First);
            DeleteNode(expandedNode.children.Last);
        }
    
        // Make the third button. If it is pressed, merge from the first to the last paper
        if(GUI.Button(new Rect(20,100,80,20), "Merge")) {
            Debug.Log("Merge");
            MergeNodes(expandedNode.children.First, expandedNode.children.Last);
        }
    }

    void Awake() { // TODO
        InitialzieHolder();
    }
}