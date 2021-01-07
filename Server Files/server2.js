
// JavaScript source code
const app = require('express')();
const server = require('http').createServer(app);
const io = require('socket.io')(server);

const PORT = process.env.PORT || 3000;

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/index.html');
});

let Users = new Map();  // declaring Users structure
let Rooms = [];
let Games = [];

let Animals = ['Possum', 'Frog', 'Zebra', 'Lizard', 'Beaver', 'Panda', 'Giraffe', 'Toucan', 'Pelican', 'Sloth', 'Alligator', 'Scorpion', 'Viper', 'Armadillo'];
let Deck;
let DiscardPile;

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
        let newRoomName = generateRoomName();
        socket.join(newRoomName);
        changeUserProperty("room", newRoomName);
        changeUserProperty("state", states.ROOM);
        socket.emit('createdRoom', { name: newRoomName });
        //socket.emit('roomCount', { roomCount: 1});
        //send room name to unity of the creator only
        listRoomUsers();
    });

    socket.on('joinRoom', (roomName) => {
        console.log('Room name given: ' + roomName);

        if (Rooms.includes(roomName)) {
            //check for roomstate
            if (typeof Games[roomName] === 'undefined') {
                //game has not started, players may join

                var playerCount = Object.keys(socket.adapter.rooms[roomName].sockets).length;
                console.log("player count: " + playerCount);

                if (playerCount >= 6) {
                    // max player count met; cannot join
                    console.log("Cannot join; room is full.");
                    socket.emit('roomError', { message: 'Room is full, cannot join' });
                }
                else {
                    socket.join(roomName);
                    changeUserProperty("room", roomName);
                    changeUserProperty("state", states.ROOM);
                    console.log(socket.adapter.rooms[Users.get(socket.id).room].sockets);
                    //console.table(Users.get(socket.id).room);
                    socket.emit('createdRoom', { name: roomName });
                    //socket.to(roomName).emit('roomCount', { roomCount: playerCount });
                    console.log(Users.get(socket.id).username + ' joined room ' + roomName);
                    listRoomUsers();
                }
            }
            else {
                //game has started, players CANNOT join
                console.log("Cannot join; game has already started.");
                socket.emit('roomError', { message: 'Game has started, cannot join' });
            }
        }
        else {
            // room does not exist
            console.log("Cannot join; room does not exist.");
            socket.emit('roomError', { message: 'Room not found, try again' });
        }
    });

    socket.on('leaveRoom', () => {
        console.log(Users.get(socket.id).username + ' left their room');

        let formerRoom = Users.get(socket.id).room;

        socket.leave(Users.get(socket.id).room);
        changeUserProperty("room", "");
        changeUserProperty("state", states.LOBBY);
        //console.log(socket.adapter.rooms[Users.get(socket.id).room].sockets);

        console.log("room:");
        console.log(Users.get(socket.id).room);

        updateFormerRoomList(formerRoom);
    });

    socket.on('startGame', () => {
        //check player count one more time:
        var playerCount = Object.keys(socket.adapter.rooms[Users.get(socket.id).room].sockets).length;
        if (playerCount < 2 || playerCount > 6) {
            console.log("invaild player count, attempting to update play button...");
            socket.emit('roomCount', { roomCount: 1 });
        }
        else {
            console.log(Users.get(socket.id)['username'] + ' has started the game!');
            let roomStarted = Users.get(socket.id)['room'];

            for (user in socket.adapter.rooms[Users.get(socket.id).room].sockets) {
                changeUserPropertyWithID(user.id, 'state', states.GAME);
            }
            console.table(Users);

            io.in(roomStarted).emit('loadGame');

            // make new game instance here
            // var game = new Game(listRoomUsers());
            Games[roomStarted] = new Game(listRoomUsers(), roomStarted); // assigned new game to Games array based on roomname
        }
    });

    socket.on('setReady', () => {
        console.log('checking for all ready...');
        Games[Users.get(socket.id)['room']].readyCheck(socket.id);
    });

    socket.on('drawCard', (fromDeck) => {
        let newCard;
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
        io.in(Users.get(socket.id)['room']).emit('firstOutPlayer', { player: Users.get(socket.id).username, playerIndex: Games[Users.get(socket.id)['room']].Turn });
        Games[Users.get(socket.id)['room']].OutPlayer = Games[Users.get(socket.id)['room']].Turn;

        //send firstout index to all players:
        //io.in(Users.get(socket.id)['room']).emit('firstOutPlayerIndex', { playerIndex: Games[Users.get(socket.id)['room']].OutPlayer });
    });

    socket.on("updateOutDeck", (outDeck) => {
        console.table(outDeck);
        io.in(Users.get(socket.id)['room']).emit('updateOutDeck', outDeck);
    });

    socket.on("receiveScore", (score) => {
        Games[Users.get(socket.id)['room']].updatePlayerScore(socket.id, score);
    });

    socket.on("deleteGame", () => {

        socket.leave(Users.get(socket.id).room);
        changeUserProperty("room", "");
        changeUserProperty("state", states.LOBBY);

        if (Games.includes(Users.get(socket.id)['room'])) {
            console.log(`destroying game in room ${Users.get(socket.id)['room']}`);

            let gameIndex = Games.indexOf(Users.get(socket.id)['room']);
            Games.splice(gameIndex, 1);
        }
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

        let roomDetails = {
            ...tempUsers
        }

        console.log('--- Room ' + socket.adapter.rooms[Users.get(socket.id).room] + ' Details ---');
        console.table(roomDetails);
        io.in(Users.get(socket.id).room).emit('roomUsers', roomDetails);        // sending to all users within a room
        return tempUsers;
    }

    // update users in previous rooms
    function updateFormerRoomList(formerRoom) {
        if (socket.adapter.rooms[formerRoom] != null || socket.adapter.rooms[formerRoom] !== undefined) {
            console.table(socket.adapter.rooms[formerRoom].sockets);
            let _sockets = socket.adapter.rooms[formerRoom].sockets;
            let tempUsers = [];
            for (let _socket in _sockets) {
                if (Users[_socket].state === states.ROOM) {
                    tempUsers.push(
                        {
                            username: getUsernameFromSocketID(_socket),
                            id: _socket
                        });
                }
            }

            let roomDetails = {
                ...tempUsers
            }

            console.log('--- Room ' + formerRoom + ' Details ---');
            //console.table(roomDetails);
            socket.to(formerRoom).emit('roomUsers', roomDetails);
            //socket.to(formerRoom).emit('roomCount', { roomCount: tempUsers.length });
            //io.emit('roomUsers', roomDetails);
        }
        else {
            console.log("room is empty, removing it");
            let roomIndex = Rooms.indexOf(formerRoom);
            Rooms.splice(roomIndex, 1);
        }
    }

    function getUsernameFromSocketID(socketid) {
        if (socketid == null) {
            socketid = socket.id;
        }
        if (Users.get(socketid) !== undefined || Users.get(socketid) != null) {
            console.log('id? ' + Users.get(socket.id).username);
            return Users.get(socketid).username;
        } else {
            return null;
        }
    }

    // server generates room name
    function generateRoomName() {
        let animal = Math.floor(Math.random() * (Animals.length));
        let num = Math.floor(Math.random() * 100);
        let roomName = Animals[animal] + num.toString().padStart(2, "0");

        while (Rooms.includes(roomName)) {
            console.log("changing room name, chosen was taken");
            num = Math.floor(Math.random() * 100);
            roomName = Animals[animal] + num.toString().padStart(2, "0");
        }

        Rooms.push(roomName);
        console.table(Rooms);
        return roomName;
    }

    function changeUserProperty(property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (Users.has(socket.id)) {
            let tempObj = Users.get(socket.id);
            // console.log('changed current user property: ' + property);
            tempObj[property] = value;
            Users.set(socket.id, tempObj);
        }
        if (property === 'username') checkUsers();
    }

    function changeUserPropertyWithID(socketID, property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (Users.has(socketID)) {
            let tempObj = Users.get(socketID);
            // console.log('changed current user property: ' + property);
            tempObj[property] = value;
            Users.set(socketID, tempObj);
        }
        if (property === 'username') checkUsers();
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
    }

    function removeUser(socket) {
        if (Users.has(socket.id)) {
            Users.delete(socket.id);
            checkUsers();
        }
    }

    function checkUsers() {
        console.table(Users);
        listUsers();
    }

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
        let usernameObject = {};
        for (let i = 0; i < tempUsers.length; i++) {
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

        this.ScoreCard = [];
    }

    roundSetUp() {
        this.declareRound();

        this.ScoreCard[this.Round - 3] = new Array(Array.from(this.Players.values()).length);
        this.ScoreCard[this.Round - 3].forEach((value) => {
            this.ScoreCard[this.Round - 3][value]= -1;
        });

        this.declareTurn(true);

        this.setDeck();
        this.drawPlayerHands();
        this.addToDiscard(this.drawCard());
    }

    readyCheck(playerID) {
        this.changePlayerPropertyWithID(playerID, 'ready', true);
        console.table(Array.from(this.Players.values()));

        let allReady = true;
        this.Players.forEach((value) => {
            if (value.ready == false) allReady = false;
        });

        if (allReady) this.roundSetUp();
    }

    buildPlayerMap(userInfo) {
        // userInfo: {[ {username: ____, id: socketid}, ... {}]}
        userInfo.forEach(user => {
            this.Players.set(user.id,
                {
                    id: user.id,
                    username: user.username,
                    hand: [],
                    out: 'false',
                    score: 0,
                    ready: false
                }
            );
        });


        /*        foreach (user in userInfo) {
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
                };*/
        console.log("Player map built.");
        console.table(this.Players);
        console.log(this.Players.values());
    }

    changePlayerPropertyWithID(socketID, property, value) {
        // users properties: id, username, observeallcontrol, observeallevents
        if (this.Players.has(socketID)) {
            let tempObj = this.Players.get(socketID);
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
        console.log(`Starting round ${this.Round}`);
        // current round
        io.in(this.Roomname).emit('currentRound', { 'round': this.Round });
    }

    declareTurn(firstTurn) {
        let PlayersArray = Array.from(this.Players.values());

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
        this.Players.forEach((value) => {
            for (let i = 0; i < this.Round; i++)
                value.hand.push(this.drawCard());
            console.log(value.username + "'s hand:");
            console.table(value.hand);
            let hand = { ...value.hand };
            io.to(value.id).emit('playerHand', hand);
        });

        /*for (var player in this.Players) {
            for (var i = 0; i < this.Round; i++)
                player.hand.push(drawCard());
            console.log(player.username + "'s hand:");
            console.table(player.hand);
        }*/
    }

    clearPlayerHands(){
        console.log(`Clear each player's hand...`);
        this.Players.forEach(player => {
            player.hand = [];
            console.table(`${player.hand}`);
        });
    }

    drawCard() {
        if (Deck.length == 0) {
            this.discardToDeck();
        }

        let rand = Math.floor(Math.random() * Deck.length);

        let swap = Deck[rand];
        Deck[rand] = Deck[Deck.length - 1];
        Deck[Deck.length - 1] = swap;

        return Deck.pop();
    }

    discardToDeck() {
        let topOfDiscard = DiscardPile.pop();
        let secondOfDiscard = DiscardPile.pop();

        DiscardPile.forEach((value) => {
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
        let playerArray = Array.from(this.Players.keys());
        let playerIndex = playerArray.indexOf(playerID);
        console.log("player index: " + playerIndex);

        //
        if (this.Round == 12){
            console.log("doubling score");
            score *= 2;
        } else if(this.Round == 12){
            console.log("tripling score");
            score *= 3;
        }

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

        //reset round method
        if (this.Round < 13) {
            this.resetRound();
        } else {
            console.log("GAME OVER, just finished round 13.");
            io.in(this.Roomname).emit('gameover');
        }
    }

    resetRound() {
        console.log(`resetting round... Next round will be ${this.Round+1}`);
        // increment round
        this.Round++;
        // set all ready to false
        // this.Players.foreach(player => this.changePlayerPropertyWithID(player, 'ready', false));
        this.Players.forEach(player => player.ready = false);
        // console.log("are all players 'ready' = false?");
        // console.table(Array.from(this.Players.values()));

        // clear players hands
        this.clearPlayerHands();

        // set outplayer to -1
        this.OutPlayer = -1;
        // set roundOver to false
        this.roundOver = false;
    }
}

