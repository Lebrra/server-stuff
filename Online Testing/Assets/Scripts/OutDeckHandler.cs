using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OutDeckHandler : MonoBehaviour
{
    public List<CardButton> myCurrentHand;

    public OutDropHandler[] firstOutDrops;
    //public TextMeshProUGUI[] titleTexts;
    public UnityEngine.UI.Text[] firstTitleTexts;

    public void setOutDeck(List<string>[] cards, Out[] outTypes)
    {
        for (int i = 0; i < 4; i++)
        {
            //default state is off
            if (outTypes[i] != Out.None)
            {
                firstOutDrops[i].gameObject.SetActive(true);
                firstTitleTexts[i].text = outTypes[i].ToString();
                firstOutDrops[i].setOutState(outTypes[i]);

                foreach(string card in cards[i])
                {
                    GameObject newCard = CardPooler.instance.PopCard(card, firstOutDrops[i].transform);
                    newCard.GetComponent<CardButton>().interactable = false;
                    firstOutDrops[i].addOutDeckCard(newCard.GetComponent<CardButton>());
                }
            }
        }

        Debug.Log("First out panel loaded.");
    }

    public void resetOutPanel()     //needed?
    {
        foreach(DropHandler d in firstOutDrops)
        {
            //reset data in each drop
            d.gameObject.SetActive(false);
        }
    }

    public void FillHandCopy(List<CardButton> hand)
    {
        Debug.Log("Created final copy of hand");
        myCurrentHand = new List<CardButton>();
        foreach (CardButton a in hand) myCurrentHand.Add(a);
    }

    public void ReturnToHand(CardButton cardAdded)
    {
        if (!myCurrentHand.Contains(cardAdded))
        {
            print($"Returning card to hand {cardAdded.myCard.suit} - {cardAdded.myCard.number}");
            myCurrentHand.Add(cardAdded);
        }
        else Debug.LogWarning("Card already exists in hand", cardAdded.gameObject);
    }

    public bool RemoveFromHand(CardButton card)
    {
        if (myCurrentHand.Contains(card))
        {
            myCurrentHand.Remove(card);
            return true;
        }
        else
        {
                // they were first out
            if (myCurrentHand.Count == 0) return false;

            Debug.LogError("Card " + card.name + " not found in hand copy.", card.gameObject);
            return false;
        }
    }
}
