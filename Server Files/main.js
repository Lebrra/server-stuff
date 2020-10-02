// --------------------------------------------------------------------------
// This is the Collab Hub server.
// Authors: Nick Hwang, Tony T Marasco, Eric Sheffield
// Contact: nickthwang@gmail.com
// --------------------------------------------------------------------------



const
  http = require("http"),
  express = require("express"),
  socketio = require("socket.io"),
  path = require("path"),
  collabVersion = "0.0.4a",
  _ = require('lodash');

// this kind of works like an enum
const
  types = {
    CONTROL: 'control',
    EVENT: 'event',
    MESSAGE: 'message',
    ROOMS: 'room',
    SELF: 'self',
    CHAT: 'chat',
    LIST: 'list'
  }

// create a new express app
let app = express();
// create http server and wrap the express app
let server = http.createServer(app);
// bind socket.io to that server
let io = socketio(server);
var HUB = {};
var UniqueHeaders = false; // not implemented

const SERVER_PORT = process.env.PORT || 3000;

// default data structures
var namespaces = ['/hub', '/osu', '/default'];
var rooms = ['default', 'room1', 'room2'];
var defaultRoom = ['default'];

var consoleDisplay = 'false';

// an instance of Space is created for each namespace
class Space {
  /**
   * @property {Map} Users - an Map of connected Users // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Map
   * @property {Object[]} Controls - an object array of all available control data
   * @property {Object[]} Events - an object array of all available event data
   * @property {Object[]} Rooms - a string array of available Rooms 
   */
  constructor() {
    this.Users = new Map();
    this.Controls = [];
    this.Events = [];
    this.Rooms = {};
    var defaultRooms = rooms;
  }
}

function onNewConnection(socket) {
  // executed when a new connect is made
  const snsp = socket.nsp.name,
    id = socket.id;
  socket.room = defaultRoom; // not really using this

  socket.emit('connect');
  socket.emit('connected', 'connected');
  socket.emit('serverMessage', 'Collab-Hub Version: ' + collabVersion + ". You're in Room " + socket.room);

  addUser(socket);
  allRooms();
  myRooms();
  listEvents();
  dumpControls();

  //#region socket.on events

  socket.on('clearUsers', () => {
    socket.broadcast.emit('serverMessage', 'clearing user list');
    io.emit('serverMessage', 'clearing user list');
    if (consoleDisplay == 'true') {
      // console.log("Clearing user list...");
    }
    clearUsers();
    listUsers();
  });

  socket.on('clearControls', function () {
    socket.broadcast.emit('serverMessage', 'clearing control list');
    if (consoleDisplay == 'true') {
      // console.log("Clearing control list..");
    }
    clearControls();
    dumpControls();
  });

  socket.on('clearEvents', function () {
    io.of(snsp).emit('serverMessage', 'clearing Events list');
    if (consoleDisplay == 'true') {
      console.log("Clearing Events list..");
    }
    HUB[snsp].Events = [];
    listEvents();
  });

  socket.on('chat', (data) => {
    var tempid = getUsernameFromSocketID();
    console.log('temp id ' + tempid);
    var allSockets = getConnectedSockets();
    chat = {
      id: tempid,
      chat: data
    }
    broadcast(types.CHAT, allSockets, 'chat', chat);
    // var username = getUsernameFromSocketID(socket.id);
    // socket.broadcast.emit('chat', {
    //   id: username,
    //   chat: data
    // });
    if (consoleDisplay == 'true') {
      console.log("chat message received and broadcast: " + tempid);
    }
  })

  // add / change user name to Users
  socket.on('addUsername', function (username) {
    addUsername(username);
  });

  // EVENTS
  socket.on('event', function (header) {
    event(header);
  });

  socket.on('getEvents', function () {
    listEvents();
  });

  // return a list of Users to requestor
  socket.on('getUsers', function () {
    listUsers();
  });

  // clear Users
  socket.on('clearUsers', function () {
    clearUsers();
  });

  // remove user
  socket.on('disconnect', function () {
    removeUser();
    if (consoleDisplay == 'true') {
      console.log('A user disconnected - ' + socket.id);
    }
    listUsers();
    listEvents();
    dumpControls();
    allRooms();
  });

  socket.on('control', function (data) {
    control(data);
  });

  socket.on('observeControl', function (header) {
    observeControl(header);
  });

  socket.on('unobserveControl', function (header) {
    unobserveControl(header);
  });

  socket.on('observeAllControl', function (bool) {
    observeAllControl(bool);
  });

  socket.on('observeEvent', function (header) {
    observeEvent(header);
  });

  socket.on('unobserveEvent', function (header) {
    unobserveEvent(header);
  });

  socket.on('observeAllEvents', function (bool) {
    observeAllEvents(bool);
  });

  socket.on('getControl', function (header) {
    console.log(header);
    if (header == 'dump') {
      dumpControls();
    } else {
      var values = getControlValues(header);
      if (values != null) {
        socket.emit('control', values.header, values.values);
      } else {
        // server will return -1 (maybe a bad idea)
        socket.emit('control', header, null);
      }
    }
  });

  socket.on('setConsoleDisplay', (bool) => {
    setConsoleDisplay(bool);
  });

  socket.on('allRooms', () => {
    allRooms();
    console.log('allRooms received');
  });

  socket.on('joinRoom', (newRoom) => {
    joinRoom(newRoom);
  });

  socket.on('leaveRoom', (room) => {
    leaveRoom(room, myRooms);
  });

  socket.on('myRooms', () => {
    myRooms();
  });

  //#endregion

  //#region functions

  function event(header) {
    if (consoleDisplay == 'true') {
      // console.log("updated Events list: " + header);
    }
    // check if event header already exists, if no, add add header and values
    var _event = HUB[snsp].Events.find(event => event.header == header);

    if (_event == null || _event == undefined) {
      console.log(`adding Event... ${header}`);
      HUB[snsp].Events.push({
        header: header,
        from: socket.id,
        observers: []
      });

      // if new event, send list of all events
      listEvents();
      return;
    }

    // 1. broadcast values to individual observers
    if (_event.observers.length > 0) {
      broadcast(types.EVENT, _event.observers, header, null);
    }

    // 2. get socket ids of all users with observerallcontrol == true
    let allEventObservers = Array.from(HUB[snsp].Users.values()).filter(user => user.observeallevents == 'true').map(user => user.id);

    // 3. filter out individual observers from the observeallcontrol array
    let filteredEventObservers = allEventObservers.filter(observer => !_event.observers.includes(observer));
    console.log('obs ' + filteredEventObservers);

    // 4. broadcast to (remaining) filteredControlObservers
    if (filteredEventObservers.length > 0) {
      broadcast(types.EVENT, filteredEventObservers, header, null);
    }

    if (consoleDisplay == 'true') {
      console.log('received event ' + header);
    }
  }

  function control(data) {
    if (consoleDisplay == 'true') {
      console.log("control: " + data.header + " - " + data.values);
    }

    // check if control header already exists, if not, add add header and values
    var control = HUB[snsp].Controls.find(control => control.header == data.header);

    if (control == null || control == undefined) {
      // console.log(`adding Control... ${data}`);
      HUB[snsp].Controls.push({
        header: data.header,
        values: data.values,
        from: socket.id,
        observers: []
      });

      dumpControls();
      return;
    }

    // update values of header
    control.values = data.values;

    // broadcast values to individual observers
    if (control.observers.length > 0) {
      //
      broadcast(types.CONTROL, control.observers, data.header, data.values);
    }

    let allControlObservers = Array.from(HUB[snsp].Users.values()).filter(user => user.observeallcontrol == 'true').map(user => user.id);

    // filter out individual observers from the observeallcontrol array
    let filteredControlObservers = allControlObservers.filter(observer => !control.observers.includes(observer));

    // broadcast to filteredControlObservers
    if (filteredControlObservers.length > 0) {
      broadcast(types.CONTROL, filteredControlObservers, data.header, data.values);
    }

    if (data.header == null) {
      console.log("Control data without header...");
      return;
    }

    if (consoleDisplay == 'true') {
      console.log('New Control Data: ' + data.header + " - " + data.values);
      // console.log(namespaces[space][room]['Controls']);
    }
  }

  // general broadcast function (one or multiple targets)
  function broadcast(type, targets, header, message) {

    switch (type) {
      case types.CONTROL:
        if (Array.isArray(targets)) {
          console.log(Array.isArray(targets));
          // targets an array of socketids
          for (target of targets) {
            socket.to(target).emit(type, header, message);
            console.log(`Emitting ${type} - ${message} to ${target}`);
          }
        } else {
          socket.to(targets).emit(type, header, message);
        }
        break;
      case types.EVENT:
        // targets an array of socketids
        for (target of targets) {
          socket.to(target).emit(type, header);
          console.log(`emitting ${type} to ${target}`);
        }
        break;
      case types.CHAT:
        // targets an array of socketids
        for (target of targets) {
          socket.to(target).emit(header, message);
          console.log(`emitting ${type} to ${target}`);
        }
        break;
      case types.MESSAGE:
        // check if has many recipients
        if (Array.isArray(targets)) {
          console.log(Array.isArray(targets));
          // targets an array of socketids
          for (target of targets) {
            socket.to(target).emit(type, header, message);
            console.log(`Emitting ${type} - ${message} to ${target}`);
          }
        } else {
          // testing some things here, not real implementation
          for (var room in socket.adapter.rooms) {
            let tempSockets = Object.keys(socket.adapter.rooms[room].sockets);
            socket.to(tempSockets).emit('serverMessage', 'hello friends!');
          }
          console.log(targets);
          // io.of(snsp).emit('serverMessage', 'hello solo friends!');
          console.log(`Emitting ${type} - ${message} to ${targets}`);
        }
        break;
      case types.LIST:
        // LISTS MUST SEND TO EVERYONE
        for (target of targets) {
          socket.to(target).emit(header, message);
          console.log(`emitting ${type} to ${target}`);
        }
        break;
    }
  }

  function getConnectedSockets() {
    return Array.from(HUB[snsp].Users.keys());
  }

  function addUsername(newUsername) {
    // find existing user, add new username
    if (HUB[snsp].Users.has(socket.id)) {
      changeUserProperty('username', newUsername);
    } else {
      addUser(socket);
      addUsername(newUsername);
    }

    // server feedback and upload other users
    if (consoleDisplay == 'true') {
      console.log("create new username and id " + newUsername + " " + socket.id);
    }
    listUsers();
    dumpControls();
    listEvents();
    allRooms();
  }

  function changeUserProperty(property, value) {
    // users properties: id, username, observeallcontrol, observeallevents
    if (HUB[snsp].Users.has(socket.id)) {
      tempObj = HUB[snsp].Users.get(socket.id);
      // console.log('changed current user property: ' + property);
      tempObj[property] = value;
      HUB[snsp].Users.set(socket.id, tempObj);
    }
  }

  // could eliminate this function and have the socket.on event all the changeUserProperty function
  function observeAllControl(bool) {
    changeUserProperty('observeallcontrol', bool);
  }

  // could eliminate this function and have the socket.on event all the changeUserProperty function
  function observeAllEvents(bool) {
    changeUserProperty('observeallevents', bool);
  }

  function observeControl(header) {
    console.log('observing ' + header);

    let foundControl = HUB[snsp].Controls.filter(target => target.header == header);
    // check if control header exists, if no, through warning
    if (foundControl.length > 0) {
      // add socket id to array of control's observers
      if (!foundControl[0].observers.includes(socket.id)) {
        foundControl[0].observers.push(socket.id);
        console.log('added observer');
        // console.table(foundControl);
        dumpControls();
      } else {
        console.log(`Already a subscriber.`);
        broadcast(types.MESSAGE, socket.id, 'serverMessage', `Control header ${header} not found`);
      }
    } else {
      console.log('Control header does not exist');
      broadcast(types.MESSAGE, socket.id, 'serverMessage', `Control header ${header} not found`);
    }
  }

  function unobserveControl(header) {
    // check if control header already exists,
    let found = HUB[snsp].Controls.find(target => target.header == header);
    if (found == null || found == undefined) {
      // err out, control header not found
      // broadcast(type.MESSAGE, socket.id, header, `Control header ${header} not found`);
      console.log('control not found');
    } else {
      if (found.observers.includes(socket.id)) {
        // remove observer
        found.observers.pop(socket.id);
        // recreate array if necessary
        if (found.observers == null || found.observers == undefined) {
          found.observers = [];
        }
        dumpControls();
      } else {
        // observer not found
        console.log('observer not found');
      }
    }
  }

  function observeEvent(header) {
    console.log('observing event ' + header);

    let foundEvent = HUB[snsp].Events.filter(target => target.header == header);
    // check if control header exists, if no, through warning
    if (foundEvent.length > 0) {
      // add socket id to array of control's observers
      if (!foundEvent[0].observers.includes(socket.id)) {
        foundEvent[0].observers.push(socket.id);
        console.log('added event observer');
        // console.table(foundEvent);
        listEvents();
      } else {
        console.log(`Already an observer.`);
      }
    } else {
      console.log('Event header does not exist');
    }
  }

  function unobserveEvent(header) {
    // check if event header already exists,
    let foundEvent = HUB[snsp].Events.filter(target => target.header == header);
    if (foundEvent == null || foundEvent == undefined) {
      // err out, control header not found
      console.log('event not found');
    } else {
      if (foundEvent[0].observers.includes(socket.id)) {
        // remove observer
        foundEvent[0].observers.pop(socket.id);
        console.log('removing observer ' + socket.id + ' from event');
        // recreate array if necessary
        if (foundEvent.observers == null || foundEvent.observers == undefined) {
          foundEvent.observers = [];
        }
        listEvents();
      } else {
        // observer not found
        console.log('event observer not found');
      }
    }
  }

  // ------Server Functions

  function getUsernameFromSocketID(socketid) {
    if (socketid == null) {
      socketid = socket.id;
    }
    if (HUB[snsp].Users.get(socketid) != undefined || HUB[snsp].Users.get(socketid) != null) {
      console.log('id? ' + HUB[snsp].Users.get(socket.id).username);
      return HUB[snsp].Users.get(socketid).username;
    } else {
      return null;
    }
  }

  // Clear users in namespace
  function clearUsers() {
    HUB[snsp].Users = [];
  }

  function clearControls() {
    HUB[snsp].Controls = [];
  }

  function sendRoom() {
    socket.emit('room', socket.room);
  }

  function generateUsername() {
    var value = Math.floor(Math.random() * HUB[snsp].Users.size * 2);
    var tempName = `User` + value.toString().padStart(3, "0");
    return tempName;
  }

  function addUser(socket) {
    // create a new user mapping
    if (!HUB[snsp].Users.has(socket.id)) {
      var tempUsername = generateUsername();
      HUB[snsp].Users.set(
        socket.id,
        {
          username: tempUsername,
          id: socket.id,
          observeallcontrol: 'false',
          observeallevents: 'false'
        });
    }

    console.log(HUB[snsp].Users);
    listUsers();
  }

  function removeUser() {
    // console.log(`removing user ${socket.id}...`);
    HUB[snsp].Users.delete(socket.id);

    // remove controls and events associated with disconnected user
    HUB[snsp].Controls = HUB[snsp].Controls.filter(control => control.from != socket.id);
    HUB[snsp].Events = HUB[snsp].Events.filter(event => event.from != socket.id);

    // broadcast updated user list
    listUsers();
    listEvents();
    dumpControls();
  }

  // broadcast the list of possible events -- showing who is originator of event and who are subscribing
  function listEvents() {
    // create a human readable version of events
    // clone so we are not changing the values
    let events = _.cloneDeep(HUB[snsp].Events);

    for (var event in events) {
      events[event].observers = events[event].observers.map(observer => getUsernameFromSocketID(observer));
      events[event].from = getUsernameFromSocketID(events[event].from);
    }

    // io.of(snsp).emit('events', events);
    broadcast(types.LIST, getConnectedSockets(), 'events', events);
  }

  function listUsers() {
    // console.log('...listing users.....');
    let tempUsers = Array.from(HUB[snsp].Users);
    io.of(snsp).emit('users', tempUsers);
  }

  function setConsoleDisplay(val) {
    consoleDisplay = val;
    console.log(`changed console display to ${consoleDisplay}`);
  }

  function dumpControls() {
    if (consoleDisplay == 'true') {
      console.log("dumping all control data");
    }

    // create a human readable verison of controls -- replace all socket ids with usernames
    let controls = _.cloneDeep(HUB[snsp].Controls);

    for (var control in controls) {
      controls[control].observers = controls[control].observers.map(observer => getUsernameFromSocketID(observer));
      controls[control].from = getUsernameFromSocketID(controls[control].from);
    }

    io.of(snsp).emit('controlDump', controls);
  }

  function getControlValues(header) {
    for (let control of Controls) {
      if (consoleDisplay == 'true') {
        console.log(control.header);
      }
      if (control.header == header) {
        // console.log('match! ' + control.values);
        return control;
      }
    }
    if (consoleDisplay == 'true') {
      console.log("No such control values. " + header);
    }
    return null;
  }

  function joinRoom(newRoom) {
    //check if room is available
    // console.log(HUB[snsp].Rooms.includes('room1'));
    if (!HUB[snsp].Rooms.includes(newRoom.toString())) {
      HUB[snsp].Rooms.push(newRoom.toString());
      console.log(`Created new room: ${newRoom}`);
    }

    // join new room, received as function parameter
    socket.join(newRoom.toString(), () => {
      // console.table(socket.rooms);
      socket.emit('serverMessage', socket.id + ' socket have joined to ' + newRoom);
      // console.table(HUB[snsp].Rooms[HUB[snsp].Rooms.indexOf(newRoom)]);
      // send client info on all rooms they're in
      console.log('test rooms');
      console.table(io.sockets.adapter.rooms);
      myRooms();
      allRooms();
    });
  }

  function leaveRoom(room) {
    //check if room is available
    console.log('leave room', room, snsp);
    if (HUB[snsp].Rooms.includes(room)) {
      socket.leave(room, () => {
        // socket.room = room;
        socket.emit('serverMessage', 'You have left room ' + room);
        // send client info on all rooms they're in
        myRooms();
        allRooms();
      });
    }
  }

  function allRooms() {
    // 1. GET ALL ROOMS IN NAMESPACE
    let socketRooms = Object.keys(socket.adapter.rooms);
    // filtered rooms are the room name, with no details
    filteredRooms = socketRooms.filter(room => !room.includes(snsp));

    // 2. add usernames to each room  --- this a mess -- could probably all done in 3-4 lines 
    var roomDetails = {};
    for (var room in socket.adapter.rooms) {
      // filter out self-created rooms 
      if (room.toString().includes(snsp)) {
        continue;
      }

      // connected sockets
      console.log('room: ' + room);
      console.log(socket.adapter.rooms[room]);
      let _sockets = Object.keys(socket.adapter.rooms[room].sockets);
      console.log('sockets: ' + _sockets);
      // console.log(`ts: ${_sockets} in ${room}`);
      let tempUsers = [];
      for (_socket in _sockets) {
        console.log('checking ' + _sockets[_socket] + " against " + getUsernameFromSocketID(_sockets[_socket]));
        tempUsers.push(getUsernameFromSocketID(_sockets[_socket]));
      }

      roomDetails[room] = {
        ...tempUsers
      }
    }
    io.of(snsp).emit('allRooms', filteredRooms);
    io.of(snsp).emit('allRoomDetails', roomDetails);
  }

  function myRooms() {
    // GET ROOMS SOCKET BELONGS TO
    let tempRooms = [];
    tempRooms = Object.values(socket.rooms);

    tempRooms = tempRooms.filter(room => room != socket.id);

    socket.emit('myRooms', tempRooms);
  }

  // functions lists room name and the clients within rooms
  function updateRooms() {
    console.log(`updating rooms`);
    allRooms();
    myRooms();
  }
}

function startServer() {
  console.clear();
  // create a new express app
  app = express();
  // create http server and wrap the express app
  server = http.createServer(app);
  // bind socket.io to that server
  io = socketio(server);

  app.use(express.static("public"));

  // The structure container
  HUB = {};

  // create socket namespaces
  for (space of namespaces) {
    // io instance using the namespace name (name is a property of socket namespace)
    let nsp = io.of(space);
    nsp.on('connection', onNewConnection);
    console.log(`created namespace ` + nsp.name);

    HUB[nsp.name] = new Space();
    HUB[nsp.name].Rooms = rooms; // not really using separate structure for 
  }

  // important! must listen from `server`, not `app`, otherwise socket.io won't function correctly
  server.listen(SERVER_PORT, () => console.info(`Listening on port ${SERVER_PORT}.`));

  // will send one message per second to all its clients
  let secondsSinceServerStarted = 0;
  setInterval(() => {
    // console.log('starting interval transmissions');
    // secondsSinceServerStarted++;
    // io.emit("seconds", secondsSinceServerStarted);
    io.emit('ping');
  }, 1000);
}

startServer();
