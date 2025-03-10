using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    void Awake()
    {
        // 确保 GameManager 只存在一个实例
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 保持对象在场景切换时不被销毁
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 200秒后调用 QuitGame 方法
        Invoke("QuitGame", 200f);
    }

    void QuitGame()
    {
        Debug.Log("Quitting game after 200 seconds...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}