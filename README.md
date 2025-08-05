# PVT
This repository contains the source code and documentation for Attention Test — a .NET Avalonia-based application designed to perform the Psychomotor Vigilance Task (PVT).

## Installation
NOTE: Only Windows OS is currently supported.

1. Download the latest release and unzip it
2. Launch the application using `AttTest.exe`

## Usage
1. Launch the application using `AttTest.exe`

2. The application uses configuration files to configure the experiment. 
    Example configuration file:
    ```json
    {
      "RoundLengthMinSeconds": 2,
      "RoundLengthMaxSeconds": 5,
      "FocusPointVisibilityLength": 355,
      "TestLengthSeconds": 60,
      "IntroLengthSeconds": 2,
      "FalseStartPointVisibleLength": 100
    }
    ```
3. Follow the instructions
4. Experiment results will be generated after the experiment ends.  The files will be stored in the same directory as the `AttTest.exe`. 
   The files are: `time_table_{name}_{date}.csv` and `{name}_{date}.csv`

### Output files

Example of `time_table_{name}_{date}.csv`:
```csv
stimulus;keyPress;type
3287.7347;3643.1038;miss
6390.1383;3661.6973;not-visible-early
```

Example of `{name}_{date}.csv`:

```csv
roundId;result;addInfo
0;355;Missed round
1;0;Too fast
2;272;
3;286;
4;268;
```


## Developer setup
1. Install .NET 5+
2. Open the software solution in the IDE of your choice