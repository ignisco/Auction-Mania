using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Awake: " + this.gameObject);
    }

    public void LoadScene(string sceneName) {
        UnitySceneManager.LoadScene(sceneName);
    }
}