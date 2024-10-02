using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GameDataModel gameData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 씬 로드 매서드
    /// </summary>
    /// <param name="sceneIndex"> -1의 경우 씬 종료 </param>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (sceneIndex < SceneManager.sceneCount)
        {
            //미구현
        }
    }
    
}
