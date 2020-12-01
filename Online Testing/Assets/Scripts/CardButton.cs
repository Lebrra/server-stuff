using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
{
    public Card myCard;

    Transform handObject;

    public void MyCard(string cardName)
    {
        myCard = CardParser.parseCard(cardName);
        handObject = transform.parent;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(handObject.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move card image with mouse/finger
        transform.position = Input.mousePosition;
    }

    //public void 
}
