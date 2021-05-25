using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTestingScript : MonoBehaviour
{
    // for testing while not on a server

    Suit[] suits = new Suit[5] { Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade, Suit.Joker };

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.instance.addCardToHand(GenerateCard());
        }
    }

    string GenerateCard()
    {
        int randSuit = Random.Range(0, 5);
        if (randSuit == 4) return "Joker_0";

        int randNum = Random.Range(1, 14);
        return suits[randSuit] + "_" + randNum.ToString();
    }
}
