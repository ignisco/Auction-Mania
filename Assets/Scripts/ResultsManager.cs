using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResultsManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Text> textOfPlayers;
    
    private void Awake() {
        for (int i = 0; i < textOfPlayers.Count; i++)
        {
            textOfPlayers[i].text = "";
        }
    }
    void Start()
    {
        List<Player> players = new List<Player>(GameManager.Instance.players);
        Debug.Log("Final score (best to worst):");
        players.Sort((p1, p2) => p2.totalBalance - p1.totalBalance);
        for (int i = 0; i < players.Count; i++)
        {
            textOfPlayers[i].text = (i+1) + ". " + players[i].getName() + " - $" + players[i].totalBalance;
        }

    }
}

