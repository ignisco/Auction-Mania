using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card {
    public int value;
    public GameObject gameObjectCard;
    public Card(int value, GameObject gameObjectCard) {
        this.value = value;
        this.gameObjectCard = gameObjectCard;
        updateGameObject();
    }
    
    // Update GameObject card to match the value of this card
    public void updateGameObject() {
        this.gameObjectCard.GetComponentInChildren<Text>().text = this.value.ToString();
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
    public int turnOfPlayer;
    private Player currentPlayer {get {return players[turnOfPlayer];}}
    private Player nextPlayer {get { return players[(turnOfPlayer + 1) % numberOfPlayers];}}
    private PlayerController playerController;
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

        if (Time.time - start > 0.2) {
            start = Time.time;
            // 0 being the player
            if (turnOfPlayer != 0 && gameObjectCards.Exists(goCard => goCard.activeInHierarchy)) {
                currentPlayer.playBid(currentBid + 1);
                return;
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
            Debug.Log("Round is over");
            if(UnityEditor.EditorApplication.isPlaying) 
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            return;
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
    
}
