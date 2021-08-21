using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JoinButton : MonoBehaviour
{
    // Start is called before the first frame update
    private InputField nameField;
    private InputField gameIDField;
    private Text errorMessage;
    void Start()
    {
        nameField = GameObject.Find("Name").GetComponent<InputField>();
        gameIDField = GameObject.Find("GameID").GetComponent<InputField>();
        errorMessage = GameObject.Find("Error").GetComponent<Text>();
        GetComponent<Button>().onClick.AddListener(joinGame);
    }

    // Update is called once per frame
    void joinGame()
    {
        if (nameField.text.Length == 0 || gameIDField.text.Length == 0) {
            errorMessage.text = "Sorry, can't read your mind just yet. Gotta enter name and id the old way";
            return;
        }
        else if (gameIDField.text.Length < 4) {
            errorMessage.text = "That id seems a bit short";
            return;
        }
        else if (gameIDField.text.Length > 4) {
            errorMessage.text = "That id is too long. You just wasted time typing unnecessary stuff";
            return;
        }
        
        StartCoroutine(NetworkManager.instance.JoinGame(nameField.text, gameIDField.text.ToUpper(), callback));
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
