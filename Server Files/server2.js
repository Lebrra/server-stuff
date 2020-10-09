const { emit } = require('cluster');

// JavaScript source code
var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

const PORT = process.env.PORT || 3000;

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/index.html');
});

var Users = new Map();  // declaring Users structure
var lobbyActive = '';

io.sockets.on('connection', (socket) => {
    console.log('a user connected');

    socket.emit('connectionmessage', {
        id: socket.id
    });

    // add new user to User Map
    addUser(socket);

    socket.on('updateUsername', (newName) => {
        addUsername(newName);
    });

    socket.on('buttonClicked', (number) => {
        console.log('button pressed ' + number);
        io.emit('serverMessage', { message: `button ${number}` });
    });

    socket.on('createNewLobby', () => {
        console.log(Users.get(socket.id)['username'] + ' has started a lobby');
        lobbyActive = socket.id;
        socket.emit('enableLobby');
    });

    socket.on('joinLobby', () => {
        if (lobbyActive == '') console.log(Users.get(socket.id)['username'] + ' cannot join lobby, none exist!');
        else {
            console.log(Users.get(socket.id)['username'] + ' has joined the lobby');
            socket.emit('enableLobby');
        }
    });

    socket.on('leaveLobby', () => {
        if (lobbyActive == socket.id) {
            console.log('the host has left the lobby, closting lobby...');
            lobbyActive = '';
            io.emit('disableLobby');
        }
        else {
            console.log(Users.get(socket.id)['username'] + ' has left the lobby');
            socket.emit('disableLobby');
        }
    });

    socket.on('startGame', () => {
        console.log(Users.get(socket.id)['username'] + ' has started the game!');
        lobbyActive = '';
        io.emit('disableLobby');
    });

    socket.on('disconnect', () => {
        console.log('user disconnected');
        io.emit('removeUser', { id: socket.id });
        removeUser(socket);
    });

    function changeUserProperty(property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (Users.has(socket.id)) {
            tempObj = Users.get(socket.id);
            // console.log('changed current user property: ' + property);
            tempObj[property] = value;
            Users.set(socket.id, tempObj);
        }
        checkUsers();
    }

    function addUsername(newUsername) {
        // coming back to this
        changeUserProperty('username', newUsername);
    }

    function addUser(socket) {
        // create a new user mapping
        if (!Users.has(socket.id)) {
            Users.set(
                socket.id,
                {
                    username: "----",
                    id: socket.id
                }
            );
            checkUsers();
        }
    };

    function removeUser(socket) {
        if (Users.has(socket.id)) {
            Users.delete(socket.id);
            checkUsers();
        }
    };

    function checkUsers() {
        console.table(Users);
        listUsers();
    };

    function listUsers() {

        /*  // Giving array of just usernames
        let tempUsers = Array.from(Users.values());
        var usernameObject = {};
        for (var i = 0; i < tempUsers.length; i++) {
            usernameObject[i] = tempUsers[i]["username"];
        }
        //console.table(usernameObject);
        io.emit('users', usernameObject);
        */

            // Giving id, name pairs
        let tempUsers = Array.from(Users.values());
        var usernameObject = {};
        for (var i = 0; i < tempUsers.length; i++) {
            usernameObject[i] = {
                username: tempUsers[i]["username"],
                id: tempUsers[i]["id"]
            };
        }
        io.emit('users', usernameObject);
    }
});

server.listen(PORT);