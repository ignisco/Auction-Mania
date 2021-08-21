using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SellableCards : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerController playerController;
    public Card card;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ClickEvent);
    }
    public void setPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    public void setRelatedCard (Card card) {
        this.card = card;
    }

    void ClickEvent() {
        if (playerController.sellCard(this.card)) {
            gameObject.SetActive(false);
        }
    }
}
