using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // The System.IO namespace contains functions related to loading and saving files

public class DataController : MonoBehaviour {
    private LevelData[] allLevelData;

    private string gameDataFileName = "data.json";

    void Start() {
        DontDestroyOnLoad(gameObject);

        LoadGameData();
    }

    /// <summary>
    /// 获取当前关卡的关卡数据
    /// </summary>
    /// <returns> 当前关卡的关卡数据 </returns>
    internal LevelData GetCurrentLevelData() {
        // If we wanted to return different rounds, we could do that here
        // We could store an int representing the current round index in PlayerProgress

        return allLevelData[0]; // TODO
    }

    /// <summary> 从磁盘文件读取并解析数据 </summary>
    private void LoadGameData() {
        // 计算磁盘文件的绝对路径
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);

        if (File.Exists(filePath)) {
            // 从磁盘文件读取 含有JSON格式的数据的字符串
            string dataAsJson = File.ReadAllText(filePath);
            // 解析字符串并生成 游戏数据对象
            GameData loadedData = JsonUtility.FromJson<GameData>(dataAsJson);

            // 从 游戏数据对象 获取 allLevelData属性
            allLevelData = loadedData.allLevelData;
        } else {
            Debug.LogError("Cannot load game data!");
        }
    }
}