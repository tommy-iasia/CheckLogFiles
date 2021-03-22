# Check Log Worker

When run, you are expected to provide a configure file which contains the configuration in JSON format

    CheckLogWorker.exe Configure.json

![start](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogWorker/previews/21032202-start.png?raw=true)

Configure.json

````
{
  "Server": "https://localhost:44369",
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

Program related fields are those that must be provided whenever program is invoked.  
They define the basic of program flow

| Field | Format | Example | Description |
|-|-|-|-|
| Server | url | "https://check-log-server.iasia.com:44369" | the address of server for submitting result |
| Identifier | text | "iAsia.Example.A" | identifier in server identifying this machine uniquely |
| Runner | text | "HarddiskRemainLow" | the name of runner to be run in this invocation |

### Splitting Configuration

Configuration can be splitted into multiple JSON files. Just add those files as parameter when invoking the program.

In order to avoid duplication, you are adviced to split configuration into different files in a structured manner.

    CheckLogWorker.exe Program.json HarddiskRemainLow.json

# Runners

There are many runners used for different purposes. And each of them requires different configuration fields.

- Harddisk Remain Low
- Harddisk Over-Growth
- KPI Queue Runner

## Harddisk Remain Low Runner

Checks the harddisk space and fire warning when space is running low.

````
{
  "Server": "https://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "HarddiskRemainLowRunner",
  "Drive": "C:\\",
  "WarnSize": "10GB",
  "ErrorSize": "500MB"
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

Checks the harddisk growth speed and fire warning when space is used fast

````
{
  "Server": "https://localhost:44369",
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

## KPI Queue Runner

Checks our KPI.txt and raise error when queue is not processing fast enough

````
{
  "Server": "https://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "KpiQueueRunner",
  "FilePattern": "C:\\iAsia\\iTrade\\iAsiaLogs\\<yyyyMMdd>\\kpi.txt",
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
| FilePattern | tagged path | "C:\\iAsiaLogs\\\<yyyyMMdd\>\\kpi.txt" | the *kpi.txt* path with date format tag |
| IgnoreQueuePatterns | regex array | [] | regex patterns for ignoring queue names |
| WarnProportion | number | 0.01 | unprocessed proportion in queue triggering a warning |
| WarnCount | number | 100 | unprocessed amount in queue which calming a warning |
| ErrorProportion | number | 0.01 | unprocessed proportion in queue triggering an error |
| ErrorCount | number | 100 | unprocessed amount in queue which calming an error |

> Suggested to be called *every 15 minutes*
