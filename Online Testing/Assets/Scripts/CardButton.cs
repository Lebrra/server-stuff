using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Card myCard;

    Transform parentObject;
    Transform handObject;

    bool waitForClick = false;

    public bool interactable = true;

    private void Update()
    {
        if(waitForClick && Input.GetMouseButtonDown(0))
        {
            ShrinkCard();
        }
    }

    public void MyCard(string cardName)
    {
        myCard = CardParser.parseCard(cardName);
        handObject = parentObject = transform.parent;

        //set texts
        char cardText = CardParser.valueToChar(myCard.number);
        if (cardText == '0') transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "10";
        else transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = cardText.ToString();
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = myCard.suit.ToString();

        if(myCard.suit == Suit.Diamond || myCard.suit == Suit.Heart)
        {
            // make texts red
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
            transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
        }
        else
        {
            // make texts black
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = Color.black;
            transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = Color.black;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // zoom in on card
        if (!waitForClick)
        {
            transform.localScale *= 2;
            Debug.Log("card expanded");

            waitForClick = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        GetComponent<Image>().raycastTarget = false;
        
        // ~- Leah I've noticed that if i disable the line below, we don't get the card bloat -- can you look at this?
        transform.SetParent(handObject);
        transform.localScale = transform.localScale;
        
        print("DRAG BEGIN: " + transform.parent);

        // if in drop handler, remove it from its list   <--- USING IT FOR TWO DIFFERENT OBJECTS == CONFUSING
        if (parentObject.GetComponent<DropHandler>()) parentObject.GetComponent<DropHandler>().removeCard(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (interactable)
        {
            // move card image with mouse/finger
            transform.position = Input.mousePosition;

            // print("DRAGGING: " + transform.parent);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;

        var rayResults = new List<RaycastResult>();
        bool hitHand = false;
        EventSystem.current.RaycastAll(eventData, rayResults);
        foreach (var hit in rayResults)
        {
            if (hit.gameObject.transform == handObject)
            {
                hitHand = true;
            }
        }

        print("ray results: " + rayResults);
            
        print("dropObkect: " + dropObject.name);

        if (dropObject?.GetComponent<DropHandler>())
        {
            if (dropObject.GetComponent<DropHandler>().checkValidDrop(this))
            {
                parentObject = dropObject.transform;
                transform.SetParent(parentObject);
            }
            else
            {
                print("failed valid drop");
                ReturnToHand();
            }
        }
        else if (dropObject?.GetComponent<DiscardHandler>())
        {
            if (!dropObject.GetComponent<DiscardHandler>().DiscardCard(this))
                ReturnToHand();
        } else if (hitHand)
        {
            print("Manualling resort cards");
            transform.SetParent(null);
 
            Vector3 dropPos = eventData.position;
            print("dropPos: " + dropPos);
            CardButton [] cards = handObject.GetComponentsInChildren<CardButton>();
            // i and i+1 x position
            
            print("mouse released at: " + dropPos.x);
            
            for (int i = 0; i < cards.Length-1; i++)
            {
                print("card pos: " + cards[i].transform.position.x);
                if (dropPos.x > cards[i].transform.position.x && dropPos.x < cards[i + 1].transform.position.x)
                { 
                    // print();
                    int sibInd = cards[i].transform.GetSiblingIndex();
                    print("Inserting card at: " + sibInd + 1);
                    ReturnToHand();
                    transform.SetSiblingIndex(sibInd+1);
                }
                else if (dropPos.x < cards[0].transform.position.x)
                {
                    print("Moving card to left-most position");
                    ReturnToHand();
                    transform.SetAsFirstSibling();
                }
                else
                {
                    print("Moving card to right-most position");
                    ReturnToHand();
                    transform.SetAsLastSibling();
                }
            }
            // handObject.chi
        }
        else
        {
            print("did not drag card to a droppable object");
            ReturnToHand();
        }

        GetComponent<Image>().raycastTarget = true;
    }

    public void ReturnToHand()
    {
        print("return to hand");
        parentObject = handObject;
        transform.SetParent(parentObject);
        if (transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>())
        {
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;
        }
    }

    void ShrinkCard()
    {
        transform.localScale /= 2;
        Debug.Log("card shrunk");
        waitForClick = false;
    }
}
