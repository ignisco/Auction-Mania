using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card {
    public int value;
    public GameObject gameObject;
    public Card(int value, GameObject gameObject) {
        this.value = value;
        this.gameObject = gameObject;
        updateGameObject();
    }
    
    // Update GameObject card to match the value of this card
    public void updateGameObject() {
        this.gameObject.GetComponentInChildren<Text>().text = this.value.ToString();
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<Player> players;
    public List<GameObject> gameObjectCards;
    private List<int> cardDeck;
    private List<Card> cards;
    private int numberOfPlayers {get {return players.Count;}}
    private int turnOfPlayer;
    private Player currentPlayer {get {return players[turnOfPlayer];}}
    private Player nextPlayer {get { return players[(turnOfPlayer + 1) % numberOfPlayers];}}
    private int currentBid;

    float start;
    // Start is called before the first frame update

    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }

        // TODO: get number of players from server
        int numberOfPlayersFromServer = 6;

        for (int i = players.Count - 1; i >= numberOfPlayersFromServer ; i--)
        {
            players[i].gameObject.SetActive(false);
            players.Remove(players[i]);
        }

        // Sorting by name to ensure we move clockwise around the table
        players.Sort((o1, o2) => o1.name.CompareTo(o2.name));   
    }
    void Start()
    {
        // Randomly choosing who starts the game
        turnOfPlayer = Random.Range(0, numberOfPlayers - 1);
        
        cardDeck = new List<int>();
        for (int i = 0; i < 30; i++) {
            cardDeck.Add(i+1);
        }

        // Removing random cards until remaining deck is a multiple of numberOfPlayers
        // NB: if there are only 3 players, remove 6 cards
        if (numberOfPlayers == 3) {
            for (int i = 0; i < 6; i++) {
                drawCard();
            }
        }
        while (cardDeck.Count % numberOfPlayers != 0) {
            drawCard();
        }

        cards = new List<Card>();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            cards.Add(new Card(drawCard(), gameObjectCards[0]));
            gameObjectCards.RemoveAt(0);
        }

        // Deactivating unused cards
        foreach (GameObject card in gameObjectCards) {
            card.SetActive(false);
        }
    }

    private void Update() {

        if (Time.time - start > 0.5) {
            start = Time.time;
            currentPlayer.playBid(currentBid + 1);
        }
    }


    void roundOver() {
        currentPlayer.roundWon();
        updateCards();
        currentBid = 0;
        foreach (Player player in players)
        {
            player.hasPassed = false;
        }
    }

    // Update is called once per frame
    public void makeMove(int bid)
    {
        // Player passes
        if (bid == 0) {
            for (int i = 1; i < numberOfPlayers; i++)
            {
                List<Player> activePlayers = players.FindAll(player => !player.hasPassed);
                // This player won the round
                if (activePlayers.Count == 1) {
                    currentPlayer.endsTurn();
                    turnOfPlayer = players.IndexOf(activePlayers[0]);
                    activePlayers[0].getsTurn();
                    roundOver();
                    return;
                }


                Player player = players[(turnOfPlayer + i) % numberOfPlayers];

                // First in line who has yet to pass gets a turn
                if (!player.hasPassed) {
                    currentPlayer.endsTurn();
                    turnOfPlayer = players.IndexOf(player);
                    player.getsTurn();
                    return;
                }
            }
        } else {
            currentBid = bid;
            for (int i = 1; i < numberOfPlayers; i++)
            {
                Player player = players[(turnOfPlayer + i) % numberOfPlayers];

                // First in line who has yet to pass gets a turn
                if (!player.hasPassed) {
                    currentPlayer.endsTurn();
                    turnOfPlayer = players.IndexOf(player);
                    player.getsTurn();
                    return;
                }
            }
        }

        throw new System.Exception("What happend here??");

    }

    public bool validateMove(int bid) {
        if (bid > currentBid) {
            return true;
        }
        return false;
    }

    int drawCard () {
        int card = cardDeck[Random.Range(0, cardDeck.Count - 1)];
        cardDeck.Remove(card);
        return card;
    }

    void updateCards() {
        if (cardDeck.Count == 0) {
            Debug.Log("Round is over");
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i] = new Card(drawCard(), cards[i].gameObject);
        }
    }

    
}
