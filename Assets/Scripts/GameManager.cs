using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card {
    public int value;
    public GameObject gameObjectCard;

    // Used to find soldByPlayer after JSON-deserialization
    public string nameOfSellingPlayer;
    private Player soldByPlayer;
    public Card(int value, GameObject gameObjectCard) {
        this.value = value;
        this.gameObjectCard = gameObjectCard;
        updateGameObject();
    }
    
    // Update GameObject card to match the value of this card
    public void updateGameObject() {
        this.gameObjectCard.GetComponentInChildren<Text>().text = this.value.ToString();
    }

    public void setSellingPlayer(Player player) {
        this.soldByPlayer = player;
    }

    public Player getSellingPlayer() {
        if (this.soldByPlayer != null) {
            return this.soldByPlayer;
        }
        throw new System.Exception("No player defined as selling this card");
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool firstReceivedData = true;
    public List<Player> players;
    private List<string> moves;
    private List<Card> sellingCards;
    public List<GameObject> gameObjectCards;
    public GameObject biddingPhaseUI;
    public List<GameButton> biddingPhaseButtons;
    private List<int> cardDeck;
    private List<Card> cards;
    private int numberOfPlayers;
    private Player currentPlayer;
    private PlayerController playerController;
    private int currentBid;

    public bool biddingPhase = true;

    public List<string> getMoves() {
        return this.moves;
    }

    public bool isCurrentPlayer(Player player) {
        if (currentPlayer == null) return false;
        return this.currentPlayer.Equals(player);
    }

    void onReceivedGameData(Dictionary<string, object> data) {
        if (firstReceivedData) {
            Setup(data);
            firstReceivedData = false;
            return;
        }

    }

    void onReceivedPlayerData(Dictionary<string, object> data) {

        var networkMoves = data["moves"] as List<object>;

        string newestMove = networkMoves[0] as string;
        //this.moves.Insert(0, newestMove);

        if (biddingPhase) {
            currentPlayer.playBid(int.Parse(newestMove));
        }
        else {
            Debug.Log(newestMove);
            Card card = JsonUtility.FromJson<Card>(newestMove);
            card.setSellingPlayer(findPlayerByName(card.nameOfSellingPlayer));
            sellCard(card);
        }
    }

    private Player findPlayerByName(string name) {
        foreach (Player player in players)
        {
            if (player.getName() == name) return player;
        }
        return null;
    }

    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        NetworkManager.instance.listenToGameData(onReceivedGameData);
    }

    void Setup (Dictionary<string, object> data) {
        numberOfPlayers = (int) (long) data["numberOfPlayers"];
        List<string> playerNames = (data["players"] as List<object>).Select(e => e as string).ToList();
        NetworkManager.instance.listenToPlayerData(playerNames, onReceivedPlayerData);
        Random.InitState((int) (long) data["randomSeed"]);
        this.moves = new List<string>();
        
        for (int i = 0; i < numberOfPlayers; i++)
        {
            players[i].setName(playerNames[i]);

            // If you are controlling this player, enable the PlayerController
            if (playerNames[i] == NetworkManager.instance.currentName) {
                playerController = players[i].gameObject.GetComponent<PlayerController>();
                playerController.enabled = true;
                foreach (GameButton button in biddingPhaseButtons)
                {
                    button.setPlayerController(playerController);
                }
            }
        }
        
        for (int i = players.Count - 1; i >= numberOfPlayers ; i--)
        {
            players[i].gameObject.SetActive(false);
            players.Remove(players[i]);
        }

        // Sorting by name to ensure we move clockwise around the table
        players.Sort((o1, o2) => o1.gameObject.name.CompareTo(o2.gameObject.name));

        // Randomly choosing who starts the game
        currentPlayer = players[Random.Range(0, numberOfPlayers)];
        currentPlayer.getsTurn();
        
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
        if (numberOfPlayers == 2) {
            for (int i = 0; i < 14; i++) {
                drawCard();
            }
        }
        while (cardDeck.Count % numberOfPlayers != 0) {
            drawCard();
        }

        updateCards();
    }

    IEnumerator nextRound() {
        Player temp = currentPlayer;
        currentPlayer = null;
        yield return new WaitForSeconds(2);
        currentPlayer = temp;
        updateCards();
        currentBid = 0;
        foreach (Player player in players)
        {
            player.hasPassed = false;
            player.hideDrawnCard();
        }
    }


    void roundOver() {
        List<int> values = currentPlayer.cards.ConvertAll<int>(card => card.value);
        Debug.Log(string.Format("Here's the winner list: ({0}).", string.Join(", ", values)));
        currentPlayer.roundWon();

        // Wait a bit so players can see the result of the round before moving on
        StartCoroutine(nextRound());
    }

    void givePlayerCard(Player player, Card card) {
        currentPlayer.getsCard(card);
        if (playerController.controlsPlayer(currentPlayer)) {
            playerController.updateCardGraphics();
        }
        card.gameObjectCard.SetActive(false);
    }

    // Update is called once per frame
    public void makeMove(int bid)
    {
        // Player passes
        if (bid == 0) {

            currentPlayer.hasPassed = true;

            List<Player> activePlayers = players.FindAll(player => !player.hasPassed);
            
            // Player gets least valuable card
            givePlayerCard(currentPlayer, cards[activePlayers.Count]);
            
            List<int> values = currentPlayer.cards.ConvertAll<int>(card => card.value);
            Debug.Log(string.Format("Here's the list: ({0}).", string.Join(", ", values)));

            // If only one player hasn't passed, they are the winner of this bidding round
            if (activePlayers.Count == 1) {
                currentPlayer.endsTurn();
                currentPlayer = activePlayers[0];
                currentPlayer.getsTurn();
                givePlayerCard(currentPlayer, cards[0]);
                roundOver();
                return;
            }

            for (int i = 1; i < numberOfPlayers; i++)
            {

                Player player = players[(players.IndexOf(currentPlayer) + i) % numberOfPlayers];

                // First in line who has yet to pass gets a turn
                if (!player.hasPassed) {
                    currentPlayer.endsTurn();
                    currentPlayer = player;
                    currentPlayer.getsTurn();
                    return;
                }
            }
        }

        // Player raises bid
        currentBid = bid;
        for (int i = 1; i < numberOfPlayers; i++)
        {
            Player player = players[(players.IndexOf(currentPlayer) + i) % numberOfPlayers];

            // First in line who has yet to pass gets a turn
            if (!player.hasPassed) {
                currentPlayer.endsTurn();
                currentPlayer = player;
                currentPlayer.getsTurn();
                return;
            }
        }

        throw new System.Exception("What happend here??");

    }

    public bool validateMove(int bid) {
        return bid > currentBid;
    }

    int drawCard () {
        int card = cardDeck[Random.Range(0, cardDeck.Count)];
        cardDeck.Remove(card);
        return card;
    }

    void updateCards() {
        if (cardDeck.Count == 0) {
            if (!this.biddingPhase) {
                // Game is over
                Debug.Log("Game is over, loading Results Scene");
                SceneManager.instance.LoadScene("Results");
                return;
            }
            else {
                Debug.Log("Bidding Phase is over");
                currentPlayer.endsTurn();
                this.biddingPhase = false;
                this.biddingPhaseUI.SetActive(false); // Hiding UI related to bidding
                // Refilling Card deck with cash cards
                for (int i = 0; i <= 31; i++)
                {
                    int value = Mathf.FloorToInt(i/2);
                    if (value == 1) continue;
                    cardDeck.Add(value);
                }
                // Removing random cards until remaining deck is a multiple of numberOfPlayers
                // NB: if there are only 3 players, remove 6 cards
                if (numberOfPlayers == 3) {
                    for (int i = 0; i < 6; i++) {
                        drawCard();
                    }
                }
                if (numberOfPlayers == 2) {
                    for (int i = 0; i < 14; i++) {
                        drawCard();
                    }
                }
                while (cardDeck.Count % numberOfPlayers != 0) {
                    drawCard();
                }
            }
        }

        if (!this.biddingPhase) {
            sellingCards = new List<Card>();

        }


        // Re-activating all cards
        foreach (GameObject gameObjectCard in gameObjectCards)
        {
            gameObjectCard.SetActive(true);
        }

        cards = new List<Card>();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            cards.Add(new Card(drawCard(), gameObjectCards[i]));

            // DEBUG
            if (i == 0) {
                Debug.Log(JsonUtility.ToJson(cards[0]));
            }
        }

        // Sorting list descending to easily remove lowest value
        cards.Sort((c1, c2) => c2.value - c1.value);

        // Deactivating unused cards
        for (int i = numberOfPlayers; i < 6; i++) 
        {
            gameObjectCards[i].SetActive(false);
        }
    }



    // Methods related to selling

    public bool canSellCard(Card card) {
        return !sellingCards.Exists(c => c.getSellingPlayer() == card.getSellingPlayer());
    }

    public void sellCard(Card card) {
        sellingCards.Add(card);
        card.getSellingPlayer().drawHiddenCardGraphics();
        // All players have placed their cards
        if (sellingCards.Count == numberOfPlayers) {
            sellingCards.Sort((c1, c2) => c2.value - c1.value);
            for (int i = 0; i < numberOfPlayers; i++)
            {
                sellingCards[i].getSellingPlayer().revealCardForSale(sellingCards[i]);
                sellingCards[i].getSellingPlayer().earnMoney(cards[i].value);
            }
            // Wait a bit so players can see the result of the round before moving on
            StartCoroutine(nextRound());
        }
    }
}
