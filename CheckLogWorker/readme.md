# Check Log Worker

When run, you are expected to provide a configure file which contains the configuration in JSON format

    CheckLogWorker.exe Configure.json

![start](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/previews/21032202-start.png?raw=true)

Configure.json

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Pikachu",
  "Runner": "HarddiskRemainLow",
  "Drive": "C:\\",
  "WarnSize": "10GB",
  "ErrorSize": "500MB"
}
````

## Configure

Commonly, configuration fields are in key-value pair. And fields fall into two categories

- Program related fields
- Runner related fields

**Program related** fields are those that must be provided whenever program is invoked.  
They define the basic of program flow

| Field | Format | Example | Description |
|-|-|-|-|
| Server | url | "http://check-log-server.iasia.com:44369" | the address of server for submitting result |
| Identifier | text | "iAsia.Example.A" | identifier in server identifying this machine uniquely |
| Runner | text | "HarddiskRemainLow" | the name of runner to be run in this invocation |

### Splitting Configuration

Configuration can be splitted into multiple JSON files. Just add those files as parameter when invoking the program.

In order to avoid duplication, you are adviced to split configuration into different files in a structured manner.

    CheckLogWorker.exe Program.json HarddiskRemainLow.json

# Runners

There are many runners for different purposes. More are to be added.  
Each of them requires different configuration fields.

- [Harddisk Remain Low](#harddisk-remain-low-runner)
- [Harddisk Over-Growth](#harddisk-over-growth)
- [Oversize Daily Directory Runner](#oversize-daily-directory-runner)
- [KPI Queue Runner](#kpi-queue-runner)
- [NetWarn Overflow Runner](#netwarn-overflow-runner)
- [NetError Overflow Runner](#neterror-overflow-runner)
- [Large Retransmission Request Runner](#large-retransmission-request-runner)
- [Retransmission Rejected Runner](#retransmission-rejected-runner)

## Harddisk Remain Low Runner

Check the harddisk space and fire warning when space is running low.

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "HarddiskRemainLowRunner",
  "Drive": "C:\\",
  "WarnSize": "20GB",
  "ErrorSize": "5GB"
}
````

| Field | Format | Unit | Example | Description |
|-|-|-|-|-|
| Runner | text | | "HarddiskRemainLowRunner" | |
| Drive | text | | "C:\\" | the drive to be chcked |
| WarnSize | size | B | "10GB" | fire warning when space is lower than the given value |
| ErrorSize | size | B | "500MB" | fire error when space is lower than the given value |

> Suggested to be called *hourly*

## Harddisk Over-Growth

Check the harddisk growth speed and fire warning when space is used fast

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "HarddiskOverGrowthRunner",
  "Drive": "C:\\",
  "WarnRate": "10GB/hr",
  "ErrorRate": "1GB/30min"
}
````

| Field | Format | Unit | Example | Description |
|-|-|-|-|-|
| Runner | text | | "HarddiskOverGrowthRunner" | |
| Drive | text | | "C:\\" | the drive to be chcked |
| WarnRate | size rate | B/s | "10GB/hr" | fire warning when usage is faster than the given value |
| ErrorSize | size rate | B/s | "1GB/30min" | fire error when usage is faster than the given value |

> Suggested to be called *every 15 minutes*

## Oversize Daily Directory Runner

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "OversizeDailyDirectoryRunner",
  "PathPattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>",
  "WarnSize": "1GB",
  "ErrorSize": "10GB"
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "OversizeDailyDirectoryRunner" | |
| PathPattern | tag path | "C:\\iAsiaLogs\\*\<yyyyMMdd\>*" | folder path with date format tags |
| WarnSize | size | "1GB" | size of folder which triggers a warning |
| ErrorSize | size | "10GB" | size of folder which triggers an error |

> Suggested to be called *hourly*

## KPI Queue Runner

Check our *kpi.txt* and raise error when queue is not processing fast enough

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "KpiQueueRunner",
  "FilePattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>\\kpi.txt",
  "IgnoreQueuePatterns": [
    "Q-CMDFDBSAVING-C",
    "Q-ITDBW\\."
  ],
  "WarnProportion": 0.01,
  "WarnCount": 100,
  "ErrorProportion": 0.05,
  "ErrorCount": 500
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "KpiQueueRunner" | |
| FilePattern | tag path | "C:\\iAsiaLogs\\*\<yyyyMMdd\>*\\kpi.txt" | the *kpi.txt* path with date format tags |
| IgnoreQueuePatterns | regex array | ["Q-CMDFDBSAVING-C", "Q-ITDBW\\."] | regex patterns for ignoring queue names |
| WarnProportion | decimal | 0.01 | unprocessed proportion in queue triggering a warning |
| WarnCount | int | 100 | unprocessed amount in queue which calming a warning |
| ErrorProportion | decimal | 0.05 | unprocessed proportion in queue triggering an error |
| ErrorCount | int | 500 | unprocessed amount in queue which calming an error |

> Suggested to be called *every 15 minutes*

This is extremely useful for MDF and OMDI.  
Just remember to add DB queue into *IgnoreQueuePatterns* as DB items are meant to process slowly.

## NetWarn Overflow Runner

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "NetWarnOverflowRunner",
  "FilePattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>\\NetWarnLog.txt",
  "IgnoreCount": 50,
  "IgnoreSpan": "5min",
  "ErrorPorts": [ 24000 ]
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "NetWarnOverflowRunner" | |
| FilePattern | tag path | "C:\\*\<yyyyMMdd\>*\\NetWarnLog.txt" | *NetWarnLog.txt* path with date format tags |
| IgnoreCount | int | 100 | amount of log line being ignored before firing |
| IgnoreSpan | time span | "5min" | length of time being ignored before firing |
| ErrorPorts | int array | [ 24000 ] | ports that raise error instead of warning |

> Suggested to be called *every 10 minutes*

- If applying on MDF, add iTrade port to *ErrorPorts* as iTrade can upward blocks MDF
- If applying on iTrade, add Cascade port to *ErrorPorts*
- Beware that *IgnoreSpan* must be smaller than the calling interval; otherwise the checking is always false
- Checking frequently is necessary as *NetWarnLog.txt* can grow rapidly and may become too large to read

## NetError Overflow Runner

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "NetErrorOverflowRunner",
  "FilePattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>\\NetErrorLog.txt",
  "IgnoreCount": 100,
  "IgnoreSpan": "30s"
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "NetWarnOverflowRunner" | |
| FilePattern | tag path | "C:\\*\<yyyyMMdd\>*\\NetErrorLog.txt" | *NetErrorLog.txt* path with date format tags |
| IgnoreCount | int | 20 | amount of log line being ignored before firing |
| IgnoreSpan | time span | "30s" | length of time being ignored before firing |

> Suggested to be called *every 5 minutes*

- Beware that *IgnoreSpan* must be smaller than the calling interval; otherwise the checking is always false
- Checking frequently is necessary as *NetErrorLog.txt* can grow extremely fast

## Large Retransmission Request Runner

Check our *Retransmission.txt* and raise error when large re-transmission is triggered, indicating potential network failure

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "LargeRetransmissionRequestRunner",
  "FilePattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>\\Retransmission.txt",
  "WarnCount": 100,
  "ErrorCount": 1000
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "LargeRetransmissionRequestRunner" | |
| FilePattern | tag path | "C:\\*\<yyyyMMdd\>*\\Retransmission.txt" | *Retransmission.txt* path with date format tags |
| WarnCount | int | 100 | re-transmission length which triggers a warning |
| ErrorCount | int | 500 | re-transmission length which triggers an error |

> Suggested to be called at least *every 5 minutes*

## Retransmission Rejected Runner

Check our *Retransmission.txt* and raise error when re-transmission is rejected by HKEX, indicating a refresh

````
{
  "Server": "http://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "RetransmissionRejectedRunner",
  "FilePattern": "C:\\Tommy\\tasks\\210316 CheckLogFiles\\iAsiaLogs\\<yyyyMMdd>\\Retransmission.txt"
}
````

| Field | Format | Example | Description |
|-|-|-|-|
| Runner | text | "RetransmissionRejectedRunner" | |
| FilePattern | tag path | "C:\\*\<yyyyMMdd\>*\\Retransmission.txt" | *Retransmission.txt* path with date format tags |

> Suggested to be called at least *every 5 minutes*

# Logging

Logs are generated whenever the worker program is called.

They are logged in *.log* files. You are suggested to clear them from time to time.

# Submitting to Server

Every logs are submitted to except that *SkipSendInfo* is set true in configure and no warning or error is generated.

![server](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/previews/21032203-logs.png?raw=true)

Continue with the [server setting](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogServer/readme.md).

# Scheduling

Calling with first parameter being *Schedule*, the program enters schedule mode.

    CheckLogWorker.exe Schedule Schedule.json

````
[
  {
    "Time": "**:%1:00",
	"Arguments": ["HarddiskRemainLowRunner.json"]
  }
]
````

Schedule mode will run different runners at given time pattern using normal file arguments.

## Time Pattern

Time pattern is used to calculate the trigger time daily. It supports both range and interval.

| Example | Pattern | Description |
|-|-|-|
| **\*\***:00:00 | Wildcard | hourly |
| **9-16**:00:00 | Range | from 9am to 4pm |
| \*\*:**%15**:00 | Interval | every 15 minutes |
| 9-16:%15:00 | Combine | every 15 minutes from 9am to 4pm |

You are adviced to use Window's Task Scheduler to call the CheckLogWorker.exe in your favor.

## Windows TaskScheduler

You are advise to set a daily task in TaskScheduler daily calling the worker in schedule mode.

Be noted that new schedule mode worker will replace the old one automatically.  
There is no need to worry about duplication.
