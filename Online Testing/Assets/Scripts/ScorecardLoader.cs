using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScorecardLoader : MonoBehaviour
{
    public static ScorecardLoader inst;

    public int firstoutplayer = -1;
    public Color32 defaultColor;
    public Color32 WinnerColor;

    [Header("Waiting")]
    public GameObject waitingPopup;
    public TextMeshProUGUI waitingScore;

    public Button deckBtn;
    public Button discBtn;

    [Header("Scorecard")]
    public GameObject scoreCard;
    public TextMeshProUGUI[] nameTexts;
    public ScorecardTexts[] scorecardTexts;
    public TextMeshProUGUI[] totalScores;
    int currentColumn;

    public GameObject showScoreBtn;
    public GameObject closeScoreBtn;

    [Header("Ready Checking")]
    public GameObject readycheck;
    public TextMeshProUGUI readyText;
    public bool ready;

    [Header("Game Over")]
    public bool gameover = false;
    public GameObject gameoverScreen;

    int[][] loadedScores;

    private void Awake()
    {
        inst = this;
        gameObject.SetActive(false);

        currentColumn = -1;
    }

    public void EnableWait(int score)
    {
        waitingPopup.SetActive(true);
        waitingScore.text = "Your score this round: <#1D6820>" + score.ToString();

        deckBtn.interactable = false;
        discBtn.interactable = false;
    }

    public void DisableWait()
    {
        waitingPopup.SetActive(false);
        scoreCard.SetActive(true);
    }

    public void LoadNames(string[] playerNames)
    {
        if (currentColumn == -1)
        {
            for (int i = 0; i < playerNames.Length; i++)
            {
                nameTexts[i].text = playerNames[i];
            }

            currentColumn++;
        }
    }

    public void LoadScores(int[][] scores)
    {
        /*for(int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == null) break;

            for(int j = 0; j < scores[i].Length; j++)
            {
                if(scores[i][j] == 0)
                {
                    Debug.Log("who was first out? " + firstoutplayer);
                    if (firstoutplayer == j) scorecardTexts[i].roundRow[j].text = "X";
                    else scorecardTexts[i].roundRow[j].text = "-";
                }
                else scorecardTexts[i].roundRow[j].text = scores[i][j].ToString();
            }
        }*/

        if (scores[currentColumn] == null) Debug.LogError("no scores found in column " + currentColumn);

        for (int j = 0; j < scores[currentColumn].Length; j++)
        {
            if (scores[currentColumn][j] == 0)
            {
                Debug.Log("who was first out? " + firstoutplayer);
                if (firstoutplayer == j) scorecardTexts[currentColumn].roundRow[j].text = "X";
                else scorecardTexts[currentColumn].roundRow[j].text = "-";
            }
            else scorecardTexts[currentColumn].roundRow[j].text = scores[currentColumn][j].ToString();
        }

        currentColumn++;

        CalculateTotals(scores);
        gameObject.GetComponent<Image>().enabled = false;
        StartCoroutine(DelayReadyButton());
    }

    public void CalculateTotals(int[][] scores)
    {
        foreach (TextMeshProUGUI t in totalScores) t.color = defaultColor;

        Debug.Log("calculating totals...");

        int[] totals = new int[scores[0].Length];
        for (int i = 0; i < totals.Length; i++) totals[i] = 0;

        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] == null) break;

            for (int j = 0; j < scores[0].Length; j++)
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

    public void ReadyCheck()
    {
        if (!ready)
        {
            ready = true;
            if (!gameover)
            {
                nh_network.server.setReady();
                readyText.text = "Waiting for other players...";
                gameObject.GetComponent<Image>().enabled = true;
                deckBtn.interactable = true;
                discBtn.interactable = true;
            }
            else
            {
                //end game things
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                nh_network.server.destroyGameData();
            }
        }
    }

    public void reset()
    {
        ready = false;
        readycheck.SetActive(false);
        scoreCard.SetActive(false);
        gameObject.SetActive(false);

        if (!showScoreBtn.activeInHierarchy) showScoreBtn.SetActive(true);

        gameObject.GetComponent<Image>().enabled = false;
    }

    IEnumerator DelayReadyButton()
    {
        yield return new WaitForSeconds(5F);

        readycheck.SetActive(true);
        if (!gameover) readyText.text = "Tap anywhere to start next round!";
        else readyText.text = "The game has ended, tap anywhere to continue...";
    }

    public void OpenScoreCard()
    {
        scoreCard.SetActive(true);
        closeScoreBtn.SetActive(true);
    }

    public void CloseScoreCard()
    {
        closeScoreBtn.SetActive(false);
        scoreCard.SetActive(false);
    }
}

[System.Serializable]
public struct ScorecardTexts
{
    public TextMeshProUGUI[] roundRow;
}