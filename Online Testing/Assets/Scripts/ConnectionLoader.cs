using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLoader : MonoBehaviour
{
    public static ConnectionLoader inst;
    public GameObject myScreen;

    private void Awake()
    {
        inst = this;
        myScreen.SetActive(true);
    }

    public void DisableScreen()
    {
        Debug.Log("disable screen");
        myScreen.SetActive(false);
        inst = null;
    }

    IEnumerator ShowError()
    {
        yield return new WaitForSecondsRealtime(30F);

        // not connecting, show message 'it looks like the server might be down, please try again later!' and a quit button
    }
}
