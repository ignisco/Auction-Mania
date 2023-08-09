using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Networking : MonoBehaviourPunCallbacks
{
    private const string GameVersion = "1.0";
    private const int MaxPlayersPerRoom = 4;

    // Each client keeps track of there number
    public static int PlayerNumber;

    [SerializeField]
    private GameObject _lobby; // Reference to the shared lobby prefab
    private GameObject _lobbyInstance; // Reference to the instantiated lobby prefab

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
            Vector3 spawnPosition = new Vector3(0f, 0f, 0f); // Set your spawn position
            PhotonNetwork.Instantiate(_lobby.name, spawnPosition, Quaternion.identity);
        }

        PlayerNumber = PhotonNetwork.PlayerList.Length;
    }

    // ... Rest of the script ...
}
