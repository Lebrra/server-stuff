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

    socket.on('setReady', () => {
        console.log('checking for all ready...');
        Games[Users.get(socket.id)['room']].readyCheck(socket.id);
    });

    socket.on('drawCard', (fromDeck) => {
        var newCard;
        if (fromDeck === true) {
            newCard = Games[Users.get(socket.id)['room']].drawCard();
            io.in(Users.get(socket.id)['room']).emit('drewFromDeck', { player: Users.get(socket.id).username });
        }
        else {
            newCard = Games[Users.get(socket.id)['room']].drawFromDiscard();
            io.in(Users.get(socket.id)['room']).emit('drewFromDiscard', { player: Users.get(socket.id).username });
        }
        console.log(newCard);
        socket.emit('newCard', {card: newCard});
    })

    socket.on('discardCard', (discardInfo) => {
        console.log(getUsernameFromSocketID(socket.id) + " discarded " + discardInfo);
        Games[Users.get(socket.id)['room']].addToDiscard(discardInfo);
        Games[Users.get(socket.id)['room']].declareTurn(false);
    });


    socket.on("firstOut", () => {
        io.in(Users.get(socket.id)['room']).emit('firstOutPlayer', { player: Users.get(socket.id).username });
        Games[Users.get(socket.id)['room']].OutPlayer = Games[Users.get(socket.id)['room']].Turn;
    });

    socket.on("updateOutDeck", (outDeck) => {
        console.table(outDeck);
        io.in(Users.get(socket.id)['room']).emit('updateOutDeck', outDeck);
    });

    socket.on("receiveScore", (score) => {
        Games[Users.get(socket.id)['room']].updatePlayerScore(socket.id, score);
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
                    username: "New Player",
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

setInterval(function () {
  io.emit('ping');
}, 1000);

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
        this.OutPlayer = -1;
        this.roundOver = false;

        this.ScoreCard = new Array();
    }

    roundSetUp() {
        this.declareRound();

        this.ScoreCard[this.Round - 3] = new Array(Array.from(this.Players.values()).length);
        this.ScoreCard[this.Round - 3].forEach((value, index, array) => {
            value = -1;
        });

        this.declareTurn(true);

        this.setDeck();
        this.drawPlayerHands();
        this.addToDiscard(this.drawCard());
    }

    readyCheck(playerID) {
        this.changePlayerPropertyWithID(playerID, 'ready', true);
        console.table(Array.from(this.Players.values()));

        var allReady = true;
        this.Players.forEach((value, index, array) => {
            if (value.ready == false) allReady = false;
        });

        if (allReady) this.roundSetUp();
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
        Deck = ['Joker_0', 'Joker_0', 'Joker_0', 'Joker_0',
            'Heart_1', 'Heart_2', 'Heart_3', 'Heart_4', 'Heart_5', 'Heart_6', 'Heart_7', 'Heart_8', 'Heart_9', 'Heart_10', 'Heart_11', 'Heart_12', 'Heart_13',
            'Heart_1', 'Heart_2', 'Heart_3', 'Heart_4', 'Heart_5', 'Heart_6', 'Heart_7', 'Heart_8', 'Heart_9', 'Heart_10', 'Heart_11', 'Heart_12', 'Heart_13',
            'Diamond_1', 'Diamond_2', 'Diamond_3', 'Diamond_4', 'Diamond_5', 'Diamond_6', 'Diamond_7', 'Diamond_8', 'Diamond_9', 'Diamond_10', 'Diamond_11', 'Diamond_12', 'Diamond_13',
            'Diamond_1', 'Diamond_2', 'Diamond_3', 'Diamond_4', 'Diamond_5', 'Diamond_6', 'Diamond_7', 'Diamond_8', 'Diamond_9', 'Diamond_10', 'Diamond_11', 'Diamond_12', 'Diamond_13',
            'Spade_1', 'Spade_2', 'Spade_3', 'Spade_4', 'Spade_5', 'Spade_6', 'Spade_7', 'Spade_8', 'Spade_9', 'Spade_10', 'Spade_11', 'Spade_12', 'Spade_13',
            'Spade_1', 'Spade_2', 'Spade_3', 'Spade_4', 'Spade_5', 'Spade_6', 'Spade_7', 'Spade_8', 'Spade_9', 'Spade_10', 'Spade_11', 'Spade_12', 'Spade_13',
            'Club_1', 'Club_2', 'Club_3', 'Club_4', 'Club_5', 'Club_6', 'Club_7', 'Club_8', 'Club_9', 'Club_10', 'Club_11', 'Club_12', 'Club_13',
            'Club_1', 'Club_2', 'Club_3', 'Club_4', 'Club_5', 'Club_6', 'Club_7', 'Club_8', 'Club_9', 'Club_10', 'Club_11', 'Club_12', 'Club_13'
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

        if (this.Turn == this.OutPlayer) {
            //round has ended!
            console.log("~~ the round has ended! ~~");
            this.roundOver = true;
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

    updatePlayerScore(playerID, score) {
        var playerArray = Array.from(this.Players.keys());
        var playerIndex = playerArray.indexOf(playerID);
        console.log("player index: " + playerIndex);

        this.ScoreCard[this.Round - 3][playerIndex] = score;
        console.table(this.ScoreCard);
        //update client scorecards
        //io.in(this.Roomname).emit('sendNewScore', {'score': score});
        if(this.roundOver)
            this.updateScoreCard();
    }

    updateScoreCard(){
        // get only the playernames
        let playerNames = Array.from(this.Players.values()).map(player => player['username']);
        // send player names to all clients
        io.in(this.Roomname).emit('playernamesInRoom', {'players': playerNames});
        console.log("Sending Player Names: ");
        console.table(playerNames);

        // send score card to all room clients
        io.in(this.Roomname).emit('updateScoreCard', {'scorecard': this.ScoreCard});
        console.log("Sending Score Card: ");
        console.table(this.ScoreCard);
    }
}
