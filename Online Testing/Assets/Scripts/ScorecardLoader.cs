using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorecardLoader : MonoBehaviour
{
    public static ScorecardLoader inst;

    [Header("Waiting")]
    public GameObject waitingPopup;
    public TextMeshProUGUI waitingScore;

    [Header("Scorecard")]
    public GameObject scoreCard;
    public TextMeshProUGUI[] nameTexts;
    public ScorecardTexts[] scorecardTexts;
    public TextMeshProUGUI[] totalScores;

    int[][] loadedScores;

    private void Awake()
    {
        inst = this;
    }

    public void EnableWait(int score)
    {
        waitingPopup.SetActive(true);
        waitingScore.text = score.ToString();
    }

    public void DisableWait()
    {
        waitingPopup.SetActive(false);
        scoreCard.SetActive(true);
    }

    public void LoadNames(string[] playerNames)
    {
        for(int i = 0; i < playerNames.Length; i++)
        {
            nameTexts[i].text = playerNames[i];
        }
    }

    public void LoadScores(int[][] scores)
    {
        Debug.Log("loading all values...");
        Debug.Log("first value: " + scores[0][0]);
    }
}

[System.Serializable]
public struct ScorecardTexts
{
    public TextMeshProUGUI[] roundRow;
}