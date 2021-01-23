# TankWars
A client-server game in C# using multi-threading following the MVC practice
which allows the server to accept multiple players(clients) simultaneously
in the network based on the TCP/IP protocol, and prevents any race conditions.
This project uses socket to make the connection and JSON to serialize
and deserialize the info between client and server.
It also saves game records to database, and public data to a webserver.
