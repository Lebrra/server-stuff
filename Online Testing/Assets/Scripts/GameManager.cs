using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    nh_network server;

    [Header("Round Info")] public int round;

    [Header("Turn Bools")]
    public bool myTurn = false; // might not need this one
    public bool myDraw = false;
    public bool myDiscard = false;

    [Header("Hand")]
    public GameObject cardPrefab;
    public Transform handObject;
    public List<CardButton> myHand;

    [Header("Discard Objects")]
    public Transform discardTransform;
    public GameObject cardInDiscard;

    [Header("Panels")]
    public GameObject outPanel;

    private void Awake()
    {
        if (instance) Destroy(gameObject);
        else instance = this;

        server = nh_network.server;
    }

    void Start()
    {
        server.setReady();
    }

    public void drawCard(bool fromDeck)
    {
        if (myDraw)
        {
            server.drawCard(fromDeck);
            myDraw = false;
            myDiscard = true;
        }
    }

    public bool discardCard(string cardName)
    {
        if (myDiscard)
        {
            server.discardCard(cardName);
            myDiscard = myTurn = false;
            return true;
        }
        else return false;
    }

    public void addCardToHand(string cardName)
    {
        GameObject newCard = CardPooler.instance.PopCard(cardName, handObject);
        myHand.Add(newCard.GetComponent<CardButton>());
        
        // notification
        var notification = new Notification($"Discarded {newCard.GetComponent<CardButton>().myCard.ToString()}", 3, true, Color.black);
        NotificationManager.instance.addNotification(notification);
    }

    public void addCardToDiscard(string cardName)
    {
        GameObject newCard = CardPooler.instance.PopCard(cardName, discardTransform);
        newCard.GetComponent<CardButton>().enabled = false;

        if (cardInDiscard)
        {
            cardInDiscard.GetComponent<CardButton>().enabled = true;
            CardPooler.instance.PushCard(cardInDiscard);
            cardInDiscard = null;
        }

        // notification
        var notification = new Notification($"Discarded {newCard.GetComponent<CardButton>().myCard.ToString()}", 3, true, Color.black);
        NotificationManager.instance.addNotification(notification);

        cardInDiscard = newCard;
    }

    public void openOutPanel(bool open)
    {
        outPanel.SetActive(open);
    }
}
