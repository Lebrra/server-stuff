using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorecardLoader : MonoBehaviour
{
    [Header("Waiting")]
    public GameObject waitingPopup;
    public TextMeshProUGUI waitingScore;

    [Header("Scorecard")]
    public GameObject scoreCard;
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[][] scorecardTexts;
    public TextMeshProUGUI[] totalScores;

    int[][] loadedScores;

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

    }
}
