using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HostButton : MonoBehaviour
{
    // Start is called before the first frame update
    private InputField inputField;
    private Slider slider;
    private Text errorMessage;
    void Start()
    {
        inputField = FindObjectOfType<InputField>();
        slider = FindObjectOfType<Slider>();
        errorMessage = GameObject.Find("Error").GetComponent<Text>();
        GetComponent<Button>().onClick.AddListener(hostGame);
    }

    // Update is called once per frame
    void hostGame()
    {
        if (inputField.text.Length == 0) {
            errorMessage.text = "Need a name if you wanna play";
            return;
        }
        
        StartCoroutine(NetworkManager.instance.HostGame(inputField.text, Mathf.FloorToInt(slider.value), callback));
    }

    void callback(string error) {
        if (error == null) {
            SceneManager.instance.LoadScene("Waiting");
        } else {
            Debug.Log(error);
            errorMessage.text = error;
        }
    }
}
