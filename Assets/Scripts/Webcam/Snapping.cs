using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Snapping : MonoBehaviour
{


    public void Snap()
    {
        // Photo handling of camera is done in WebCam.cs
        FindObjectOfType<WebCam>().SnapPhoto();

        // Set name of your character
        TMP_InputField inputField = FindObjectOfType<TMP_InputField>();
        FindObjectOfType<Networking>().SetPlayerName(inputField.text);

    }
}
