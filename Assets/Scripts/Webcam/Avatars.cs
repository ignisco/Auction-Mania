using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Avatars : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private GameObject capturePrefab;

    private void Awake()
    {
        // Keeping this in the game scene
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Instantiate the capture prefab on the correct player position for each client
        var canvas = transform.Find("Player -" + Networking.PlayerNumber.ToString())
        .Find("Canvas");
        // instantiate capture prefab as child of canvas
        Instantiate(capturePrefab, canvas);
    }

    [PunRPC]
    public void RPC_DisplayImage(byte[] image, int playerNumber)
    {
        Debug.Log("Receiving");
        var texture2d = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture2d.LoadImage(image);
        var sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero);

        // Set the image on the canvas
        var imageGameObject = transform.Find("Player -" + playerNumber)
            .Find("Canvas").Find("Image").gameObject;
        imageGameObject.GetComponent<Image>().sprite = sprite;
        imageGameObject.SetActive(true);


    }

    [PunRPC]
    public void RPC_SetBidAndCash()
    {
        // set bid and cash for all players
        foreach (Transform player in transform)
        {
            var canvas = player.Find("Canvas");
            var bid = canvas.Find("Bid").gameObject;
            var cash = canvas.Find("Cash").gameObject;
            bid.SetActive(true);
            cash.SetActive(true);

        }
    }

    [PunRPC]
    public void RPC_SetPlayerName(string playerName, int playerNumber)
    {
        // set name on canvas for given player
        var textObject = transform.Find("Player -" + playerNumber)
            .Find("Canvas").Find("Name").gameObject;
        textObject.GetComponent<Text>().text = playerName;

    }
}
