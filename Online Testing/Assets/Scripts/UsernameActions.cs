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

    public void clearField()
    {
        nameField.text = "";
    }

    public void acceptName()
    {
        if(nameField.text != "")
        {
            //local testing
            //removeUsername(nameField.placeholder.GetComponent<Text>().text);
            //addUsername(nameField.text);

            // maybe sets username here
            nameField.placeholder.GetComponent<Text>().text = nameField.text;
            nameField.text = "";
        }
    }

    public void addUsername(string name)
    {
        GameObject newText = Instantiate(textPrefab, usernameList.transform);
        newText.GetComponent<Text>().text = name;
        usernameTexts.Add(name, newText);
    }

    public void removeUsername(string name)
    {
        if (usernameTexts.ContainsKey(name))
        {
            GameObject remove = usernameTexts[name];
            usernameTexts.Remove(name);
            Destroy(remove);
        }
    }
}
