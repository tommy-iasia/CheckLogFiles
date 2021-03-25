# Check Log Scheduler

You may use this scheduler instead of Window's TaskScheduler.

![preview](https://github.com/tommy-iasia/CheckLogFiles/blob/master/CheckLogScheduler/previews/21032501-preview.png?raw=true)

````
CheckLogScheduler.exe CheckLogScheduler.json
````
CheckLogScheduler.json
````
[
  {
    "Time": "**:30:00",
    "Arguments": ["HarddiskRemainLowRunner.json"]
  },
  {
    "Time": "**:%15:00",
    "Arguments": ["HarddiskOverGrowthRunner.json"]
  }
]
````

## Dependency

CheckLogScheduler.exe requires **CheckLogWorker.exe** in the same directory.

## Configuration

This time, the configuration file is simple. Only time and arguments are needed.

*Time* is an expression for triggering time.  
*Arguments* is an array to be passed to Check Log Worker when triggering.

### Time Expression

A time expression is of HH:mm:ss format. And patterns can be applied on all three fields.

| Example | Trigger | Pattern | Description |
|-|-|-|-|
| 12:34:56 | exactly 12:34:56 daily | Static number | giving a fixed number for the field |
| **9-16**:00:00 | hourly from 9am to 4pm | Range | range is applied on hour field |
| 9-16:**%15**:00 | every 15 minutes from 9am to 4pm | Interval | 15 minute interval on minute field |

## Lifecycle

One Scheduler can run multiple Workers according to time expressions in configuration.

And workers are run in **sequential** order with **15 minutes timeout** each.

## Single Instance

Scheduler only run one instance per file location. Newer instance will replace older instance through *CheckLogScheduler.Singleton.guid*.

Therefore, you are suggested to add a daily task in Windows' TaskScheduler to ensure scheduler is up and runing everyday.
