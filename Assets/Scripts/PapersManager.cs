using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 波的数据类型 </summary>
using WaveData = System.Collections.Generic.LinkedList<WaveController.WaveAttribute>;
/// <summary> 代表一条正弦波的参数组，由三个 <c>float: a, omega, phi</c> 组成 </summary>
using WaveAttribute = WaveController.WaveAttribute;
using System;

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
    /// <summary> 用于在 <c>Hierarchy</c> 中收纳众多节点的 <c>Holder</c> 的 <c>Transform</c> </summary>
    private Transform papersHolderTransform;
    /// <summary> 多叉树的根节点 </summary>  
    private PaperNode rootNode;
    /// <summary> 当前被展开观察节点 </summary>  
    private PaperNode expandedNode;

    /// <summary> 初始化 <c>Hierarchy</c> 中的 <c>Holder</c> </summary>
    void InitialzieHolder() {
        papersHolderTransform = new GameObject("Papers").transform;
        papersHolderTransform.position = topLeftPosition;
        expandedNode = rootNode = new PaperNode {
            children = new LinkedList<PaperNode>()
        };
    }

    // 计算屏幕上第 count 张纸片应有的 localPosition
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
            children = new LinkedList<PaperNode>()// TODO 更好的写法？ 如何方便的给自定义域 allocate ？如何避免手动写出 new 中类型？
        };
        newNode.data.AddLast(new WaveAttribute(WaveController.paperHeight / 2, 3)); // TODO：以后如果需要创建不同大小的纸片，何如？
        
        // 找到纸片的位置，实例化纸片，连接好节点到 WaveController 的对应，连接好纸片的 waveData
        Vector3 position = CalcPosition(expandedNode.children.Count);
        GameObject paper = Instantiate(paperPrefab, position, Quaternion.identity, papersHolderTransform);
        newNode.waveScript = paper.GetComponent<WaveController>();
        newNode.waveScript.waveData = newNode.data;

        // 将新节点添加到当前展开节点 expandedNode 的子节点列表 children 中
        expandedNode.children.AddLast(newNode);
    }

    void Awake() { // TODO
        InitialzieHolder();
    }
}