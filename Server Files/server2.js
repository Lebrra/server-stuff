const { emit } = require('cluster');
const { debug } = require('console');

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
var Rooms = [];
var Games = [];

var Animals = ['Possum', 'Frog', 'Zebra', 'Lizard', 'Beaver', 'Panda', 'Giraffe', 'Toucan', 'Pelican', 'Sloth', 'Alligator', 'Scorpion', 'Viper', 'Armadillo'];
var Deck;
var DiscardPile;

const
    states = {
        ROOM: 'room',
        LOBBY: 'lobby',
        GAME: 'game'
    }

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

    socket.on('disconnect', () => {
        console.log('user disconnected');
        io.emit('removeUser', { id: socket.id });
        removeUser(socket);
    });

    // socket event to intiate room creation
    socket.on('createRoom', () => {
        var newRoomName = generateRoomName();
        socket.join(newRoomName);
        changeUserProperty("room", newRoomName);
        changeUserProperty("state", states.ROOM);
        socket.emit('createdRoom', { name: newRoomName });
        //send room name to unity of the creator only 
        listRoomUsers();
    });

    socket.on('joinRoom', (roomName) => {
        console.log('Room name given: ' + roomName);

        if (Rooms.includes(roomName)) {
            socket.join(roomName);
            changeUserProperty("room", roomName);
            changeUserProperty("state", states.ROOM);
            console.log(socket.adapter.rooms[Users.get(socket.id).room].sockets);
            //console.table(Users.get(socket.id).room);
            socket.emit('createdRoom', { name: roomName });
            console.log(Users.get(socket.id).username + ' joined room ' + roomName);
            listRoomUsers();
        }
        else {
            // room does not exist
            console.log("Cannot join; room does not exist.");
        }
    });

    socket.on('leaveRoom', () => {
        console.log(Users.get(socket.id).username + ' left their room');

        var formerRoom = Users.get(socket.id).room;

        socket.leave(Users.get(socket.id).room);
        changeUserProperty("room", "");
        changeUserProperty("state", states.LOBBY);
        //console.log(socket.adapter.rooms[Users.get(socket.id).room].sockets);

        updateFormerRoomList(formerRoom);
    });

    socket.on('startGame', () => {
        console.log(Users.get(socket.id)['username'] + ' has started the game!');
        var roomStarted = Users.get(socket.id)['room'];

        for (user in socket.adapter.rooms[Users.get(socket.id).room].sockets) {
            changeUserPropertyWithID(user.id, 'state', states.GAME);
        }
        console.table(Users);

        io.in(roomStarted).emit('loadGame');

        // make new game instance here
        // var game = new Game(listRoomUsers());
        Games[roomStarted] = new Game(listRoomUsers(), roomStarted); // assigned new game to Games array based on roomname
    });

    socket.on('drawCard', (fromDeck) => {
        var newCard;
        if (fromDeck === true) {
            newCard = Games[Users.get(socket.id)['room']].drawCard();
        }
        else {
            newCard = Games[Users.get(socket.id)['room']].drawFromDiscard();
        }
        console.log(newCard);
        socket.emit('newCard', {card: newCard});
    })

    socket.on('setReady', () => {
        console.log('checking for all ready...');
        Games[Users.get(socket.id)['room']].readyCheck(socket.id);
    });

    // send usernames in our room
    function listRoomUsers() {
        console.table(socket.adapter.rooms[Users.get(socket.id).room].sockets);
        let _sockets = socket.adapter.rooms[Users.get(socket.id).room].sockets;
        let tempUsers = [];
        for (_socket in _sockets) {
            tempUsers.push(
                {
                    username: getUsernameFromSocketID(_socket),
                    id: _socket
                });
        }

        var roomDetails = {
            ...tempUsers
        }

        console.log('--- Room Details ---');
        console.table(roomDetails);
        io.in(Users.get(socket.id).room).emit('roomUsers', roomDetails);        // sending to all users within a room
        return tempUsers;
    }

    // update users in previous rooms
    function updateFormerRoomList(formerRoom) {
        if (socket.adapter.rooms[formerRoom] != null || socket.adapter.rooms[formerRoom] != undefined) {
            console.table(socket.adapter.rooms[formerRoom].sockets);
            let _sockets = socket.adapter.rooms[formerRoom].sockets;
            let tempUsers = [];
            for (_socket in _sockets) {
                tempUsers.push(
                    {
                        username: getUsernameFromSocketID(_socket),
                        id: _socket
                    });
            }

            var roomDetails = {
                ...tempUsers
            }

            console.log('--- Room Details ---');
            console.table(roomDetails);
            socket.to(formerRoom).emit('roomUsers', roomDetails);
            //io.emit('roomUsers', roomDetails);
        }
    }

    function getUsernameFromSocketID(socketid) {
        if (socketid == null) {
            socketid = socket.id;
        }
        if (Users.get(socketid) != undefined || Users.get(socketid) != null) {
            console.log('id? ' + Users.get(socket.id).username);
            return Users.get(socketid).username;
        } else {
            return null;
        }
    }

    // server generates room name
    function generateRoomName() {
        var animal = Math.floor(Math.random() * (Animals.length));
        var num = Math.floor(Math.random() * 100);
        var roomName = Animals[animal] + num.toString().padStart(2, "0");
        Rooms.push(roomName);
        console.table(Rooms);
        return roomName;
    }

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

    function changeUserPropertyWithID(socketID, property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (Users.has(socketID)) {
            tempObj = Users.get(socketID);
            // console.log('changed current user property: ' + property);
            tempObj[property] = value;
            Users.set(socketID, tempObj);
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
                    id: socket.id,
                    room: "",
                    state: states.LOBBY
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

class Game {

    constructor(userInfo, roomname) {
        // what properties are important?
        // players -> (sockets, users, )
        // alert players game started (debug)

        // round methods:
        // setDeck() <- resets the deck
        // deal cards

        console.log("New Game Instance created.");

        this.Deck;
        this.DiscardPile;
        this.Players = new Map();    // holds: {socketid, username, hand [cards], out(bool), score(int)}
        this.buildPlayerMap(userInfo);
        this.Roomname = roomname;

        this.Round = 3;              // current game round
        this.Turn;
    }

    roundSetUp() {
        this.declareRound();
        this.declareTurn(true);

        this.setDeck();
        this.drawPlayerHands();
        this.addToDiscard(this.drawCard());
    }

    readyCheck(playerID) {
        this.changePlayerPropertyWithID(playerID, 'ready', true);
        console.table(this.Players);

        this.Players.forEach((value, index, array) => {
            if (value.ready == false) return;
        });

        this.roundSetUp();
    }

    buildPlayerMap(userInfo) {
        // userInfo: {[ {username: ____, id: socketid}, ... {}]}
        for (user in userInfo) {
            this.Players.set(
                userInfo[user].id,
                {
                    id: userInfo[user].id,
                    username: userInfo[user].username,
                    hand: [],
                    out: 'false',
                    score: 0,
                    ready: false
                }
            );
        }
        console.log("Player map built.");
        console.table(this.Players);
        console.log(this.Players.values());
    }

    changePlayerPropertyWithID(socketID, property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (this.Players.has(socketID)) {
            tempObj = this.Players.get(socketID);
            // console.log('changed current user property: ' + property);
            tempObj[property] = value;
            this.Players.set(socketID, tempObj);
        }
    }

    setDeck() {
        Deck = ['Joker', 'Joker', 'Joker', 'Joker',
            'HeartAce', 'Heart2', 'Heart3', 'Heart4', 'Heart5', 'Heart6', 'Heart7', 'Heart8', 'Heart9', 'Heart10', 'HeartJack', 'HeartQueen', 'HeartKing',
            'HeartAce', 'Heart2', 'Heart3', 'Heart4', 'Heart5', 'Heart6', 'Heart7', 'Heart8', 'Heart9', 'Heart10', 'HeartJack', 'HeartQueen', 'HeartKing',
            'DiamondAce', 'Diamond2', 'Diamond3', 'Diamond4', 'Diamond5', 'Diamond6', 'Diamond7', 'Diamond8', 'Diamond9', 'Diamond10', 'DiamondJack', 'DiamondQueen', 'DiamondKing',
            'DiamondAce', 'Diamond2', 'Diamond3', 'Diamond4', 'Diamond5', 'Diamond6', 'Diamond7', 'Diamond8', 'Diamond9', 'Diamond10', 'DiamondJack', 'DiamondQueen', 'DiamondKing',
            'SpadeAce', 'Spade2', 'Spade3', 'Spade4', 'Spade5', 'Spade6', 'Spade7', 'Spade8', 'Spade9', 'Spade10', 'SpadeJack', 'SpadeQueen', 'SpadeKing',
            'SpadeAce', 'Spade2', 'Spade3', 'Spade4', 'Spade5', 'Spade6', 'Spade7', 'Spade8', 'Spade9', 'Spade10', 'SpadeJack', 'SpadeQueen', 'SpadeKing',
            'ClubAce', 'Club2', 'Club3', 'Club4', 'Club5', 'Club6', 'Club7', 'Club8', 'Club9', 'Club10', 'ClubJack', 'ClubQueen', 'ClubKing',
            'ClubAce', 'Club2', 'Club3', 'Club4', 'Club5', 'Club6', 'Club7', 'Club8', 'Club9', 'Club10', 'ClubJack', 'ClubQueen', 'ClubKing'
        ];
    
        DiscardPile = [];   // draw one card to discard pile
        //DiscardPile.push(this.drawCard());
    }

    declareRound() {
        // current round
        io.in(this.Roomname).emit('currentRound', { 'round': this.Round });
    }

    declareTurn(firstTurn) {
        var PlayersArray = Array.from(this.Players.values());

        if (firstTurn) {
            this.Turn = (this.Round - 3) % PlayersArray.length;
        }
        else {
            this.Turn++;
            if (this.Turn == PlayersArray.length) this.Turn = 0;
        }

        console.log("Player turn: " + PlayersArray[this.Turn].username);
        io.in(this.Roomname).emit('currentTurn', { 'player': PlayersArray[this.Turn].username });
        io.to(PlayersArray[this.Turn].id).emit('yourTurn');
    }

    drawPlayerHands() {
        this.Players.forEach((value, index, array) => {
            for (var i = 0; i < this.Round; i++)
                value.hand.push(this.drawCard());
            console.log(value.username + "'s hand:");
            console.table(value.hand);
            var hand = { ...value.hand };
            io.to(value.id).emit('playerHand', hand);
        });

        /*for (var player in this.Players) {
            for (var i = 0; i < this.Round; i++)
                player.hand.push(drawCard());
            console.log(player.username + "'s hand:");
            console.table(player.hand);
        }*/
    }

    drawCard() {
        if (Deck.length == 0) {
            this.discardToDeck();
        }
    
        var rand = Math.floor(Math.random() * Deck.length);
    
        var swap = Deck[rand];
        Deck[rand] = Deck[Deck.length - 1];
        Deck[Deck.length - 1] = swap;
    
        var card = Deck.pop();
    
        return card;
    }
    
    discardToDeck() {
        var topOfDiscard = DiscardPile.pop();
        var secondOfDiscard = DiscardPile.pop();
    
        DiscardPile.forEach((value, index, array) => {
            Deck.push(value);
        });
    
        DiscardPile = [secondOfDiscard, topOfDiscard];
    
        console.table(DiscardPile);
        console.table(Deck);
    }

    addToDiscard(card) {
        DiscardPile.push(card);
        io.in(this.Roomname).emit('addToDiscard', { 'card': card });
    }

    drawFromDiscard() {
        if (DiscardPile.length > 0)
            return DiscardPile.pop();
        else return 'none';
    }
}