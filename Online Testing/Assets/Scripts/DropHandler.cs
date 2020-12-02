using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler, IComparer<CardButton>
{
    public List<CardButton> cards; 
    
    public Out outState;

    public float reorderTime = .1f;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("dropped", gameObject);
    }

    public bool checkValidDrop(CardButton newCard)
    {
        print("new card...");
        // first card
        if (cards.Count == 0)
        {
            cards.Add(newCard);
            return true;
        }
        
        // second card
        int incomingNum = newCard.myCard.number;
        Suit incomingSuit = newCard.myCard.suit;
        List<int> cardNums = cards.Select(acard => acard.myCard.number).ToList();
        // check wilds first~!
        
        //  compare second card to first card...
        if (cards.Count == 1)
        {
            if (incomingNum == cards[0].myCard.number)
            {
                outState = Out.Set;
            }
            else if (incomingSuit == cards[0].myCard.suit)
            {
                outState = Out.Run;
            }
            else
            {
                // if the second card is not set-making nor contiguous, reject 
                print($"Second card {newCard.myCard.suit}: {newCard.myCard.number} is neither set-making nor contiguous");
                return false;
            }
        }

        // 2 or more cards...
        if (outState == Out.Set)
        {
            if(incomingNum == cards[0].myCard.number)
            {
                cards.Add(newCard);
                Invoke("ReorderCardObjects", reorderTime);
                // ReorderCardObjects();
                return true;
            }
            else
            {
                return false;
            }
        }

        if (outState == Out.Run)
        {
            if (cardNums.Contains(incomingNum)) // this might be necessary if i adjust the contiguous
            {
                return false;
            }

            if (!checkContingous(incomingNum))
            {
                print($"{newCard.myCard.suit}: {newCard.myCard.number} - Card not contiguous.");
                return false;
            }
            else
            {
                cards.Add(newCard);
                cards.Sort(Compare);
                Invoke("ReorderCardObjects", reorderTime);
                // ReorderCardObjects();
                return true;
            }
        }

        print($"Oops something wrong with the validDropCheck on {gameObject.name}");
        return false;
    }

    public bool removeCard(CardButton card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            print($"Removed {card} from CheckPile on {gameObject.name}");
            cards.Sort(Compare);

            // ReorderCardObjects();
            
            if (cards.Count == 1)
            {
                outState = Out.None;
            }
            return true;
        }
        else
        {
            print($"Trying to remove card that doesn't exist in DropHandler on {gameObject.name}");
            return false;
        }
    }

    bool checkContingous(int cardNum)
    {
        print("checking contiguousness:");
        print(Mathf.Abs(cardNum - cards[0].myCard.number) );
        print(Mathf.Abs(cardNum - cards[cards.Count - 1].myCard.number));
        return Mathf.Abs(cardNum - cards[0].myCard.number) == 1 |
               Mathf.Abs(cardNum - cards[cards.Count - 1].myCard.number) == 1;
    }
    
    public int Compare(CardButton x, CardButton y)
    {
        print("Sorting Cards...");
        if (x.myCard.number < y.myCard.number) return -1;
        else if (x.myCard.number > y.myCard.number) return 1;
        else return 0;
    }

    void ReorderCardObjects()
    {
        foreach (var t in cards)
        {
            t.transform.SetAsLastSibling();
        }
    }
    
}
