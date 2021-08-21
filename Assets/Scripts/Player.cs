using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private int balance = 20;
    private int bid = 0;
    public bool hasPassed = false;
    private Text balanceText;
    private Text nameText;
    private Text bidText;

    private GameObject drawnCard;
    private Text drawnCardText;

    // balance and current bid combined
    public int totalBalance {get {return bid + balance;}}

    public List<Card> cards;
    public void Start() {
        this.balanceText = gameObject.transform.Find("Canvas").Find("Cash").GetComponent<Text>();
        this.bidText = gameObject.transform.Find("Canvas").Find("Bid").GetComponent<Text>();
        this.nameText = gameObject.transform.Find("Canvas").Find("Name").GetComponent<Text>();
        this.cards = new List<Card>();
        this.drawnCard = gameObject.transform.Find("Card").gameObject;
        this.drawnCard.SetActive(false);
        this.drawnCardText = this.drawnCard.GetComponentInChildren<Text>();
    }

    public string getName() {
        return this.nameText.text;
    }

    public void setName(string name) {
        this.nameText.text = name;
    }
    
    public void getsCard(Card card) {
        cards.Add(card);
        this.bidText.gameObject.SetActive(false); // Hiding bid
        this.drawnCard.SetActive(true);
        this.drawnCardText.text = card.value.ToString();
    }

    public void earnMoney(int value) {
        this.balance += value;
        showNewBalance();
    }

    public void revealCardForSale(Card cardForSale) {
        this.drawnCardText.text = cardForSale.value.ToString();
    }

    public void drawHiddenCardGraphics() {
        this.drawnCard.SetActive(true);
        this.drawnCardText.text = "";
    }

    // Hide card and show bid when new round starts
    public void hideDrawnCard() {
        this.drawnCard.SetActive(false);
        if (GameManager.Instance.biddingPhase) {
            this.bidText.gameObject.SetActive(true);
        }
    }
    
    void showNewBalance() {
        this.balanceText.text = "$" + this.balance;
        this.bidText.text = "$" + this.bid;
    }

    public void getsTurn() {
        this.nameText.fontStyle = FontStyle.Bold;
    }

    public void endsTurn() {

        this.nameText.fontStyle = FontStyle.Normal;
    }

    public void roundWon() {
        this.bid = 0;
        showNewBalance();
    }

    private void pass() {
        GameManager.Instance.makeMove(0);
        int cashBack = Mathf.FloorToInt(this.bid/2.0f);
        this.balance += cashBack;
        this.bid = 0;
        showNewBalance();
    }

    public void playBid(int bid) {
        if (bid == 0) {
            pass();
            return;
        }
        int bidIncrease = bid - this.bid;
        GameManager.Instance.makeMove(bid);
        this.balance -= bidIncrease;
        this.bid = bid;
        showNewBalance();
    }

    public bool canAfford(int bid) {
        return bid <= this.totalBalance;
    }
    
}