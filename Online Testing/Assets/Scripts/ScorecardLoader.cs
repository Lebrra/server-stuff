using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorecardLoader : MonoBehaviour
{
    public static ScorecardLoader inst;

    public bool firstout = false;
    public Color32 WinnerColor;

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
        for(int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == null) break;

            for(int j = 0; j < scores[i].Length; j++)
            {
                if(scores[i][j] == 0)
                {
                    if (firstout) scorecardTexts[i].roundRow[j].text = "X";
                    else scorecardTexts[i].roundRow[j].text = "-";
                }
                else scorecardTexts[i].roundRow[j].text = scores[i][j].ToString();
            }
        }

        CalculateTotals(scores);
    }

    public void CalculateTotals(int[][] scores)
    {
        int[] totals = new int[scores.Length];
        for (int i = 0; i < totals.Length; i++) totals[i] = 0;

        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == null) break;

            for (int j = 0; j < scores[i].Length; j++)
            {
                totals[j] += scores[i][j];
            }
        }

        List<int> winning = new List<int>();
        for (int i = 0; i < totals.Length; i++)
        {
            totalScores[i].text = totals[i].ToString();

            if (winning.Count == 0) winning.Add(i);
            else if (totals[winning[0]] == totals[i]) winning.Add(i);
            else if(totals[winning[0]] > totals[i])
            {
                winning.Clear();
                winning.Add(i);
            }
        }

        foreach (int i in winning)
            totalScores[i].color = WinnerColor;
    }
}

[System.Serializable]
public struct ScorecardTexts
{
    public TextMeshProUGUI[] roundRow;
}