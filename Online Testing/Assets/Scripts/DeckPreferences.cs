using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeckPreferences : MonoBehaviour
{
    public GameObject deckPanel;
    public Color[] colors;
    public Sprite[] backs;

    public GameObject colorPrefab;
    public GameObject cardbackPrefab;

    public Transform colorContainer;
    public Transform cardbackContainer;

    public GameObject demoCard;

    public GameObject inGameCard;

    public PlayerData data;

    // public ColorOption[] colors;

    // Start is called before the first frame update
    void Start()
    {
        data = SaveLoad.Load();

        foreach (var color in colors)
        {
            var newColorOption = Instantiate(colorPrefab, colorContainer);
            newColorOption.GetComponent<Image>().color = color;
            newColorOption.AddComponent<Button>();
            newColorOption.GetComponent<Button>().onClick.AddListener(()=> setColor(color));
        }
        
        foreach (var back in backs)
        {
            var newCardback = Instantiate(cardbackPrefab, cardbackContainer);
            newCardback.transform.GetChild(0).GetComponent<Image>().sprite = back;
            newCardback.AddComponent<Button>();
            newCardback.GetComponent<Button>().onClick.AddListener(
                ()=> setCardback(back));
        }

        updateInGameDeck();
    }

    void updateInGameDeck()
    {
        if (inGameCard != null)
        {
            print("Updating in game deck card");
            inGameCard.transform.GetChild(0).GetComponent<Image>().color = data.getColor();
            inGameCard.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = backs[data.cardback];
        }
    }

    public void openDeckPrefPanel()
    {
        deckPanel.SetActive(true);
        demoCard.transform.GetChild(0).GetComponent<Image>().color = data.getColor();
        demoCard.transform.GetChild(1).GetComponent<Image>().sprite = backs[data.cardback];
    }
    
    public void closeDeckPrefPanel()
    {
        SaveLoad.Save(data);
        deckPanel.SetActive(false);

        updateInGameDeck();
    }

    public void setColor(Color color)
    {
        print($"set color pressed {color}");
        // demoCard.GetComponent()
        demoCard.transform.GetChild(0).GetComponent<Image>().color = color;
        data.setColor(color);
        print("color: " + data.R + " - " + data.G  + " - "+ data.B  + " - ");
        // save
    }
    
    public void setCardback(Sprite back)
    {
        // demoCard.GetComponent()
        demoCard.transform.GetChild(1).GetComponent<Image>().sprite = back;
        data.cardback = Array.IndexOf(backs, back);
        print("back: " + data.cardback);
        //save
    }
}
