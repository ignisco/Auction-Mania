using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LeaveButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(leaveGame);
    }

    // Update is called once per frame
    void leaveGame()
    {
        StartCoroutine(NetworkManager.instance.leaveGame());
    }
}
