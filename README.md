# CheckLogFiles

Executables can be downloaded in [publish folder](https://github.com/tommy-iasia/CheckLogFiles/tree/master/publish)

![image](https://github.com/tommy-iasia/CheckLogFiles/raw/master/CheckLogServer/previews/21032204-map.png?raw=true)

This project consists of three components
- [Worker](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)
- [Server](#server)
- [Updater](#updater)

# Worker

Worker runs variety of checkings on client machine according to the configure files provided.  
Completing the checking, it submit the result to Server.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)

# Server

Server consists a map of all clients. When a client submits its checking, server will update the map.  
When needed, it fires notifications.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/readme.md)

# Updater

Update workers automatically according to server's instruction.
