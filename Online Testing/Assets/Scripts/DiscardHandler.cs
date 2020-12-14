﻿using System.Collections;
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
            GameManager.instance.myHand.Remove(card);
            CardPooler.instance.PushCard(card.gameObject);
            return true;
        }
        else return false;
    }
}