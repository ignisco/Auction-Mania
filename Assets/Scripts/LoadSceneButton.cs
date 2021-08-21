using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    // Start is called before the first frame update
    public string sceneToLoad;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadScene);
    }

    // Update is called once per frame
    void LoadScene()
    {
        SceneManager.instance.LoadScene(sceneToLoad);
    }
}
