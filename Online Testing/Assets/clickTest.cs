using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// this namespace is for registering the onpointer
using UnityEngine.EventSystems;

public class clickTest : MonoBehaviour, IPointerClickHandler
{
    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
        Debug.Log(name + " Game Object Clicked!");
        Debug.Log(pointerEventData.button);

        // Check is the click is right mouse button
        if(pointerEventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right Button Clicked");
        }
    }
}
