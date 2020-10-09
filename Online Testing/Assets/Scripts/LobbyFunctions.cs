using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyFunctions : MonoBehaviour
{
    public static LobbyFunctions inst;

    public GameObject lobbyButtons;

    private void Awake()
    {
        if (inst) Destroy(gameObject);
        else inst = this;

        lobbyButtons.SetActive(false);
    }

    public void hostLobby()
    {
        nh_network.server.createNewLobby();
    }

    public void joinLobby()
    {
        nh_network.server.joinLobby();
    }

    public void startGame()
    {
        nh_network.server.startLobby();
    }

    public void leaveLobby()
    {
        nh_network.server.leaveLobby();
    }
}
