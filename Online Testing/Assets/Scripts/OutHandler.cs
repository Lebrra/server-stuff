using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutHandler : MonoBehaviour
{
    public bool hasGoneOut = false;

    List<CardButton> handCopy;

    public DropHandler[] dropSpots;
    bool[] openDrop = {false, false, false, false};
    int nextToOpen = 1;

    public Button goOutBtn;

    private void OnEnable()
    {
        if (!hasGoneOut) OpenOutMenu();
    }

    private void OnDisable()
    {
        if (!hasGoneOut) CloseOutMenu();
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
                    foreach (CardButton c in dropSpots[i].cards) c.ReturnToHand();
                    dropSpots[i].clearDropZone();

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

    public void SendFirstOut()
    {
        JSONObject OutDeck = new JSONObject();

        for (int i = 0; i < 4; i++)
        {
            JSONObject cardArr = JSONObject.Create(JSONObject.Type.ARRAY);
            //print("type? " + cardArr.type);

            cardArr.Add(dropSpots[i].outState.ToString());

            if (dropSpots[i].outState != Out.None)
            {
                foreach (CardButton c in dropSpots[i].cards)
                {
                    cardArr.Add(CardParser.deparseCard(c.myCard));
                }
            }

            //cardArr.Add(Out.Run.ToString());
            //cardArr.Add("joker_2");
            //cardArr.Add("Club_3");
            //cardArr.Add("Club_4");
            //print("contents? - " + cardArr);

            OutDeck.AddField("out" + i, cardArr);
        }

        //OutDeck.AddField("out1", cardArr);
        //OutDeck.AddField("out2", cardArr);
        print("test? - " + OutDeck);
        nh_network.server.SendFirstOut(OutDeck);

        hasGoneOut = true;
        GameManager.instance.outButton.SetActive(false);

        GameManager.instance.openOutPanel(false);
    }
}
