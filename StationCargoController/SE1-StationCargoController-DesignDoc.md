# SE1-StationCargoController Design Document (Version 1)

## Project Overview

### Purpose

Create a Space Engineers Version 1 Programmable Block script that automates loading and unloading operations for detachable cargo containers ("gooseEggs").

The automation resides entirely on the **station**, not on the transport ship ("goose") and not on the cargo container ("gooseEgg").

### Primary Design Goals

* Pilot remains in cockpit.
* gooseEgg remains lightweight and passive.
* No antenna required.
* No PB required on gooseEgg.
* No Event Controller required on gooseEgg.
* No timer blocks required.
* Single managed docking connector.
* Support both loading and unloading stations.
* Configuration via PB Custom Data.

---
### Space Engineers In-Game PB Compatibility Notes
Discovery

Testing revealed that the Space Engineers in-game PB compiler does not accept source files in Visual Studio / MDK format.

The PB compiler automatically supplies the Program class context.

Repository Form

May contain:

using ...
public sealed class Program : MyGridProgram
{
}
In-Game PB Form

Must contain only:

public Program()
{
}

public void Main(string argument)
{
}

plus helper methods and fields.

No explicit Program class declaration.

# System Architecture

## goose (Transport Ship)

Blocks:

* Cockpit
* Landing Gear
* Thrusters
* Power

Responsibilities:

* Transport containers.
* Lock and unlock landing gear.
* Pilot operations.

No scripting required.

---

## gooseEgg (Cargo Container)

Blocks:

* Connector
* 3–4 Cargo Containers

Responsibilities:

* Store cargo.
* Dock with stations.

No:

* Programmable Block
* Antenna
* Event Controller
* Timer Block

---

## Station

Blocks:

* Connector
* Programmable Block
* Storage
* Conveyors
* Sorters

Responsibilities:

* Detect connectable containers.
* Lock connector automatically.
* Monitor fill percentage.
* Disconnect automatically.
* Report status.

---

# Configuration

Stored in PB Custom Data.

Example:

```ini
[Station]
Mode=Load
Threshold=95
DisconnectDelaySeconds=10
ConnectorName=Cnx
```

## Parameters

### Mode

Valid values:

* Load
* Unload

### Threshold

Completion threshold.

Examples:

* 95
* 5

### DisconnectDelaySeconds

Wait period after threshold reached before disconnecting.

### ConnectorName

Exact connector name.

---

# Runtime

## Update Frequency

```csharp
Runtime.UpdateFrequency = UpdateFrequency.Update100;
```

This provides:

* Low CPU usage
* Fast enough response for docking operations

---

# Container Discovery

## Station Connector

Found using exact name matching.

Example:

```ini
ConnectorName=Cnx
```

## Connected Container

After the connector is locked:

```text
Station Connector
        ||
gooseEgg Connector
```

The station gains visibility into gooseEgg blocks and inventories.

## Cargo Containers

Use all:

```csharp
IMyCargoContainer
```

blocks found on the connected gooseEgg grid.

Do not include:

* Connectors
* Cockpits
* Drills
* Refineries
* Assemblers

even if they have inventories.

---

# Fill Percentage Calculation

Use volume.

Formula:

```text
Total Current Volume
--------------------
Total Maximum Volume
```

Result:

```text
0.0% - 100.0%
```

---

# Threshold Rules

## Load Mode

Complete when:

```csharp
fillPercentage >= threshold
```

## Unload Mode

Complete when:

```csharp
fillPercentage <= threshold
```

---

# State Machine

```csharp
enum StationState
{
    WaitingForContainer,
    Processing,
    DisconnectPending,
    WaitingForContainerRemoval,
    Error
}
```

---

## WaitingForContainer

Waiting for:

```csharp
Connector.Status == Connectable
```

Action:

```csharp
Connect()
```

Transition:

```text
Processing
```

---

## Processing

Monitor:

```text
Container fill %
```

When threshold reached:

```text
DisconnectPending
```

---

## DisconnectPending

Wait:

```text
DisconnectDelaySeconds
```

Action:

```csharp
Disconnect()
```

Transition:

```text
WaitingForContainerRemoval
```

---

## WaitingForContainerRemoval

Purpose:

Prevent reconnect loops.

Ignore:

```csharp
Connectable
```

Wait for:

```csharp
Connector.Status == Unconnected
```

Transition:

```text
WaitingForContainer
```

---

## Error

Entered when:

* Connector missing
* Invalid configuration
* Container discovery failure
* Other unrecoverable condition

---

# Startup Recovery

If the world loads and a container is already connected:

```text
Connected
```

The script shall:

* Recover automatically.
* Continue monitoring.
* Require no operator intervention.

---

# Operational Workflow

## Loading Station

```text
Container Arrives
        ↓
Connectable
        ↓
Connect
        ↓
Loading
        ↓
Threshold Reached
        ↓
Wait Delay
        ↓
Disconnect
        ↓
Wait For Removal
```

---

## Unloading Station

```text
Container Arrives
        ↓
Connectable
        ↓
Connect
        ↓
Unloading
        ↓
Threshold Reached
        ↓
Wait Delay
        ↓
Disconnect
        ↓
Wait For Removal
```

---

# Additional Scenarios with Proposed Solutions

## Scenario: Processed Container Remains On Dock

Observation:

After disconnect:

```text
Connected
↓
Connectable
```

Problem:

The script could reconnect endlessly.

Solution:

```text
WaitingForContainerRemoval
```

Ignore reconnect opportunities until:

```text
Unconnected
```

---

## Scenario: Connector Jitter After Disconnect

Theoretical transition:

```text
Connectable
↓
Unconnected
↓
Connectable
```

### Version 1 Decision

Do not implement mitigation.

Reason:

* Not observed during testing.
* Connectors appear mechanically stable.

### Future Enhancement

```ini
DockClearSeconds=3
```

Require:

```text
Unconnected
```

for the specified duration before reset.

---

## Scenario: World Loads With Connected Container

### Solution

Automatically enter:

```text
Processing
```

if a container is already connected.

---

## Scenario: Container Already Above Threshold At Docking

Example:

```text
Mode=Load
Threshold=95
Current Fill=100
```

### Proposed Behavior

The station shall:

1. Connect normally.
2. Evaluate fill percentage.
3. Immediately enter DisconnectPending.
4. Wait DisconnectDelaySeconds.
5. Disconnect.

This prevents a permanently docked container.

---

## Scenario: Container Already Below Threshold At Docking

Example:

```text
Mode=Unload
Threshold=5
Current Fill=0
```

### Proposed Behavior

The station shall:

1. Connect normally.
2. Evaluate fill percentage.
3. Immediately enter DisconnectPending.
4. Wait DisconnectDelaySeconds.
5. Disconnect.

---

## Scenario: Cargo Transfer Stalls

### Observation

A sorter, conveyor, or inventory issue may prevent fill percentage from changing.

### Version 1 Decision

No timeout implemented.

The station remains in:

```text
Processing
```

until threshold is reached or the connection is lost.

### Future Enhancement

Implement:

```ini
ProcessingTimeoutMinutes=30
```

to raise an error and disconnect.

---

# Error Handling

Report via:

```csharp
Echo()
```

Examples:

```text
Connector not found.
Multiple matching connectors.
Invalid mode.
Invalid threshold.
No cargo containers detected.
Unable to identify connected container.
```

---

# Repository Structure

```text
SpaceEngineers1-Scripts/
│
├── StationCargoController/
│   ├── Program.cs
│   ├── README.md
│   ├── DevelopmentNotes.md
│   └── SE1-StationCargoController-DesignDoc.md
│
├── LICENSE
└── README.md
```

---

# Finalized Project Prompt

Create a Space Engineers Version 1 Programmable Block script written in C#.

The script shall:

1. Run on a cargo station.
2. Read configuration from Custom Data.
3. Locate a station connector by exact name.
4. Detect Connectable state.
5. Automatically connect.
6. Discover the connected gooseEgg grid.
7. Locate all cargo containers on that grid.
8. Calculate aggregate fill percentage using cargo volume.
9. Support Load and Unload modes.
10. Use >= threshold for Load.
11. Use <= threshold for Unload.
12. Wait a configurable DisconnectDelaySeconds before disconnecting.
13. Enter WaitingForContainerRemoval after disconnect.
14. Ignore reconnect attempts while waiting for container removal.
15. Reset only when connector transitions to Unconnected.
16. Recover automatically from world reloads.
17. Report status via Echo().
18. Be fully documented and suitable for inclusion in the CGMjr/SpaceEngineers1-Scripts repository.
