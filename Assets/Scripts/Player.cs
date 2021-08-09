using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private int balance = 20;
    private int bid = 0;
    private bool hasTurn = false;
    public bool hasPassed = false;
    private Text balanceText;
    private Text nameText;
    private Text bidText;
    public void Start() {
        this.balanceText = gameObject.transform.Find("Canvas").Find("Cash").GetComponent<Text>();
        this.bidText = gameObject.transform.Find("Canvas").Find("Bid").GetComponent<Text>();
        this.nameText = gameObject.transform.Find("Canvas").Find("Name").GetComponent<Text>();
    }
    
    
    void showNewBalance() {
        this.balanceText.text = "$" + this.balance;
        this.bidText.text = "$" + this.bid;
    }

    public void getsTurn() {
        this.hasTurn = true;
        this.nameText.fontStyle = FontStyle.Bold;
    }

    public void endsTurn() {
        this.hasTurn = false;
        this.nameText.fontStyle = FontStyle.Normal;
    }

    public void roundWon() {
        this.bid = 0;
        showNewBalance();
    }

    public void pass() {
        this.hasPassed = true;
        GameManager.Instance.makeMove(0);
        int cashBack = Mathf.FloorToInt(this.bid/2.0f);
        this.balance += cashBack;
        this.bid = 0;
        showNewBalance();
    }

    public void playBid(int bid) {
        int bidIncrease = bid - this.bid;
        if (balance - bidIncrease < 0) {
            Debug.Log("You can't afford this");
            pass();
            return;
        }

        if (GameManager.Instance.validateMove(bid)) {
            GameManager.Instance.makeMove(bid);
            this.balance -= bidIncrease;
            this.bid = bid;
            showNewBalance();
        } else {
            Debug.Log("Need to bid higher");
            playBid(bid + 1);
        }
    }

    
}