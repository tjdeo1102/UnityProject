using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject YellowCoin;
    public GameObject GetStarShotCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Player = FindAnyObjectByType<PlayerMovement>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PlayerMovement Player;

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
            //
        }
    }
    

    public void GenerateYellowCoin(Vector3 spawnPos)
    {
        Instantiate(YellowCoin,spawnPos,Quaternion.identity,null);
    }
}
