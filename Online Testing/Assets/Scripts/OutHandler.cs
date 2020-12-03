using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutHandler : MonoBehaviour
{
    List<CardButton> handCopy;

    public DropHandler[] dropSpots;
    bool[] openDrop = {false, false, false, false};
    int nextToOpen = 1;

    public Button goOutBtn;

    private void OnEnable()
    {
        OpenOutMenu();
    }

    public void OpenOutMenu()
    {
        for (int i = 0; i < 4; i++)
        {
            dropSpots[i].gameObject.SetActive(false);
            openDrop[i] = false;
        }

        dropSpots[0].gameObject.SetActive(true);
        openDrop[0] = true;
        nextToOpen = 1;

        handCopy = new List<CardButton>();
        foreach (CardButton c in GameManager.instance.myHand) handCopy.Add(c);
    }

    public void CloseOutMenu()
    {
        // reset everything
    }

    public void OpenNewDrop()
    {
        if (nextToOpen != -1) dropSpots[nextToOpen].gameObject.SetActive(true);
        GetNextOpen();
    }

    void GetNextOpen()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!openDrop[i])
            {
                nextToOpen = i;
                return;
            }
        }
        nextToOpen = -1;
    }

    public void RemoveEmptyDrop()
    {
        int indexToRemove = -1;
        for(int i = 0; i < 4; i++)
        {
            if (dropSpots[i].checkEmpty())
            {
                if (indexToRemove == -1) indexToRemove = i;
                else
                {
                    //remove at i because i is larger
                    dropSpots[i].gameObject.SetActive(false);
                    openDrop[i] = false;
                    GetNextOpen();
                }
            }
        }
    }

    public void ReturnToHand(CardButton cardAdded)
    {
        handCopy.Add(cardAdded);

        CheckForOut();
    }

    public void RemoveFromHand(CardButton cardRemoved)
    {
        if (handCopy.Contains(cardRemoved)) handCopy.Remove(cardRemoved);
        else Debug.LogError("Invalid Card", gameObject);

        CheckForOut();
    }

    void CheckForOut()
    {
        bool canGoOut;

        // check for valid dropHandlers
        for(int i = 0; i < 4; i++)
        {
            if (openDrop[i])
            {
                if (!dropSpots[nextToOpen].checkValid())
                {
                    canGoOut = false;
                }
            }
        }

        // check hand #
        if (handCopy.Count == 1) canGoOut = true;
        else canGoOut = false;

        if (canGoOut)
        {
            // enable a button
            goOutBtn.interactable = true;
        }
        else
        {
            goOutBtn.interactable = false;
        }
    }
}
