using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks
{
    [PunRPC]
    private void RPC_DisplayImage(byte[] image, int playerNumber)
    {
        var texture2d = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture2d.LoadImage(image);
        GameObject.Find("SharedCanvas(Clone)").transform.Find("Lobby")
            .Find(playerNumber.ToString()).GetComponentInChildren<RawImage>().texture = texture2d;
    }
}
