using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class NotificationsPanel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public bool isLive = false;

    public GameObject textPrefab;
    public Transform contentLocation;
    List<GameObject> spawnedTexts = new List<GameObject>();
    int currentLoaded = 0;

    List<(string, string)> activeMessages;

    private void OnEnable()
    {
        // load all messages onto the screen
        isLive = true;
        LoadMessages();
    }

    public void RoundReset()
    {
        activeMessages = new List<(string, string)>();
        foreach(GameObject text in spawnedTexts)
        {
            if (text.activeInHierarchy)
            {
                text.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100); // does this work?
                text.SetActive(false);
            }
        }

        currentLoaded = 0;

        isLive = false;
        gameObject.SetActive(false);
    }

    public void AddMessage(string message)  // if message needs another line, add 50 to the prefab, roughly 45 characters to a line
    {
        int currentIndex = activeMessages.Count;

        (string, string) messageData = (message, (System.DateTime.Now.Hour % 12).ToString() + ":" + System.DateTime.Now.Minute.ToString());
        activeMessages.Add(messageData);

        Debug.Log("Message: " + messageData.Item1);
        Debug.Log("Time: " + messageData.Item2);

        if (isLive) LoadMessages();
    }

    public void LoadMessages()
    {
        while (currentLoaded < activeMessages.Count)
        {
            GameObject textObject; 

            if (currentLoaded < spawnedTexts.Count)
            {
                // take from pool
                textObject = spawnedTexts[currentLoaded];
                textObject.SetActive(true);
            }
            else
            {
                // instantiate
                textObject = Instantiate(textPrefab, contentLocation);
                textObject.transform.localScale *= FindObjectOfType<ResolutionManager>().convertionRatio;       // this isn't good, fix this
                spawnedTexts.Add(textObject);
            }

            textObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = activeMessages[currentLoaded].Item1;
            textObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = activeMessages[currentLoaded].Item2;

            currentLoaded++;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click");

        // close this screen
        isLive = false;
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("click");
        // having this makes the other method work, look into why
    }
}
