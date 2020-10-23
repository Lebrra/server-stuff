using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsernameActions : MonoBehaviour
{
    public InputField nameField;

    public GameObject usernameList;
    public GameObject textPrefab;
    Dictionary<string, GameObject> usernameTexts;

    private void Start()
    {
        usernameTexts = new Dictionary<string, GameObject>();

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

            // maybe sets username here
            nameField.placeholder.GetComponent<Text>().text = nameField.text;
            nameField.text = "";
        }
    }

    public void addUsername(string id, string name)
    {
        if (usernameTexts.ContainsKey(id))
        {
            usernameTexts[id].GetComponent<Text>().text = name;
        }
        else
        {
            GameObject newText = Instantiate(textPrefab, usernameList.transform);
            newText.GetComponent<Text>().text = name;
            usernameTexts.Add(id, newText);
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
}
