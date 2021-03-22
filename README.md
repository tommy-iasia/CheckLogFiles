# CheckLogFiles

This project consists of three components
- [Worker](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)
- [Server](#server)
- [Updater](#updater)

# Worker

Worker runs variety of checkings on client machine according to the configure files provided.  
Completing the checking, it submit the result to Server.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)

# Server

Server consists a map of all clients.

When a client submits its checking, server will update the map.

And fire any notification if needed.

# Updater

Updater can receive update information from server.

And update all there components automatically if needed.
