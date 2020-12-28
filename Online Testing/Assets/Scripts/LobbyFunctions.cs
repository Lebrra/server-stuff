using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using TMPro;

public class LobbyFunctions : MonoBehaviour
{
    public static LobbyFunctions inst;

    public bool inRoom = false;

    public TMP_InputField roomInput;
    public GameObject joinBox;
    public GameObject lobbyButtons;
    public GameObject usernameInput;

    public TextMeshProUGUI roomNameText;
    public GameObject backButton;

    public GameObject playButton;

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
            nh_network.server.joinRoom(roomInput.text);
        }
    }

    public void clearInput(TMP_InputField field)
    {
        field.text = "";
    }

    public void goBack()
    {
        if (inRoom)    // leaving lobby
        {
            nh_network.server.leaveRoom();
            GetComponent<UsernameActions>().removeAllUsernames();
            roomNameText.text = "- Main Lobby -";
            // load main lobby usernames
            inRoom = false;
            playButton.SetActive(false);
        }

        lobbyButtons.SetActive(true);
        //usernameInput.SetActive(true);
        backButton.SetActive(false);
        joinBox.SetActive(false);
    }

    public void enterRoom(string roomname)
    {
        if (joinBox.activeInHierarchy) joinBox.SetActive(false);
        roomNameText.text = "Room Code: " + roomname;
        GetComponent<UsernameActions>().removeAllUsernames();
        playButton.SetActive(true);
        inRoom = true;
    }

    public void playGame()
    {

        nh_network.server.startGame();
    }

    public void openUsernamePanel(bool open)
    {
        usernameInput.SetActive(open);
    }
}
