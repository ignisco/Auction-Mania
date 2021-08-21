using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameButton : MonoBehaviour
{

    private PlayerController playerController;
    
    private void Start() {
        GetComponent<Button>().onClick.AddListener(clickEvent);
    }

    public void setPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    void clickEvent()
    {
        if (playerController != null) {
            switch (gameObject.name)
            {
                case "Increase":
                    playerController.increaseBid();
                    break;
                case "Decrease":
                    playerController.decreaseBid();
                    break;
                case "Bid":
                    playerController.playBid();
                    break;
                case "Pass":
                    playerController.pass();
                    break;
            }
        }
    }
}
