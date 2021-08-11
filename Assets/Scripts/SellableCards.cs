using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SellableCards : MonoBehaviour
{
    // Start is called before the first frame update
    private Button btn;
    private PlayerController playerController;
    public Card card;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ClickEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
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
