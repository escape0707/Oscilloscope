using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 此脚本用于控制其 GameObject 的 LineRenderer 中点的数据 </summary>
public class WaveController : MonoBehaviour {

    /// <summary> 临时用于确定波形的参数 //TODO </summary>
    public float a, omega, phi;
    /// <summary> 初始化后点的横坐标的间隔 </summary>
    public float deltaX;
    /// <summary> 波形所在的 纸片的transform </summary>
    public Transform paperTransform;
    /// <summary> 重新初始化 LineRenderer </summary>
    public void Reinitialize() {
        isInitialized = false;
    }
    
    /// <summary> WaveController 所控制的 LineRenderer </summary>
    private LineRenderer lr;
    /// <summary> WaveController 是否已经初始化 </summary>
    private bool isInitialized = false;
    /// <summary> 波形展示区的宽度 </summary>
    private float weight;

    /// <summary> 临时波形函数：正弦函数 //TODO </summary>
    float WaveFunction(float x) {
        return a * Mathf.Sin(omega * x + phi);
    }

    /// <summary> 初始化 LineRenderer </summary>
    void InitializeLineRender() {
        // 计算初始化后点的总数
        int initCount = (int)Math.Ceiling(weight / deltaX);
        Vector3[] positions = new Vector3[initCount];

        // 计算初始波形上各点的位置， t(x) = t0 - x
        for (int i = 0; i < initCount; ++i)
            positions[i] = new Vector3(i * deltaX, WaveFunction(Time.time - i * deltaX), 0);

        // 更新 LineRenderer
        lr.positionCount = initCount;
        lr.SetPositions(positions);
        isInitialized = true;
    }
    
    void Start() {
        // 绑定 LineRenderer ，清空原有点集，并设置为使用本地空间
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = false;
    }

    void Update() {
        // 由波形所在的纸片的 X 轴的 localScale 确定波形展示区的宽度
        weight = paperTransform.localScale.x;
        // 调整波的位置
        transform.localPosition = new Vector3(-weight / 2, 0, 0);
        // 若未初始化 LineRenderer ，先初始化
        if (!isInitialized) {
            InitializeLineRender();
            return;
        }

        // 取出原本在 LineRenderer 中的点
        int positionCount = lr.positionCount;
        Vector3[] positions = new Vector3[positionCount + 1];
        lr.GetPositions(positions);

        // 将之前 Line 中点的坐标更新，并且检测新坐标是否超出范围
        // 如果发现新坐标超出范围，则停止更新，并计算 positionCount
        for (int i = 0; i < positionCount; ++i)
            if ((positions[i].x += Time.deltaTime) > weight) {
                positionCount = i + 1; 		// 注意：这里下标为 i 的点，是要（保留）的
                break;
            }

        // 将点顺移一个下标
        for (int i = positionCount; i > 0; --i)
            positions[i] = positions[i - 1];
        
        // 计算新点的坐标，加入存储数组
        float y = WaveFunction(Time.time);
        ++positionCount;
        positions[0] = new Vector3(0, y, 0);

        // 更新 LineRenderer
        lr.positionCount = positionCount;
        lr.SetPositions(positions);
    }
}
