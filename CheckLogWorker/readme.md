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

## HarddiskRemainLow

Checks the harddisk space and fire a warning or an error when space is running low.

````
{
  "Server": "https://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "HarddiskRemainLow",
  "Drive": "C:\\",
  "WarnSize": "10GB",
  "ErrorSize": "500MB"
}
````

| Field | Format | Unit | Example | Description |
|-|-|-|-|-|
| Drive | text | | "C:\" | the drive to be chcked |
| WarnSize | size | B | "10GB" | fire warning when space is lower than the given value |
| ErrorSize | size | B | "500MB" | fire error when space is lower than the given value |

## HarddiskOverGrowth

Checks the harddisk growth speed and fire a warning or an error when space is used fast

````
{
  "Server": "https://localhost:44369",
  "Identifier": "iAsia.Example.A",
  "Runner": "HarddiskOverGrowth",
  "Drive": "C:\\",
  "WarnRate": "10GB/hr",
  "ErrorRate": "1GB/30min"
}
````

| Field | Format | Unit | Example | Description |
|-|-|-|-|-|
| Drive | text | | "C:\" | the drive to be chcked |
| WarnRate | size rate | B/s | "10GB/hr" | fire warning when usage is faster than the given value |
| ErrorSize | size rate | B/s | "1GB/30min" | fire error when usage is faster than the given value |
