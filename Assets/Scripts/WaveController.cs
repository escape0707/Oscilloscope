using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 此脚本用于控制其 GameObject 的 LineRenderer 中点的数据
public class WaveController : MonoBehaviour {
	private LineRenderer lr;		// WaveController 所控制的 LineRenderer
	public float a, omega, phi;		// 临时用于确定波形的参数 TODO
	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
		lr.positionCount = 0;
		lr.useWorldSpace = false;
	}

    // Update is called once per frame
    void Update () {
		// positionCount： 新 Line 中点的数量
		// 每次更新将会加入一个新点，故 positionCount 将会在之前基础上+1
		int positionCount = lr.positionCount + 1;
		Vector3[] positions = new Vector3[positionCount];
		lr.GetPositions(positions);
		// 将之前 Line 中点的坐标更新，并且检测新坐标是否超出范围
		for (int i = 0; i < positionCount - 1; i++)
			// 如果发现新坐标超出范围，则停止更新，并计算合理的 positionCount
			if ((positions[i].x += Time.deltaTime) > 1) {
				// Debug.Log(String.Format("Exceeding found!\nOld posC = {0}", positionCount));
				positionCount = i + 2;
				// Debug.Log(String.Format("New posC = {0}", positionCount));
				break;
			}
		// 将点顺移一位
		for (int i = positionCount - 1; i >= 1; i--)
			positions[i] = positions[i - 1];
		
		// 计算新点的坐标
		float y = a * Mathf.Sin(omega * Time.time + phi); // 临时波形函数 TODO
		positions[0] = new Vector3(0, y, 0);

		// 更新 lr
		lr.positionCount = positionCount;
		lr.SetPositions(positions);
	}
}
