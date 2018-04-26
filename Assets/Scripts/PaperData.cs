using UnityEngine;

/// <summary> 单张纸片的数据对象 </summary>
[System.Serializable]
internal class PaperData {
    /// <summary> 波形展示区的宽度 </summary>
    internal float paperWeight;
    /// <summary> 波形展示区的高度 </summary>
    internal float paperHeight;
    /// <summary> 波形展示区的左上角的世界坐标 </summary>
    internal Vector3 position;
    /// <summary> 波形初始数据表 </summary>
    internal WaveAttribute[] waveDataList;
}