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
        socket.On("users", loadUsers);
        socket.On("roomUsers", loadRoomUsers);
        socket.On("removeUser", removeUser);
        socket.On("createdRoom", createdRoom);
        socket.On("loadGame", loadGame);
        socket.On("currentRound", roundInfo);
        socket.On("currentTurn", turnInfo);
        socket.On("playerHand", handInfo);
        socket.On("addToDiscard", discardInfo);
        socket.On("newCard", newCardInfo);
        socket.On("yourTurn", myTurn);
        socket.On("drewFromDeck", drewFromDeck);
        socket.On("drewFromDiscard", drewFromDiscard);
        
        socket.On("ping", ping);

    }

    #region Connection/Room Functions

    // This is the listener function definition
    void onConnectionEstabilished(SocketIOEvent evt)
    {
        Debug.Log("Player is connected: " + evt.data.GetField("id"));
    }

    public void createNewLobby()
    {
        //socket.Emit("createNewLobby");
        socket.Emit("createRoom");
    }

    public void joinRoom(string roomName)
    {
        //socket.Emit("joinLobby");
        socket.Emit("joinRoom", new JSONObject(quote + roomName + quote));
    }

    public void leaveRoom()
    {
        socket.Emit("leaveRoom");
    }

    void createdRoom(SocketIOEvent evt)
    {
        Debug.Log("Created new room: " + evt.data.GetField("name"));
        LobbyFunctions.inst.enterRoom(evt.data.GetField("name").ToString().Trim('"'));
    }
    #endregion
    #region Username Functions

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


    void removeUser(SocketIOEvent evt)
    {
        ua.removeUsername(evt.data.GetField("id").ToString().Trim('"'));
    }
    #endregion
    #region Game Functions
    public void startGame()
    {
        // tells server to start game
        socket.Emit("startGame");
    }

    public void setReady()
    {
        socket.Emit("setReady");
    }

    void loadGame(SocketIOEvent evt)
    {
        Debug.Log("The game has been started!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    void roundInfo(SocketIOEvent evt)
    {
        int round;
        int.TryParse(evt.data.GetField("round").ToString().Trim('"'), out round);
        GameManager.instance.round = round;
        Debug.Log("The current round is: " + round);
    }

    void turnInfo(SocketIOEvent evt)
    {
        string player = evt.data.GetField("player").ToString().Trim('"');
        Debug.Log("It is " + player + "'s turn.");
    }

    void myTurn(SocketIOEvent evt)
    {
        Debug.Log("Its my turn!");
        GameManager.instance.myDraw = GameManager.instance.myTurn = true;
        var notification = new Notification("It is your turn", 3, true, Color.black );
        NotificationManager.instance.addNotification(notification);
    }

    #region Card Functions
    void handInfo(SocketIOEvent evt)
    {
        //List<string> newHand = new List<string>();
        Debug.Log("My hand:");

        for (int i = 0; i < evt.data.Count; i++)
        {
            string card = evt.data.GetField(i.ToString()).ToString().Trim('"');
            //newHand.Add(card);
            Debug.Log(card);
            GameManager.instance.addCardToHand(card);
        }
    }

    void discardInfo(SocketIOEvent evt)
    {
        string discard = evt.data.GetField("card").ToString().Trim('"');
        Debug.Log("Discarded " + discard);
        GameManager.instance.addCardToDiscard(discard);
        
        // NOTIFICATION HAPPENS IN GAMEMANAGER
    }

    public void drawCard(bool fromDeck)
    {
        JSONObject jsonObject = new JSONObject(fromDeck);
        socket.Emit("drawCard", jsonObject);
        
        // NOTIFICATION HAPPENS IN GAMEMANAGER
    }

    void newCardInfo(SocketIOEvent evt)
    {
        string card = evt.data.GetField("card").ToString().Trim('"');
        Debug.Log("Card selected: " + card);
        GameManager.instance.addCardToHand(card);
        var notification = new Notification($"Drew {card}", 3, true, Color.black );
        NotificationManager.instance.addNotification(notification);
    }

    void drewFromDeck(SocketIOEvent evt)
    {
        string playername = evt.data.GetField("player").ToString().Trim('"');
        Debug.Log(playername + " has drawn a card from the deck!");
        var notification = new Notification(playername + " has drawn a card from the deck!", 3, true, Color.black );
        NotificationManager.instance.addNotification(notification);
    }

    void drewFromDiscard(SocketIOEvent evt)
    {
        string playername = evt.data.GetField("player").ToString().Trim('"');
        Debug.Log(playername + " has drawn from the discard pile!");
        // update visuals of discard pile here
        var notification = new Notification(playername + " has drawn from the discard pile!", 3, true, Color.black );
        NotificationManager.instance.addNotification(notification);
    }

    public void discardCard(string cardname)
    {
        JSONObject jsonObject = new JSONObject(quote + cardname + quote);
        socket.Emit("discardCard", jsonObject);
    }

    public void ping(SocketIOEvent socketIOEvent)
    {
        Debug.Log("Ping");
        ConnectionIndicator.instance?.Ping();
    }

    public void SendFirstOut(JSONObject outDeck)
    {
        JSONObject cardArr = JSONObject.Create(JSONObject.Type.ARRAY);
        print("type? " + cardArr.type);
        cardArr.Add(Out.Run.ToString());
        cardArr.Add("joker_2");
        cardArr.Add("Club_3");
        cardArr.Add("Club_4");
        print("contents? - " + cardArr);

        JSONObject OutDeck = new JSONObject();
        OutDeck.AddField("out1", cardArr);
        OutDeck.AddField("out2", cardArr);
        print("test? - " + OutDeck);

        socket.Emit("dictionaryTest", outDeck);
    }

    #endregion

    #endregion
}