const { Console } = require('console');

// JavaScript source code
var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

const PORT = process.env.PORT || 3000;

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/index.html');
});

var Users = new Map(); // declaring our Users structure

// <socketid, { 
//     username, socketid, playState
// }>, <key, value>

io.sockets.on('connection', (socket) => {
    console.log('a user connected');

    socket.emit('connectionmessage', {
        id: socket.id
    });

    // add new user to User Map
    addUser(socket);

    socket.on('buttonClicked', (number) => {
        console.log('button pressed ' + number);
        io.emit('serverMessage', { message: `button ${number}` });
    });

    socket.on('disconnect', () => {
        console.log('user disconnected');
        removeUser(socket);
    });

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

    function removeUser(socket){
        if (Users.has(socket.id)) {
            Users.delete(socket.id);
            checkUsers();
        }
    }

    function checkUsers(){
        console.table(Users);
    }
});

server.listen(PORT);