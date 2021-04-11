using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundCounter : MonoBehaviour
{
    public static RoundCounter inst;
    TextMeshProUGUI myText;

    private void Awake()
    {
        inst = this;
        myText = GetComponent<TextMeshProUGUI>();
    }

    public void setRound(int round)
    {
        if(round < 2 || round > 13)
        {
            Debug.LogError("round out of bounds");
            return;
        }

        string roundString = round.ToString();
        if (round == 11) roundString = "J";
        else if (round == 12) roundString = "Q";
        else if (round == 13) roundString = "K";

        myText.text = roundString;
    }
}
