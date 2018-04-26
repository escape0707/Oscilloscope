/// <summary> 关卡的数据对象 </summary>
[System.Serializable]
internal class LevelData {
    /// <summary> 关卡内的多个纸片数据对象 </summary>
    internal PaperData[] papersData;
    /// <summary> 用户通关需要做的修改 </summary>
    internal WaveModification modification;
}