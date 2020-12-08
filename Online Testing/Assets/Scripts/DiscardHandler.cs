using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // if my turn, can send card to discard and remove it from hand
    }

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
