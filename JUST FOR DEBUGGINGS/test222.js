Players = new Map();
Players.set(
    "123",
    {
       id: "something else"
    }
);

Players.set(
    "456",
    {
       id: "something new"
    }
);

// var newArray = Array.from(Players.keys());
var newArray = Array.from(Players.values()).map(p => p['id']);
// newArray = newArray.filter(player => player.id);
var newArray2 = newArray.map(p => p['id']);
console.table(newArray);

// console.log(newArray.indexOf ("123"));
