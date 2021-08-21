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
        SellableCards sellableCard = newestGameObjectCard.GetComponent<SellableCards>();
        sellableCard.setRelatedCard(newestCard);
        sellableCard.setPlayerController(this);

        // Also reseting bid
        this.chosenBid = 0;
        updateBidGraphic();
    }

    public void playBid() {
        if (GameManager.Instance.isCurrentPlayer(this.player)) {
            if (this.player.canAfford(this.chosenBid) && GameManager.Instance.validateMove(this.chosenBid)) {
                NetworkManager.instance.SendMoveToServer(this.chosenBid.ToString(), this.player.getName());
            }
            else {
                Debug.Log("Not a legal move");
            }
        } 
        else {
            Debug.Log("Not your turn yet, you impatient piece of sh*t");
        }
    }

    public void pass() {
        if (GameManager.Instance.isCurrentPlayer(this.player)) {
            NetworkManager.instance.SendMoveToServer("0", this.player.getName());
        }
        else {
            Debug.Log("Not your turn yet, you impatient piece of sh*t");
        }
    }

    private void updateBidGraphic() {
        bidText.text = "$" + this.chosenBid;
    }

    public void increaseBid() {
        if (this.chosenBid < this.player.totalBalance) {
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
        card.setSellingPlayer(this.player);
        if (GameManager.Instance.biddingPhase) {
            return false;
        }
        if (!GameManager.Instance.canSellCard(card)) {
            return false;
        }
        card.nameOfSellingPlayer = this.player.getName();
        NetworkManager.instance.SendMoveToServer(JsonUtility.ToJson(card), this.player.getName());
        return true;
    }
}
