using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyFunctions : MonoBehaviour
{
    public static LobbyFunctions inst;

    public bool inLobby = false;

    public InputField roomInput;
    public GameObject joinBox;
    public GameObject lobbyButtons;
    public GameObject usernameInput;

    public Text roomNameText;
    public GameObject backButton;

    private void Awake()
    {
        if (inst) Destroy(gameObject);
        else inst = this;
    }

    public void createGameBtn()
    {
        nh_network.server.createNewLobby();
        lobbyButtons.SetActive(false);
        usernameInput.SetActive(false);
        backButton.SetActive(true);
    }

    public void joinGameBtn()
    {
        lobbyButtons.SetActive(false);
        usernameInput.SetActive(false);
        joinBox.SetActive(true);
        backButton.SetActive(true);
    }

    public void joinLobbyInput()
    {
        if (roomInput.text != "")
        {
            nh_network.server.joinLobby(roomInput.text);
        }
    }

    public void clearInput(InputField field)
    {
        field.text = "";
    }

    public void goBack()
    {
        if (inLobby)    // leaving lobby
        {
            nh_network.server.leaveRoom();
            GetComponent<UsernameActions>().removeAllUsernames();
            roomNameText.text = "- Main Lobby -";
            // load main lobby usernames
            inLobby = false;
        }

        lobbyButtons.SetActive(true);
        usernameInput.SetActive(true);
        backButton.SetActive(false);
        joinBox.SetActive(false);
    }

    public void enterRoom(string roomname)
    {
        if (joinBox.activeInHierarchy) joinBox.SetActive(false);
        roomNameText.text = "Room Code: " + roomname;
        GetComponent<UsernameActions>().removeAllUsernames();
        // load room users
        inLobby = true;
    }
}
