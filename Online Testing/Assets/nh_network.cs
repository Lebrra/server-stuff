using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class nh_network : MonoBehaviour
{
    [TextArea]
    public string serverQuickRef;

    SocketIOComponent socket;

    //public playerManager playerManager;

    void Start()
    {
        socket = GetComponent<SocketIOComponent>();

        //playerManager = GameObject.Find("Player Manager").GetComponent<playerManager>();
     
        // The lines below setup 'listener' functions
        socket.On("connectionmessage", onConnectionEstabilished);
        socket.On("serverMessage", serverMessage);
        socket.On("users", loadUsers);

        socket.On("unityAddUser", onAddUser);
        socket.On("unityAddFBUser", onAddFBUser);
        socket.On("unityRemoveUser", onRemoveUser);
        socket.On("poop", onYes);
        socket.On("unityShootPressed", onShootPressed);
        socket.On("unityAvatarSelected", onAvatarSelected);
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




    void serverMessage(SocketIOEvent evt)
    {
        Debug.Log("woot");
    }

    public void button(int num)
    {
        JSONObject test = new JSONObject(num);
        socket.Emit("buttonClicked", test);
    }

    public void newUsername(string name)
    {
        char quote = '"';
        name = quote + name + quote;

        JSONObject test = new JSONObject(name);
        socket.Emit("updateUsername", test);
    }

    void loadUsers(SocketIOEvent evt)
    {
        for(int i = 0; i < evt.data.Count; i++)
        {
            string thing = evt.data.GetField(i.ToString()).ToString();
            Debug.Log(thing.Trim('"'));
        }
    }
}