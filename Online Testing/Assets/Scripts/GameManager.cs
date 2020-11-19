using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    nh_network server;

    public bool myTurn = false;

    private void Awake()
    {
        if (instance) Destroy(gameObject);
        else instance = this;

        server = nh_network.server;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void drawCard(bool fromDeck)
    {
        if (myTurn)
        {
            server.drawCard(fromDeck);
            myTurn = false; //TEMP
        }
    }
}
