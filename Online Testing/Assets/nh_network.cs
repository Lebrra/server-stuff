using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class nh_network : MonoBehaviour
{
    public static nh_network server;

    [TextArea(3, 10)]
    public string serverQuickRef;

    SocketIOComponent socket;

    LobbyFunctions lf;
    UsernameActions ua;
    char quote = '"';

    //public playerManager playerManager;

    private void Awake()
    {
        if (server) Destroy(gameObject);
        else server = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ua = FindObjectOfType<UsernameActions>();
        lf = FindObjectOfType<LobbyFunctions>();
        socket = GetComponent<SocketIOComponent>();

        //playerManager = GameObject.Find("Player Manager").GetComponent<playerManager>();
     
        // The lines below setup 'listener' functions
        socket.On("connectionmessage", onConnectionEstabilished);
        socket.On("serverMessage", serverMessage);
        socket.On("users", loadUsers);
        socket.On("roomUsers", loadRoomUsers);
        socket.On("removeUser", removeUser);
        socket.On("disableLobby", disableLobby);
        socket.On("enableLobby", enableLobby);
        socket.On("createdRoom", createdRoom);
        socket.On("loadGame", loadGame);
        socket.On("currentRound", roundInfo);
        socket.On("currentTurn", turnInfo);
        socket.On("playerHand", handInfo);
        socket.On("addToDiscard", discardInfo);
    }

    void onYes(SocketIOEvent evt){
        //Debug.Log("OH SHIT" + evt.data.GetField("id"));

    }

    // This is the listener function definition
    void onConnectionEstabilished(SocketIOEvent evt)
    {
        Debug.Log("Player is connected: " + evt.data.GetField("id"));
    }

    // this checks the data from the event, parses the two parts by their id.
    void onAddUser(SocketIOEvent evt)
    {
        string id = evt.data.GetField("id").ToString();
        string username = evt.data.GetField("username").ToString();
        //Debug.Log("ID: " + id + " logged in as " + username);
        //playerManager.addPlayer(id, username);
    }

    void onAddFBUser(SocketIOEvent evt)
    {
        string id = evt.data.GetField("id").ToString();
        string username = evt.data.GetField("username").ToString();
        string imageurl = evt.data.GetField("fbimageurl").ToString();

        //Debug.Log("ID: " + id + " logged in as " + username);
        print("new FB user" + imageurl);
       // playerManager.addPlayer(id, username, imageurl);
    }

    void onRemoveUser(SocketIOEvent evt){
        string id = evt.data.GetField("id").ToString();
        string username = evt.data.GetField("username").ToString();
        Debug.Log("-----ID: " + id + " disconnected. Trying to remove " + username);
        //playerManager.removePlayer(id, username);
        //playerManager.removePlayer(id);
    }

    void onAvatarSelected(SocketIOEvent evt){
        string id = evt.data.GetField("id").ToString();
        Debug.Log(id + " avatar selected");
    }

    void onShootPressed(SocketIOEvent evt){
        string id = evt.data.GetField("id").ToString();
        Debug.Log(id + " pressed shoot!");
        //playerManager.playerShoot(id);
    }



    void disableLobby(SocketIOEvent evt)
    {
        //LobbyFunctions.inst.lobbyButtons.SetActive(false);
    }
    void enableLobby(SocketIOEvent evt)
    {
        //LobbyFunctions.inst.lobbyButtons.SetActive(true);
    }


    void serverMessage(SocketIOEvent evt)
    {
        Debug.Log("woot");
    }

    public void createNewLobby()
    {
        //socket.Emit("createNewLobby");
        socket.Emit("createRoom");
    }
    public void joinLobby(string roomName)
    {
        //socket.Emit("joinLobby");
        socket.Emit("joinRoom", new JSONObject(quote + roomName + quote));
    }
    public void leaveLobby()
    {
        socket.Emit("leaveLobby");
    }
    public void startLobby()
    {
        socket.Emit("startGame");
    }

    #region Username Methods

    public void newUsername(string name)
    {
        name = quote + name + quote;

        JSONObject test = new JSONObject(name);
        socket.Emit("updateUsername", test);
    }

    void loadUsers(SocketIOEvent evt)
    {
        if (!lf.inRoom) {
            Debug.Log("loading usernames...");

            for (int i = 0; i < evt.data.Count; i++)
            {
                JSONObject jsonData = evt.data.GetField(i.ToString());

                Debug.Log(jsonData.GetField("username"));
                ua.addUsername(jsonData.GetField("id").ToString().Trim('"'), jsonData.GetField("username").ToString().Trim('"'));
            } 
        }
    }

    void loadRoomUsers(SocketIOEvent evt)
    {
        Debug.Log("loading room usernames...");
        ua.removeAllUsernames();

        for (int i = 0; i < evt.data.Count; i++)
        {
            JSONObject jsonData = evt.data.GetField(i.ToString());

            Debug.Log(jsonData.GetField("username"));
            ua.addUsername(jsonData.GetField("id").ToString().Trim('"'), jsonData.GetField("username").ToString().Trim('"'));
        }
    }

    public void leaveRoom()
    {
        socket.Emit("leaveRoom");
    }

    void removeUser(SocketIOEvent evt)
    {
        ua.removeUsername(evt.data.GetField("id").ToString().Trim('"'));
    }
    #endregion

    void createdRoom(SocketIOEvent evt)
    {
        Debug.Log("Created new room: " + evt.data.GetField("name"));
        LobbyFunctions.inst.enterRoom(evt.data.GetField("name").ToString().Trim('"'));
    }

    void joinedRoom(SocketIOEvent evt)  // does this need to be different than createdRoom() ?
    {
        Debug.Log("Created new room: " + evt.data.GetField("name"));
        LobbyFunctions.inst.enterRoom(evt.data.GetField("name").ToString().Trim('"'));
    }

    public void startGame()
    {
        // tells server to start game
        socket.Emit("startGame");
    }

    void loadGame(SocketIOEvent evt)
    {
        Debug.Log("the game has been started!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    void roundInfo(SocketIOEvent evt)
    {
        int round;
        int.TryParse(evt.data.GetField("round").ToString().Trim('"'), out round);
        Debug.Log("The current round is: " + round);
    }

    void turnInfo(SocketIOEvent evt)
    {
        string player = evt.data.GetField("player").ToString().Trim('"');
        Debug.Log("It is " + player + "'s turn.");
    }

    void handInfo(SocketIOEvent evt)
    {
        List<string> newHand = new List<string>();
        Debug.Log("My hand:");

        for (int i = 0; i < evt.data.Count; i++)
        {
            string jsonData = evt.data.GetField(i.ToString()).ToString().Trim('"');
            newHand.Add(jsonData);
            Debug.Log(jsonData);
        }
    }

    void discardInfo(SocketIOEvent evt)
    {
        string discard = evt.data.GetField("card").ToString().Trim('"');
        Debug.Log("Discarded " + discard);
    }
}