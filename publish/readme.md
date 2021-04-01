# CheckLogFiles

You may download the client.7z and server.7z

## Configuration

You are recommanded to replace the following paths in files

| Path | Description
|-|-|
| http://localhost:44369 | server address |
| C:\Users\TommyLee\Downloads\Client | client installation path |
| iAsia.Pikachu | identifier for submitting logs |

## Server

Just run the *Start.cmd*.

## Client

1. Copy configurations from *[Templates](https://github.com/tommy-iasia/CheckLogFiles/tree/master/CheckLogWorker#runners)* folder
2. Replace the above mentioned values
3. Setup *[Schedule.json](https://github.com/tommy-iasia/CheckLogFiles/tree/master/CheckLogWorker#scheduling)*
4. Run *CheckLogWorker.cmd* to comfirm configurations
5. Add task into *[Windows TaskScheduler](https://github.com/tommy-iasia/CheckLogFiles/tree/master/CheckLogWorker#windows-taskscheduler)*
