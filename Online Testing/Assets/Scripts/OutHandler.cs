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

    private void OnDisable()
    {
        CloseOutMenu();
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
        foreach (DropHandler d in dropSpots) d.clearDropZone();
        handCopy.Clear();
        foreach (CardButton c in GameManager.instance.myHand) c.ReturnToHand();
    }

    public void OpenNewDrop()
    {
        if (nextToOpen != -1)
        {
            dropSpots[nextToOpen].gameObject.SetActive(true);
            openDrop[nextToOpen] = true;
        }
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
        if (!handCopy.Contains(cardAdded))
        {
            print($"Returning card to hand {cardAdded.myCard.suit} - {cardAdded.myCard.number}");
            handCopy.Add(cardAdded);
        }
        else Debug.LogWarning("Card already exists in hand", cardAdded.gameObject);

        CheckForOut();
    }

    public void RemoveFromHand(CardButton cardRemoved)
    {
        if (handCopy.Contains(cardRemoved)) handCopy.Remove(cardRemoved);
        else Debug.LogWarning("Card not found in hand", cardRemoved.gameObject);

        CheckForOut();
    }

    void CheckForOut()
    {
        if (CanGoOut())
        {
            // enable a button
            goOutBtn.interactable = true;
        }
        else
        {
            goOutBtn.interactable = false;
        }
    }

    bool CanGoOut()
    {
        // check for valid dropHandlers
        for (int i = 0; i < 4; i++)
        {
            if (openDrop[i])
            {
                if (!dropSpots[i].checkValid())
                {
                    return false;
                }
            }
        }

        // check hand #
        if (handCopy.Count == 1) return true;
        else return false;
    }
}
