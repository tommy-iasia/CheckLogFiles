# Check Log Server

![map](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/previews/21032204-map.png?raw=true)

When worker completes its runner, the result will be sent to server.  
If warning or error happens, server will notify users through Signal-R and Telegram.

# Nodes

Nodes are names with identifiers. You are suggested to name them with dot seperators to indicate the path.

For example, in the above map, there are
- iAsia.Example.Tommy
- iAsia.Example.Daisy
- iAsia.Example.A
- iAsia.Example2.Gandalf

They are shown in tree format. Do wisely balance the nodes making the map easy to read.

# Notification

When a warning or an error is received, the node is marked red. Also the whole path is marked red.  
Also, this is updated through signalR - no refresh of page is needed.

Whenever you see a marked node, click the node and the error log is shown.

![error](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/previews/21032205-nodeError.png?raw=true)

> Remember to press the **Resolve** button after resolving the case.

# Logs

All logs can be viewed in *Logs* page, no matter containing errors or not.  
Row record is saved in *Database.db*'s *LogRows* table. While, content is saved in file in *Data\\Log* folder.

Typically a log file is of size less than 5KB.  
A busy runner, running every 5 minutes, requires around 0.5MB daily.  
Therefore, 1GB can support around 60 busy runners for one month.

> Space Required per Month = 2KB × 60 ÷ Interval (min) × 24 × 30

| Busyness | Interval | Estimated Daily | Estimated Monthly |
|-|-|-|-|
| Daily | 24 hr | 2 KB | 60 KB |
| Hourly | 1 hr | 48 KB | 1.5 MB |
| Normal | 15 min | 192 KB | 5.6 MB |
| Busy | 5 min | 0.5 MB | 17 MB |

You are advised to do the math and prepare enough harddisk space for your workers.  
Also, you may reduce the frequency of calling runners outside working hour.

Most importantly, clear logs and table monthly.  
And if possible, deploy one set of runners in server machine monitoring the space and growth.

# Admin Control

Currently, there is role assignment in accounts. Accounts are in *Accounts* table with plain text password.
You are suggested to use the default account **Pikachu** before role assignment is employed in future.

However, no users can create, update or delete records or logs.
Only admin who can access to file system and **Database.db** holds the right of modifying data.

You may use any [SQLite editor](https://sqlitebrowser.org/) to modify data in *Database.db* if needed.
