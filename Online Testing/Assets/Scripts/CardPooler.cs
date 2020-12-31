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
        card.SetActive(false);
        cardPool.Add(card);
    }
}
