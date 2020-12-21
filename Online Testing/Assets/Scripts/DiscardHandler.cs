using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardHandler : MonoBehaviour
{
    /// <summary>
    /// Returns true is card is discarded
    /// </summary>
    public bool DiscardCard(CardButton card)
    {
        string cardName = CardParser.deparseCard(card.GetComponent<CardButton>().myCard);
        if (GameManager.instance.discardCard(cardName))
        {
            if (GameManager.instance.lastRound)
            {
                if (GameManager.instance.outDeckHandler.RemoveFromHand(card))
                {
                    //calulate score
                    Debug.Log("I should calculate the score here");
                }
                else
                {
                    //score = 0
                    Debug.Log("Your score is 0");
                }
            }

            GameManager.instance.myHand.Remove(card);
            CardPooler.instance.PushCard(card.gameObject);
            return true;
        }
        else return false;
    }
}
