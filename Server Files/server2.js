// JavaScript source code
var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

const PORT = process.env.PORT || 3000;

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/index.html');
});

io.sockets.on('connection', (socket) => {
    console.log('a user connected');

    socket.emit('connectionmessage', {
        id: socket.id
    });

    socket.on('buttonClicked', (number) => {
        console.log('button pressed ' + number);
        io.emit('serverMessage', {message: `button ${number}`});
    });

    socket.on('disconnect', () => {
        console.log('user disconnected');
    });                                                          
});

server.listen(PORT);