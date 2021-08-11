using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Player player;

    // Cards displayed to the right of the table
    public List<GameObject> gameObjectCards;
    private int chosenBid = 0;
    public Text bidText;
    void Start()
    {
        this.player = GetComponent<Player>();
        this.chosenBid = int.Parse(bidText.text.Substring(1));

        foreach (GameObject gameObjectCard in gameObjectCards)
        {
            gameObjectCard.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool controlsPlayer(Player player) {
        return this.player.Equals(player);
    }

    public void updateCardGraphics() {
        Card newestCard = player.cards[player.cards.Count - 1];
        GameObject newestGameObjectCard = gameObjectCards[player.cards.Count - 1];
        newestGameObjectCard.GetComponentInChildren<Text>().text = newestCard.value.ToString();
        newestGameObjectCard.SetActive(true);
        newestGameObjectCard.GetComponent<SellableCards>().setRelatedCard(newestCard);

        // Also reseting bid
        this.chosenBid = 0;
        updateBidGraphic();
    }

    public void playBid() {
        if (this.player.hasTurn) {
            string play = player.playBid(this.chosenBid);
            // There were errors
            if (play != null) {
                Debug.Log("Error: This will be shown to player: " + play);
            }
        } 
        else {
            Debug.Log("Not your turn yet, you impatient piece of sh*t");
        }
    }

    public void pass() {
        if (this.player.hasTurn) {
            player.pass();
        }
        else {
            Debug.Log("Not your turn yet, you impatient piece of sh*t");
        }
    }

    private void updateBidGraphic() {
        bidText.text = "$" + this.chosenBid;
    }

    public void increaseBid() {
        if (this.chosenBid < player.totalBalance) {
            this.chosenBid += 1;
            updateBidGraphic();
        }
    }

    public void decreaseBid() {
        if (this.chosenBid > 0) {
            this.chosenBid -= 1;
            updateBidGraphic();
        }
    }


    public bool sellCard(Card card) {
        if (GameManager.Instance.biddingPhase) {
            return false;
        }
        bool success = this.player.sellCard(card);
        return success;
    }
}
