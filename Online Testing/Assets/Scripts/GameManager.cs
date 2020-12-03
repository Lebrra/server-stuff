﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    nh_network server;

    [Header("Turn Bools")]
    public bool myTurn = false; // might not need this one
    public bool myDraw = false;
    public bool myDiscard = false;

    [Header("Hand")]
    public GameObject cardPrefab;
    public Transform handObject;
    public List<CardButton> myHand;

    [Header("Panels")]
    public GameObject outPanel;

    private void Awake()
    {
        if (instance) Destroy(gameObject);
        else instance = this;

        server = nh_network.server;
    }

    // Start is called before the first frame update
    void Start()
    {
        server.setReady();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void discardCard(string cardName)
    {
        if (myDiscard)
        {
            server.discardCard(cardName);
            myDiscard = myTurn = false;
        }
    }

    public void addCardToHand(string cardName)
    {
        GameObject newCard = Instantiate(cardPrefab, handObject);
        newCard.GetComponent<CardButton>().MyCard(cardName);
        myHand.Add(newCard.GetComponent<CardButton>());
    }

    public void openOutPanel(bool open)
    {
        outPanel.SetActive(open);
    }
}
