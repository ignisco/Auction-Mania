using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;

public class WaitingManager : MonoBehaviour
{
    // Start is called before the first frame update

    private Text title;
    private Text status;
    public List<Text> textOfPlayers;
    void Start()
    {
        title = GameObject.Find("Title").GetComponent<Text>();
        status = GameObject.Find("Status").GetComponent<Text>();
        NetworkManager.instance.listenToGameData(onReceivedData);
    }

    // Update is called once per frame

    void onReceivedData(Dictionary<string, object> data) {
        string gameID = data["gameID"] as string;
        int numberOfPlayers = (int) (long) data["numberOfPlayers"];
        List<string> players = (data["players"] as List<object>).Select(e => e as string).ToList();


        title.text = "Waiting for Game at " + gameID;
        status.text = "Players (" + players.Count + "/" + numberOfPlayers + ")";

        for (int i = 0; i < players.Count; i++)
        {
            textOfPlayers[i].text = players[i];
        }

        for (int i = players.Count; i < textOfPlayers.Count; i++) {
            textOfPlayers[i].text = "";
        }

        // Cheking whether to start the game
        if (players.Count == numberOfPlayers) {
            SceneManager.instance.LoadScene("Game");
        }
    }
}
