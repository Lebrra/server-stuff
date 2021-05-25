using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Reducing instantiating and destroying
/// </summary>
public class CardPooler : MonoBehaviour
{
    public static CardPooler instance;

    List<GameObject> cardPool;
    public GameObject cardPrefab;

    public Sprite[] suitIcons;
    public Sprite[] crownIcons;

    private void Awake()
    {
        if (instance) Destroy(gameObject);
        else instance = this;

        cardPool = new List<GameObject>();
    }

    /// <summary>
    /// Takes a card from the pool to add to play, if no cards available it will create one
    /// </summary>
    /// <returns>Card GameObject</returns>
    public GameObject PopCard(string cardData, Transform parent)
    {
        GameObject card;

        if(cardPool.Count > 0)
        {
            card = cardPool[0];
            cardPool.RemoveAt(0);
            card.transform.SetParent(parent);
            card.transform.SetSiblingIndex(card.transform.parent.childCount - 1);
            card.transform.position = parent.transform.position;
        }
        else
        {
            card = Instantiate(cardPrefab, parent);
        }
        card.SetActive(true);

        //set card values
        card.GetComponent<CardButton>().MyCard(cardData);

        return card;
    }

    /// <summary>
    /// Returns card to the pool (card is no longer in use)
    /// </summary>
    public void PushCard(GameObject card)
    {
        if (cardPool.Contains(card)) return;

        card.GetComponent<CardButton>().interactable = true;
        card.GetComponent<CardButton>().UpdateCardImage(true);
        card.SetActive(false);
        cardPool.Add(card);
    }


    /// <summary>
    /// Returns Sprite for corresponding suit
    /// </summary>
    public (Sprite, Color) GetSuitImage(Suit suit)
    {
        switch (suit)
        {
            default:
                return (null, Color.black);
            case Suit.Club:
                return (suitIcons[0], Color.black);
            case Suit.Diamond:
                return (suitIcons[1], Color.red);
            case Suit.Heart:
                return (suitIcons[2], Color.red);
            case Suit.Spade:
                return (suitIcons[3], Color.black);
            case Suit.Joker:
                return (suitIcons[4], Color.gray);
        }
    }

    public Sprite GetCrownImage(int value)
    {
        switch (value)
        {
            default:
                return null;
            case 11:
                return crownIcons[0];
            case 12:
                return crownIcons[1];
            case 13:
                return crownIcons[2];
        }
    }
}
