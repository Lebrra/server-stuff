using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardButton : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Card myCard;

    Transform parentObject;
    Transform handObject;

    private int sortingOrder;

    bool waitForClick = false;

    public bool interactable = true;
    bool wasInteracted = false;

    Image[] cardIconSprites = null;
    Image crownSprite = null;

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
        /*if (cardText == '0') transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "10";
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
        }*/

        if (cardText == '0')
        {
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "10";
            transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "10";
        }
        else
        {
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = cardText.ToString();
            transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = cardText.ToString();
        }

        UpdateCardImage(false);
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
        gameObject.AddComponent<Canvas>().overrideSorting = true;
        GetComponent<Canvas>().sortingOrder = 10;

        if (!interactable)
        {
            wasInteracted = true;
            return;
        }

        //gameObject.AddComponent<Canvas>().overrideSorting = true;
        //GetComponent<Canvas>().sortingOrder = 10;
        
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
        if (GetComponent<Canvas>())
        {
            Destroy(GetComponent<Canvas>());
        }

        if (!interactable)
        {
            wasInteracted = false;
            return;
        }

        if (wasInteracted)
        {
            // just snap back to where you came from
            ReturnToLastParent();
            wasInteracted = false;
            return;
        }

        bool hitHand = false;
        bool drop = false;
        
        // first hit ray
        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;
        
        // all hit ray
        var rayResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, rayResults);
        foreach (var hit in rayResults)
        {
            if (hit.gameObject.transform == handObject)
            {
                hitHand = true;
            }

            if (hit.gameObject.GetComponent<DropHandler>())
            {
                print("found outhandler? " + hit.gameObject.name);
                drop = true;
                dropObject = hit.gameObject;
            }
        }

        // if (dropObject?.GetComponent<DropHandler>())
        if(drop)
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
                
                if (dropPos.x < cards[0].transform.position.x)
                {
                    print("Moving card to left-most position");
                    ReturnToHand();
                    transform.SetAsFirstSibling();
                    break;
                }
                if (dropPos.x > cards[i].transform.position.x && dropPos.x < cards[i + 1].transform.position.x)
                {
                    // print();
                    int sibInd = cards[i].transform.GetSiblingIndex();
                    print("Inserting card at: " + sibInd);
                    ReturnToHand();
                    transform.SetSiblingIndex(sibInd + 1);
                    break;
                }
                if (dropPos.x > cards[cards.Length-1].transform.position.x)
                {
                    print("Moving card to right-most position");
                    ReturnToHand();
                    transform.SetAsLastSibling();
                    break;
                }
            }
            ReturnToHand();
        }
        else
        {
            print("Did not drag card to a droppable object");
            ReturnToHand();
        }

        GetComponent<Image>().raycastTarget = true;
    }

    public void ReturnToHand()
    {
        Debug.Log("Returned to hand", gameObject);
        parentObject = handObject;
        transform.SetParent(parentObject);
        
        if (transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>())
        {
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;
        }

        interactable = true;
    }

    public void ReturnToLastParent()
    {
        Debug.Log($"Returned to {parentObject.gameObject}", gameObject);
        transform.SetParent(parentObject);

        if (transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>())
        {
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
            transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;
        }

        interactable = true;
    }

    void ShrinkCard()
    {
        transform.localScale /= 2;
        Debug.Log("card shrunk");
        waitForClick = false;
    }

    public void UpdateCardImage(bool reset)
    {
        if (cardIconSprites == null) SetCardIcons();

        if (reset)
        {
            //reset all images
            for(int i = 1; i < cardIconSprites.Length; i++)
            {
                cardIconSprites[i].gameObject.SetActive(false);
            }
            transform.GetChild(4).GetChild(10).gameObject.SetActive(false);
            return;
        }

        (Sprite, Color) cardCustomization = CardPooler.instance.GetSuitImage(myCard.suit);

        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = cardCustomization.Item2;
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().color = cardCustomization.Item2;
        //transform.GetChild(3).GetComponent<Image>().color = cardCustomization.Item2;

        //transform.GetChild(3).GetComponent<Image>().sprite = cardCustomization.Item1;   // should joker be white?

        if (myCard.suit == Suit.Joker)
        {
            cardIconSprites[0].sprite = cardCustomization.Item1;
            cardIconSprites[0].color = Color.white;
            cardIconSprites[0].transform.localScale = Vector3.one * 1.5F;
            cardIconSprites[0].GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
        else
        {
            float scaler = 1.5F;

            // sprite positioning:
            switch (myCard.number)
            {
                default:    // 1, 11, 12, 13, other
                    //cardIconSprites[0].transform.localScale = Vector3.one * 1.5F;
                    scaler = 1.5F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = Vector3.zero;

                    break;

                case 2:
                    scaler = 0.8F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 60, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(0, -60, 0);

                    break;

                case 3:
                    scaler = 0.7F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(55, 85, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = Vector3.zero;
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(-55, -85, 0);

                    break;

                case 4:
                    scaler = 0.7F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(45, 55, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(-45, 55, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(45, -55, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-45, -55, 0);

                    break;

                case 5:
                    scaler = 0.6F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(60, 55, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(-60, 55, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(60, -55, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-60, -55, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = Vector3.zero;

                    break;

                case 6:
                    scaler = 0.6F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(65, 55, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(-65, 55, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(65, -55, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-65, -55, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = new Vector3(0, 55, 0);
                    cardIconSprites[5].GetComponent<RectTransform>().localPosition = new Vector3(0, -55, 0);

                    break;

                case 7:
                    scaler = 0.6F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = Vector3.zero;
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(65, 50, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(-65, 50, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-65, -50, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = new Vector3(65, -50, 0);
                    cardIconSprites[5].GetComponent<RectTransform>().localPosition = new Vector3(0, -70, 0);
                    cardIconSprites[6].GetComponent<RectTransform>().localPosition = new Vector3(0, 70, 0);

                    break;

                case 8:
                    scaler = 0.6F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(50, 70, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(-50, 70, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(50, -70, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-50, -70, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = new Vector3(50, 0, 0);
                    cardIconSprites[5].GetComponent<RectTransform>().localPosition = new Vector3(-50, 0, 0);
                    cardIconSprites[6].GetComponent<RectTransform>().localPosition = new Vector3(0, 110, 0);
                    cardIconSprites[7].GetComponent<RectTransform>().localPosition = new Vector3(0, -110, 0);

                    break;

                case 9:
                    scaler = 0.5F;
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(-60, 70, 0);
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(0, 70, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(60, 70, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(-60, 0, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    cardIconSprites[5].GetComponent<RectTransform>().localPosition = new Vector3(60, 0, 0);
                    cardIconSprites[6].GetComponent<RectTransform>().localPosition = new Vector3(-60, -70, 0);
                    cardIconSprites[7].GetComponent<RectTransform>().localPosition = new Vector3(0, -70, 0);
                    cardIconSprites[8].GetComponent<RectTransform>().localPosition = new Vector3(60, -70, 0);

                    break;

                case 10:
                    scaler = 0.5F;
                    cardIconSprites[1].GetComponent<RectTransform>().localPosition = new Vector3(-30, 80, 0);
                    cardIconSprites[2].GetComponent<RectTransform>().localPosition = new Vector3(30, 80, 0);
                    cardIconSprites[0].GetComponent<RectTransform>().localPosition = new Vector3(-30, -80, 0);
                    cardIconSprites[3].GetComponent<RectTransform>().localPosition = new Vector3(30, -80, 0);
                    cardIconSprites[4].GetComponent<RectTransform>().localPosition = new Vector3(-30, 0, 0);
                    cardIconSprites[5].GetComponent<RectTransform>().localPosition = new Vector3(30, 0, 0);
                    cardIconSprites[6].GetComponent<RectTransform>().localPosition = new Vector3(-70, -40, 0);
                    cardIconSprites[7].GetComponent<RectTransform>().localPosition = new Vector3(70, -40, 0);
                    cardIconSprites[8].GetComponent<RectTransform>().localPosition = new Vector3(-70, 40, 0);
                    cardIconSprites[9].GetComponent<RectTransform>().localPosition = new Vector3(70, 40, 0);

                    break;
            }

            if (myCard.number > 10)
            {
                // crown
                crownSprite.gameObject.SetActive(true);
                crownSprite.sprite = CardPooler.instance.GetCrownImage(myCard.number);

                cardIconSprites[0].sprite = cardCustomization.Item1;
                cardIconSprites[0].color = cardCustomization.Item2;
                cardIconSprites[0].transform.localScale = Vector3.one * scaler;
            }
            else
            {
                for(int i = 0; i < myCard.number; i++)
                {
                    cardIconSprites[i].gameObject.SetActive(true);
                    cardIconSprites[i].sprite = cardCustomization.Item1;
                    cardIconSprites[i].color = cardCustomization.Item2;
                    cardIconSprites[i].transform.localScale = Vector3.one * scaler;
                }
            }
        }
    }

    void SetCardIcons()
    {
        cardIconSprites = new Image[10];
        for (int i = 0; i < cardIconSprites.Length; i++)
        {
            cardIconSprites[i] = transform.GetChild(4).GetChild(i).GetComponent<Image>();
        }
        crownSprite = transform.GetChild(4).GetChild(10).GetComponent<Image>();
    }
}
