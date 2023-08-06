using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private int balance = 20;
    private int bid = 0;
    public bool hasTurn = false;
    public bool hasPassed = false;
    private Text balanceText;
    private Text nameText;
    private Text bidText;

    private GameObject drawnCard;
    private Text drawnCardText;

    // balance and current bid combined
    public int totalBalance { get { return bid + balance; } }

    public List<Card> cards;
    public void Start()
    {
        this.balanceText = gameObject.transform.Find("Canvas").Find("Cash").GetComponent<Text>();
        this.bidText = gameObject.transform.Find("Canvas").Find("Bid").GetComponent<Text>();
        this.nameText = gameObject.transform.Find("Canvas").Find("Name").GetComponent<Text>();
        this.cards = new List<Card>();
        this.drawnCard = gameObject.transform.Find("Card").gameObject;
        this.drawnCard.SetActive(false);
        this.drawnCardText = this.drawnCard.GetComponentInChildren<Text>();
    }

    public string getName()
    {
        return this.nameText.text;
    }

    public void getsCard(Card card)
    {
        cards.Add(card);
        this.bidText.gameObject.SetActive(false); // Hiding bid
        this.drawnCard.SetActive(true);
        this.drawnCardText.text = card.value.ToString();
    }

    public void earnMoney(int value)
    {
        this.balance += value;
        showNewBalance();
    }

    public void revealCardForSale(Card cardForSale)
    {
        this.drawnCardText.text = cardForSale.value.ToString();
    }

    public void drawHiddenCardGraphics()
    {
        this.drawnCard.SetActive(true);
        this.drawnCardText.text = "";
    }

    // Hide card and show bid when new round starts
    public void hideDrawnCard()
    {
        this.drawnCard.SetActive(false);
        if (GameManager.Instance.biddingPhase)
        {
            this.bidText.gameObject.SetActive(true);
        }
    }

    void showNewBalance()
    {
        this.balanceText.text = "$" + this.balance;
        this.bidText.text = "$" + this.bid;
    }

    public void getsTurn()
    {
        this.hasTurn = true;
        // set text to red
        this.nameText.color = Color.red;
        // set text to bold
        this.nameText.fontStyle = FontStyle.Bold;
        // make border red
        var borders = transform.Find("Canvas").Find("Borders").GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer border in borders)
        {
            border.color = Color.red;
        }

    }

    public void endsTurn()
    {
        this.hasTurn = false;
        // set text to black
        this.nameText.color = Color.black;
        // set text to normal
        this.nameText.fontStyle = FontStyle.Normal;
        // make border black
        var borders = transform.Find("Canvas").Find("Borders").GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer border in borders)
        {
            border.color = Color.black;
        }
    }

    public void roundWon()
    {
        this.bid = 0;
        showNewBalance();
    }

    public void pass()
    {
        GameManager.Instance.makeMove(0);
        int cashBack = Mathf.FloorToInt(this.bid / 2.0f);
        this.balance += cashBack;
        this.bid = 0;
        showNewBalance();
    }

    public string playBid(int bid)
    {
        int bidIncrease = bid - this.bid;
        if (balance - bidIncrease < 0)
        {
            Debug.Log("You can't afford this");

            // Pass if bot
            if (GameManager.Instance.turnOfPlayer != 0)
            {
                pass();
            }

            return "You can't afford this";
        }

        if (GameManager.Instance.validateMove(bid))
        {
            GameManager.Instance.makeMove(bid);
            this.balance -= bidIncrease;
            this.bid = bid;
            showNewBalance();
            return null;
        }
        else
        {
            Debug.Log("Need to bid higher");
            // Play higher if bot
            if (GameManager.Instance.turnOfPlayer != 0)
            {
                playBid(bid + 1);
            }
            return "Need to bid higher";
        }
    }


    // Methods related to selling

    public bool sellCard(Card card = null)
    {
        if (card == null)
        {
            // CPUs randomly pick a card to sell
            card = cards[Random.Range(0, cards.Count - 1)];
        }
        card.SetSellingPlayer(this);
        bool success = GameManager.Instance.sellCard(card);
        if (success)
        {
            cards.Remove(card);
        }
        return success;
    }


}