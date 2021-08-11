using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card {
    public int value;
    public GameObject gameObjectCard;
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
    public List<Player> players;
    private List<Card> sellingCards;
    public List<GameObject> gameObjectCards;
    public GameObject BiddingPhaseUI;
    private List<int> cardDeck;
    private List<Card> cards;
    private int numberOfPlayers {get {return players.Count;}}
    public int turnOfPlayer;
    private Player currentPlayer {get {return players[turnOfPlayer];}}
    private Player nextPlayer {get { return players[(turnOfPlayer + 1) % numberOfPlayers];}}
    private PlayerController playerController;
    private int currentBid;

    public bool biddingPhase = true;

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
        players.Sort((o1, o2) => o1.gameObject.name.CompareTo(o2.gameObject.name));

        playerController = FindObjectOfType<PlayerController>();
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

        updateCards();
    }

    private void Update() {

        if (biddingPhase) {
            if (Time.time - start > 0.2) {
                start = Time.time;

                if (!playerController.controlsPlayer(currentPlayer) && gameObjectCards.Exists(goCard => goCard.activeInHierarchy)) {
                    currentPlayer.playBid(currentBid + 1);
                    return;
                }
            }
        }
    }

    IEnumerator nextRound() {
        yield return new WaitForSeconds(2);
        updateCards();
        currentBid = 0;
        foreach (Player player in players)
        {
            player.hasPassed = false;
            player.hideDrawnCard();
        }


        if (!biddingPhase) {
            StartCoroutine(CPUSellCard());
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
                turnOfPlayer = players.IndexOf(activePlayers[0]);
                activePlayers[0].getsTurn();
                givePlayerCard(currentPlayer, cards[0]);
                roundOver();
                return;
            }

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

        // Player raises bid
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
            if (!this.biddingPhase) {
                // Game is over
                Debug.Log("Game is over");
                Debug.Log("Final score (best to worst):");
                players.Sort((p1, p2) => p2.totalBalance - p1.totalBalance);
                foreach (Player player in players)
                {
                    Debug.Log(player.getName() + " got $" + player.totalBalance);
                }

                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
            else {
                Debug.Log("Bidding Phase is over");
                currentPlayer.endsTurn();
                this.biddingPhase = false;
                this.BiddingPhaseUI.SetActive(false); // Hiding UI related to bidding
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

    public bool sellCard(Card card) {

        if (sellingCards.Exists(c => c.getSellingPlayer() == card.getSellingPlayer())) {
            return false;
        }

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
        return true;
    }


    IEnumerator CPUSellCard() {
        foreach (Player player in players)
        {
            if (!playerController.controlsPlayer(player)) {
                float timeToWait = Random.Range(0f, 1f);
                yield return new WaitForSeconds(timeToWait);
                player.sellCard();
            }
        }
    }
    
}
