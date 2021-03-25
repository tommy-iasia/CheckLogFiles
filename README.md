# CheckLogFiles

Executables can be downloaded in [publish folder](https://github.com/tommy-iasia/CheckLogFiles/tree/master/publish)

![image](https://github.com/tommy-iasia/CheckLogFiles/raw/master/CheckLogServer/previews/21032204-map.png?raw=true)

This project consists of four components
- [Worker](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)
- [Server](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/readme.md)
- [Scheduler](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogScheduler/readme.md)
- [Updater](#updater)

# Worker

Worker runs variety of checkings on client machine according to the configure files provided.  
Completing the checking, it submit the result to Server.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/readme.md)

# Server

Server consists a map of all clients. When a client submits its checking, server will update the map.  
When needed, it fires notifications.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/readme.md)

# Scheduler

Run different workers according to time patterns provided.

[continue reading](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogScheduler/readme.md)

# Updater

Update workers automatically according to server's instruction.
