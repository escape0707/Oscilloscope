using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 用于控制游戏中各种 singleton 的 GameManager </summary>
public class GameManager : MonoBehaviour {
    private static GameManager instance = null;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}