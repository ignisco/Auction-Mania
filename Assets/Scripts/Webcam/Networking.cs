using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Networking : MonoBehaviourPunCallbacks
{
    private const string GameVersion = "1.0";
    private const int MaxPlayersPerRoom = 4;

    // Each client keeps track of there number
    public static int PlayerNumber;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        JoinOrCreateRoom();
    }

    private void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MaxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("MyRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        Debug.Log("Number of players in room: " + PhotonNetwork.PlayerList.Length);


        // Instantiate the shared prefab on the server
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 spawnPosition = new Vector3(2.0999999f, 3.67000008f, 0.21266f); // Set your spawn position
            PhotonNetwork.Instantiate("Avatars", spawnPosition, Quaternion.Euler(0, 0, 90));
        }

        PlayerNumber = PhotonNetwork.PlayerList.Length;

    }


    // when master clicks start button
    public void LoadGame()
    {
        // turn on bid and cash on the avatars
        var photonView = FindObjectOfType<PhotonView>();
        photonView.RPC("RPC_SetBidAndCash", RpcTarget.AllBuffered);

        // load game scene
        PhotonNetwork.LoadLevel("Game");
    }

    // Cast RPC of own player name to all clients
    public void SetPlayerName(string playerName)
    {
        var photonView = FindObjectOfType<PhotonView>();
        photonView.RPC("RPC_SetPlayerName", RpcTarget.AllBuffered, playerName, Networking.PlayerNumber);
    }
}
