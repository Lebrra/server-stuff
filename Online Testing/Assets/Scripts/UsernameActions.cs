using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UsernameActions : MonoBehaviour
{
    //public InputField nameField;
    public TMP_InputField nameField;

    public GameObject usernameList;
    public GameObject textPrefab;
    Dictionary<string, GameObject> usernameTexts;

    public Animator message;

    PlayerData data;
    string myID;

    private void Start()
    {
        usernameTexts = new Dictionary<string, GameObject>();

        data = SaveLoad.Load();
        myID = "";
        nameField.placeholder.GetComponent<TextMeshProUGUI>().text = data.username;

        //local testing
        //addUsername(nameField.placeholder.GetComponent<Text>().text);
    }

    public void acceptName()
    {
        if(nameField.text != "")
        {
            //local testing
            //removeUsername(nameField.placeholder.GetComponent<Text>().text);
            //addUsername(nameField.text);

            nh_network.server.newUsername(nameField.text);
            //PlayerPrefs.SetString("username", nameField.text);

            data = SaveLoad.Load();
            data.username = nameField.text;
            SaveLoad.Save(data);

            // maybe sets username here
            //nameField.placeholder.GetComponent<Text>().text = nameField.text;
            nameField.placeholder.GetComponent<TextMeshProUGUI>().text = nameField.text;
            nameField.text = "";

            message.SetTrigger("flash");
        }

        //LobbyFunctions.inst.openUsernamePanel(false);
    }

    public void addUsername(string id, string name)
    {
        if (usernameTexts.ContainsKey(id))
        {
            usernameTexts[id].GetComponent<TextMeshProUGUI>().text = name;
        }
        else
        {
            GameObject newText = Instantiate(textPrefab, usernameList.transform);
            usernameTexts.Add(id, newText);

            if (myID == "")
            {
                // my name not set yet
                Debug.Log("my username is not loaded yet");
                myID = id;
                StartCoroutine(DelayUsernameUpdate(data.username));
                newText.GetComponent<TextMeshProUGUI>().text = "loading...";
            }
            else
            {
                // not my name, new user
                newText.GetComponent<TextMeshProUGUI>().text = name;
            }
        }
    }

    public void removeUsername(string id)
    {
        if (usernameTexts.ContainsKey(id))
        {
            GameObject remove = usernameTexts[id];
            usernameTexts.Remove(id);
            Destroy(remove);
        }
        else
        {
            Debug.LogError("user not found, could not remove from list");
        }
    }

    public void removeAllUsernames()
    {
        foreach(GameObject a in usernameTexts.Values) Destroy(a);
        usernameTexts = new Dictionary<string, GameObject>();
    }

    IEnumerator DelayUsernameUpdate(string name)
    {
        yield return new WaitForSeconds(0.3F);
        
        nh_network.server.newUsername(name);
    }
}
