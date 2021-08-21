using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class NetworkManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static NetworkManager instance;

    private Firebase.FirebaseApp app;
    private CollectionReference games;
    private DocumentReference currentGame;
    private ListenerRegistration currentListener;
    private List<ListenerRegistration> currentPlayerListeners = new List<ListenerRegistration>();
    public string currentName;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Awake: " + this.gameObject);

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        var dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
            app = Firebase.FirebaseApp.DefaultInstance;
            fireBaseReadyStart();
            // Set a flag here to indicate whether Firebase is ready to use by your app.
        } else {
            UnityEngine.Debug.LogError(System.String.Format(
            "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
        });
    }

    // treating it like Start(), but called after firebase is ready
    void fireBaseReadyStart() {
        games = FirebaseFirestore.GetInstance(app).Collection("games");
        StartCoroutine(cleanup());
    }

    private IEnumerator cleanup() {

        Query query = games.WhereLessThan("timestamp", Firebase.Firestore.Timestamp.FromDateTime(System.DateTime.Now.AddHours(-2d)));
        var querySnapshotTask = query.GetSnapshotAsync();

        yield return new WaitUntil(() => querySnapshotTask.IsCompleted);

        if (querySnapshotTask.Result == null) {
            Debug.Log("Error contacting the server for cleanup");
            yield break;
        }

        var enumerator = querySnapshotTask.Result.Documents.GetEnumerator();
        while (enumerator.MoveNext()) {
            var doc = enumerator.Current.Reference;
            var subcollection = doc.Collection("players");
            var task = subcollection.GetSnapshotAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            if (task.Result == null) {
                Debug.Log("Error contacting the server for cleanup");
                yield break;
            }
            var playerDocsEnumerator = task.Result.Documents.GetEnumerator();
            while (playerDocsEnumerator.MoveNext()) {
                playerDocsEnumerator.Current.Reference.DeleteAsync();
                Debug.Log("Deleted player " + playerDocsEnumerator.Current.Id + " from " + enumerator.Current.Id);
            }
            enumerator.Current.Reference.DeleteAsync();
            Debug.Log("Deleted inactive game document " + enumerator.Current.Id);
        }
    }

    public IEnumerator leaveGame() {
        if (currentGame == null) {
            Debug.Log("Not currently in any game, but still loading Lobby Scene");
            SceneManager.instance.LoadScene("Lobby");
            yield break;
        }

        var documentSnapshotTask = currentGame.GetSnapshotAsync();

        yield return new WaitUntil(() => documentSnapshotTask.IsCompleted);

        var players = documentSnapshotTask.Result.GetValue<List<object>>("players");

        players.Remove(currentName);

        currentGame.SetAsync(
            new {
                players = players,
            },
        SetOptions.MergeAll);


        currentGame = null;
        currentName = null;
        currentListener.Stop();
        currentListener = null;

        SceneManager.instance.LoadScene("Lobby");
    }

    public void listenToGameData(System.Action<Dictionary<string, object>> callback) {

        if (currentGame == null) {
            Debug.Log("Error: There is no current game to listen to");
            return;
        }

        if (currentListener != null) {
            // Overwriting listener with new listener
            // We don't need more than one listener at any time in this game
            currentListener.Stop();
        }

        currentListener = currentGame.Listen(snapshot => {
            if (snapshot.Exists) {
                callback(snapshot.ToDictionary());
            }
        });

    }

    public void listenToPlayerData(List<string> playerNames, System.Action<Dictionary<string, object>> callback) {

        if (currentGame == null) {
            Debug.Log("Error: There is no current game to listen to");
            return;
        }



        // if (currentPlayerListeners != null) {
        //     // Overwriting listener with new listener
        //     // We don't need more than one listener at any time in this game
        //     currentListener.Stop();
        // }

        foreach (string playerName in playerNames)
        {
            currentPlayerListeners.Add(currentGame.Collection("players").Document(playerName).Listen(snapshot => {
            if (snapshot.Exists) {
                callback(snapshot.ToDictionary());
            }
        }));
        }

    }

    public void SendMoveToServer(string move, string playerName) {
        StartCoroutine(sendMoveToServerTest(move, playerName));
    }

    private IEnumerator sendMoveToServerTest(string move, string playerName) {
        var playerDoc = currentGame.Collection("players").Document(playerName);
        var task = playerDoc.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        var documentSnapshot = task.Result;
        var moves = documentSnapshot.GetValue<List<string>>("moves");
        moves.Insert(0, move);
        playerDoc.SetAsync(
            new {
                moves = moves,
            });
    }

    private string RandomString(int length)
    {
        System.Random random = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    
    public IEnumerator HostGame(string name, int numberOfPlayers, System.Action<string> error) {
        string gameID;
        while (true) {
            gameID = RandomString(4);
            Query query = this.games.WhereEqualTo("gameID", gameID);
            var querySnapshotTask = query.GetSnapshotAsync();

            yield return new WaitUntil(() => querySnapshotTask.IsCompleted);

            if (querySnapshotTask.Result == null) {
                error("Server isn't answering, leave a message after the beep");
                yield break;
            }
            else if (querySnapshotTask.Result.Count == 0) {
                // This gameID is unique
                break;
            }
        }

        
        DocumentReference game = this.games.Document();
        var createGame = game.SetAsync(
            new {
                gameID = gameID,
                numberOfPlayers = numberOfPlayers,
                players = new List<object>(),
                timestamp = FieldValue.ServerTimestamp,
                randomSeed = System.DateTime.Now.Ticks,
            }
        );

        yield return new WaitUntil(() => createGame.IsCompleted);

        StartCoroutine(JoinGame(name, gameID, error));

    }


    public IEnumerator JoinGame(string name, string gameID, System.Action<string> error) {
        Query query = this.games.WhereEqualTo("gameID", gameID);
        var querySnapshotTask = query.GetSnapshotAsync();

        yield return new WaitUntil(() => querySnapshotTask.IsCompleted);

        if (querySnapshotTask.Result == null) {
            error("Server isn't answering, leave a message after the beep");
            yield break;
        }
        else if (querySnapshotTask.Result.Count == 0) {
            error("Sorry bro, no game found at " + gameID);
            yield break;
        }

        var enumerator = querySnapshotTask.Result.Documents.GetEnumerator();
        enumerator.MoveNext();
        Debug.Log(enumerator.Current.Id);


        DocumentSnapshot game = enumerator.Current;
        var players = game.GetValue<List<object>>("players");
        var numberOfPlayers = game.GetValue<long>("numberOfPlayers");

        if (players.Count == numberOfPlayers) {
            error("Late to the party? Seems they are already playing at " + gameID);
            yield break;
        }

        if (players.Contains(name)) {
            error("Name's already taken, gotta find a new one :/");
            yield break;
        }

        players.Add(name);
        game.Reference.SetAsync(
            new {
                players = players,
            },
        SetOptions.MergeAll);

        game.Reference.Collection("players").Document(name).SetAsync(
            new {
                moves = new List<string>(),
            });

        this.currentGame = game.Reference;
        this.currentName = name;
        error(null);
    }
}
