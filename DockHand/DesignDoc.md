# DockHand Design Document (Version 1.1.0)

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
* Explicit connector participation contract.
* Support both loading and unloading stations.
* Managed Grid power-readiness fulfillment.
* Configuration via PB Custom Data and Managed Grid connector Custom Data.

---

## Space Engineers In-Game PB Compatibility Notes

Testing revealed that the Space Engineers in-game PB compiler supplies the
`Program` class automatically.

PB source files therefore contain only constructors, `Main()`, fields,
and helper methods. They do not declare the `Program` class explicitly.

---

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
* Determine connector participation.
* Monitor fill percentage.
* Monitor Managed Grid power readiness.
* Disconnect automatically when service requirements are fulfilled.
* Report status.

---

# Configuration

DockHand uses two configuration locations.

## Station Programmable Block Custom Data

Example:

```ini
[Station]
Mode=Load
Threshold=95
DisconnectDelaySeconds=10
ConnectWaitSeconds=1
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

Wait period after the service requirements are satisfied before
disconnecting.

### ConnectWaitSeconds

Required continuous Connectable interval before DockHand attempts to
connect.

### ConnectorName

Exact connector name.

---

## Managed Grid Connector Custom Data

Example:

```ini
[StationCargoController]
Managed=true
PowerThreshold=95
```

`Managed=true` is required for participation.

`PowerThreshold` is optional and defines the minimum aggregate battery
charge percentage required before DockHand may release the Managed Grid.

Valid declared values are 1 through 99 percent.

If `PowerThreshold` is omitted, DockHand uses a default requirement of
99 percent.

DockHand never modifies the Managed Grid's Custom Data.

---

# Connector Participation

Version 1.1 introduces an explicit participation contract between the
station and the arriving connector.

Only connectors that advertise participation are eligible for cargo
automation.

Configuration is stored in the arriving connector's Custom Data.

```ini
[StationCargoController]
Managed=true
```

The station evaluates only the connector physically attached to its
configured dock (`OtherConnector`).

The connected grid is never searched for participating connectors.

The station never modifies another grid's Custom Data.

This establishes a clear ownership boundary:

* The arriving connector decides whether it wishes to participate.
* The station decides how participating connectors are processed.

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

The fill percentage shall be calculated using volume.

Included inventories:

* All IMyCargoContainer inventories on the connected gooseEgg grid.
* The connected gooseEgg connector inventory.

Excluded inventories:

* Drills
* Refineries
* Assemblers
* Cockpits
* Any other inventory-bearing blocks

Formula:

```
Total Current Volume
--------------------
Total Maximum Volume
```

Where:

```
Total Current Volume =
    Sum(Cargo Containers Current Volume)
    + Connector Current Volume

Total Maximum Volume =
    Sum(Cargo Containers Maximum Volume)
    + Connector Maximum Volume
```

---

# Threshold Rules

## Cargo Threshold

### Load Mode

Complete when:

```csharp
fillPercentage >= threshold
```

### Unload Mode

Complete when:

```csharp
fillPercentage <= threshold
```

The cargo threshold is an existing service condition. It is distinct
from the O-004 power-readiness threshold.

---

# Power Readiness

A participating Managed Grid may declare a power-readiness requirement.

The Managed Grid declares the requirement; the station is responsible
for fulfilling it.

Power readiness is calculated from aggregate battery storage:

```text
Aggregate Current Stored Power
------------------------------
Aggregate Maximum Stored Power
```

expressed as a percentage.

Power readiness is satisfied when:

```text
Power percentage >= effective PowerThreshold
```

A Managed Grid with no applicable battery storage is considered
power-ready.

The effective requirement is:

* The Managed Grid's declared `PowerThreshold`, when present.
* Otherwise, the station default of 99%.

If the applicable power-readiness requirement is not satisfied, DockHand
does not release the Managed Grid.

If the requirement is satisfied, DockHand may proceed with the existing
release process.

Power readiness must remain satisfied throughout `DisconnectPending`.
If it becomes unsatisfied during the disconnect delay, DockHand returns
to `WaitingForPower`.

DockHand reports the effective power requirement, current applicable
power state, and readiness result through `Echo()`.

---

# O-004 Status

**Opportunity O-004 — Ensure that the grid's power is charged before
disconnecting — is Seized.**

The Managed Grid declares its fulfillment requirement and the station is
responsible for fulfilling it.

Investigation 010-005 established the O-004 Canonical Proofs for the
candidate implementation.

Investigation 010-006 verified the implementation delta from DockHand
v1.0.9 to DockHand v1.1.0 and established that the O-004 behavior was
incorporated without an unintended regression in the relevant existing
DockHand behavior.

The O-004 design is therefore part of DockHand Version 1.1.0.


# State Machine

```csharp
enum StationState
{
    WaitingForContainer,
    Processing,
    WaitingForPower,
    DisconnectPending,
    ReportAndWait,
    WaitingForContainerRemoval,
    Error
}
```

---

## WaitingForContainer

Waiting for:

```text
Connector.Status == Connectable
```

Action:

```csharp
Connect()
```

Evaluate participation.

Transition:

```text
Processing
```

or

```text
ReportAndWait
```

---

## Processing

Monitor:

```text
Container fill %
Power readiness
```

When the cargo threshold is not reached, remain in Processing.

When the cargo threshold is reached:

```text
Power Ready
    ↓ Yes
DisconnectPending

Power Ready
    ↓ No
WaitingForPower
```

---

## WaitingForPower

Purpose:

Wait for the Managed Grid to satisfy its applicable power-readiness
requirement.

Actions:

* Remain connected.
* Reevaluate power readiness.
* Report effective requirement and current power state.

Transition:

```text
Power requirement satisfied
        ↓
DisconnectPending
```

---

## DisconnectPending

Wait:

```text
DisconnectDelaySeconds
```

while continuing to evaluate power readiness.

If power readiness becomes unsatisfied:

```text
WaitingForPower
```

The pending release is abandoned.

If the delay expires while power readiness remains satisfied:

```csharp
Disconnect()
```

Transition:

```text
WaitingForContainerRemoval
```

---

## ReportAndWait

Purpose:

Handle connectors that do not participate in cargo automation.

Entered when:

* a connector successfully docks
* participation contract is absent or disabled

Actions:

* Report connector status.
* Perform no cargo automation.
* Leave connector locked.
* Wait for connector removal.

Transition:

```text
WaitingForContainer
```

after

```text
Unconnected
```

---

## WaitingForContainerRemoval

Purpose:

Prevent reconnect loops after processing a participating connector.

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
* Reconstruct the appropriate runtime state.
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
Continuous Connect Wait
        ↓
Connect
        ↓
Participation Check
      ↙         ↘
 Processing   ReportAndWait
      ↓             ↓
Loading      Wait Removal
      ↓             ↓
Cargo Threshold
Reached
      ↓
Power Ready?
   ↙       ↘
 No         Yes
 ↓           ↓
WaitingForPower
             ↓
       DisconnectPending
             ↓
       Power Still Ready?
          ↙       ↘
        No         Yes
        ↓           ↓
WaitingForPower  Wait Delay
                    ↓
                Disconnect
                    ↓
          WaitingForContainerRemoval
```

---

## Unloading Station

```text
Container Arrives
        ↓
Connectable
        ↓
Continuous Connect Wait
        ↓
Connect
        ↓
Participation Check
      ↙         ↘
 Processing   ReportAndWait
      ↓             ↓
Unloading    Wait Removal
      ↓             ↓
Cargo Threshold
Reached
      ↓
Power Ready?
   ↙       ↘
 No         Yes
 ↓           ↓
WaitingForPower
             ↓
       DisconnectPending
             ↓
       Power Still Ready?
          ↙       ↘
        No         Yes
        ↓           ↓
WaitingForPower  Wait Delay
                    ↓
                Disconnect
                    ↓
          WaitingForContainerRemoval
```

---

# Additional Scenarios with Proposed Solutions

## Scenario: Non-Participating Connector

Observation:

Not every connector docking at the station should be processed.

Solution:

Require explicit participation using:

```ini
[StationCargoController]
Managed=true
```

Connectors that do not advertise participation remain connected,
are reported to the operator, and are ignored until removed.

---

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

Automatically recover to the appropriate runtime state based upon
connector status and participation.

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
2. Evaluate participation.
3. If participating, evaluate fill percentage.
4. Immediately enter DisconnectPending.
5. Wait DisconnectDelaySeconds.
6. Disconnect.

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
2. Evaluate participation.
3. If participating, evaluate fill percentage.
4. Immediately enter DisconnectPending.
5. Wait DisconnectDelaySeconds.
6. Disconnect.

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
├── DockHand/
│   ├── PB-Script.cs
│   ├── README.md
│   ├── DevelopmentNotes.md
│   └── SE1-DockHand-DesignDoc.md
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
6. Evaluate the connected `OtherConnector` for participation.
7. Ignore connectors that do not participate.
8. Discover the connected gooseEgg grid.
9. Locate all cargo containers on that grid.
10. Calculate aggregate fill percentage using cargo volume.
11. Support Load and Unload modes.
12. Use >= threshold for Load.
13. Use <= threshold for Unload.
14. Wait a configurable DisconnectDelaySeconds before disconnecting.
15. Enter ReportAndWait for non-participating connectors.
16. Enter WaitingForContainerRemoval after disconnecting participating connectors.
17. Ignore reconnect attempts while waiting for container removal.
18. Reset only when connector transitions to Unconnected.
19. Recover automatically from world reloads.
20. Report status via Echo().
21. Be fully documented and suitable for inclusion in the CGMjr/SpaceEngineers1-Scripts repository.