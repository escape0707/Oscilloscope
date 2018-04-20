using System;
using UnityEngine;

/// <summary> 
/// 此脚本用于控制其 GameObject 的 LineRenderer 中点的数据，
/// 使用时请设置好 waveData
/// </summary>
public class WaveController : MonoBehaviour {
    /// <summary> 波形所在的纸片的 transform </summary>
    public Transform paperTransform;
    /// <summary> 波形显示对象 Waveform 的 LineRenderer </summary>
    public LineRenderer lineRenderer;
    /// <summary> 波形展示区的宽度 </summary>
    internal const float paperWeight = 2; // TODO: 这里暂时还是写死的，为了方便之后实例化不同高宽的纸片，应该怎么做？
    // 回答：要么把宽度和高度当成静态变量用，要么每次生成的时候要求显示传递设置
    /// <summary> 波形展示区的高度 </summary>
    internal const float paperHeight = 1; // TODO: 如果波形超过了上下界，何如？
    
    /// <summary> 初始化时点的横坐标的间隔 </summary>
    internal const float deltaX = .02f;
    /// <summary> 当前 WaveController 所采用的波形数据源 </summary>
    internal WaveData waveData;

    /// <summary> 立即重新初始化 LineRenderer </summary>
    internal void Refresh() {
        lineRenderer.positionCount = 0;
        InitializeLineRender();
    }

    /// <summary> WaveController 是否已经初始化 </summary>
    private bool isInitialized = false;
    /// <summary> 点坐标数据的缓冲区 </summary>
    private static Vector3[] positions;

    /// <summary> 检查点坐标数据缓冲区 positions 是否已经分配；并调整到可以容下 size 的大小 </summary>
    private static void CheckBufferSize(int size) {
        // 检查点坐标数据缓冲区 positions 是否已经分配
        if (positions == null)
            positions = new Vector3[Mathf.NextPowerOfTwo(size)];
        // 如果已分配，检查点坐标数据缓冲区大小是否小于 size ，
        // 并调整大小到能容纳的下一个 2 的幂
        else if (positions.Length < size)
            Array.Resize(ref positions, Mathf.NextPowerOfTwo(size));
    }

    /// <summary> 波形函数：众正弦函数叠加 </summary>
    private float WaveFunction(float x) {
        float y = 0;
        foreach (WaveAttribute wa in waveData)
            y += wa.a * Mathf.Sin(wa.omega * x + wa.phi);
        return y;
    }

    /// <summary> 初始化 LineRenderer </summary>
    private void InitializeLineRender() {
        // 计算初始化时点的总数
        int initCount = (int) Math.Ceiling(paperTransform.localScale.x / deltaX);
        CheckBufferSize(initCount);

        // 计算初始波形上各点的位置， t(x) = t0 - x
        for (int i = 0; i < initCount; ++i)
            positions[i] = new Vector3(i * deltaX, WaveFunction(Time.time - i * deltaX), 0);

        // 更新到 LineRenderer
        lineRenderer.positionCount = initCount;
        lineRenderer.SetPositions(positions);
        isInitialized = true;
    }

    void Start() {
        // 清空 LineRenderer 原有点集，并设置为使用本地空间
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = false;
        // 计算 Paper 对象的 Transform 
        paperTransform.localScale = new Vector3(paperWeight, paperHeight, 0);
        paperTransform.localPosition = new Vector3(paperWeight / 2, -paperHeight / 2, 0);
        // 计算 Waveform 对象的 Transform
        lineRenderer.GetComponent<Transform>().localPosition = new Vector3(0, -paperHeight / 2, 0);
    }

    void Update() {
        // 若未初始化 LineRenderer ，先初始化
        if (!isInitialized) {
            InitializeLineRender();
            return;
        }

        // 取出原本在 LineRenderer 中的点
        // TODO：这里是不是有更好的办法？比如把 positions 的结构换成 LinkedList 然后映射 + 反映射？
        int positionCount = lineRenderer.positionCount;
        CheckBufferSize(positionCount + 1);
        lineRenderer.GetPositions(positions);

        // 将之前 Line 中点的坐标更新，并且检测新坐标是否超出范围
        // 如果发现新坐标超出范围，则停止更新，并计算 positionCount
        // (如果均未超出范围（尽管不应如此），则保留原本的 positionCount)
        for (int i = 0; i < positionCount; ++i)
            if ((positions[i].x += Time.deltaTime) > paperWeight) {
                positionCount = i + 1; // 注意：这里下标为 i 的点，是要（保留）的
                break;
            }
        // 将点顺移一个下标
        for (int i = positionCount; i > 0; --i)
            positions[i] = positions[i - 1];

        // 计算新点的坐标，加入存储数组
        float y = WaveFunction(Time.time);
        positions[0] = new Vector3(0, y, 0);
        ++positionCount;

        // 更新 LineRenderer
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPositions(positions);
    }
}

/// <summary> 代表一条正弦波的参数组，由三个 float: a, omega, phi 组成 </summary>
internal class WaveAttribute {
    internal float a, omega, phi;
    internal WaveAttribute(float a = 1, float omega = 1, float phi = 0) {
        this.a = a;
        this.omega = omega;
        this.phi = phi;
    }
}