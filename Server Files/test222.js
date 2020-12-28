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

var newArray = Array.from(Players.keys());

console.table(newArray);

console.log(newArray.indexOf ("123"));
