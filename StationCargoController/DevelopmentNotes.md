# Development Notes

## Design Evolution

The project began as a connector-toggle script running on a transport
ship.

Investigation of Space Engineers PB API limitations revealed:

-   Programmable Blocks cannot discover arbitrary disconnected grids.
-   Landing-gear-attached grids are not exposed through
    GridTerminalSystem in a useful way.
-   A PB on a gooseEgg cannot be triggered directly from the goose
    cockpit.

The design evolved toward a station-centric architecture.

------------------------------------------------------------------------

## Key Discovery

Testing confirmed:

1.  A station PB can see its own connector before docking.
2.  The station PB can lock the station connector.
3.  Once locked, the station PB can access the connected gooseEgg grid.
4.  Cargo inventories on the gooseEgg become visible to the station PB.

This discovery enabled automatic fill-percentage monitoring.

------------------------------------------------------------------------

## Container Philosophy

A gooseEgg should remain lightweight, passive, and reusable.

The final design intentionally avoids antennas, batteries, Programmable
Blocks, Event Controllers, and Timer Blocks on the container.

------------------------------------------------------------------------

## Reconnect Loop Prevention

A major design challenge was preventing repeated
connect/process/disconnect loops.

The solution is the `WaitingForContainerRemoval` state. The dock must
become **Unconnected** before another container may be processed.

------------------------------------------------------------------------

## 2026-07 Development Notes

### PB Compiler Discovery

The in-game PB compiler rejected `Program.cs` files containing `using`
statements or an explicit `Program` class declaration. A PB-native
implementation format is required.

### Connector Discovery Change

Connector discovery was revised to search only the PB's construct,
require exactly one matching connector, and enter the `Error` state if
zero or multiple matches are found.

### Fill Percentage Revision

Runtime testing revealed that the gooseEgg connector contains an
inventory. Fill-percentage calculations were revised to include both
cargo container inventories and the connected gooseEgg connector
inventory.

------------------------------------------------------------------------

## 2026-07 Version 1.1 Design Evolution

Following completion of Session 007 test planning, the project
architecture was extended to support explicit connector participation.

### Connector Participation

-   Participation is determined exclusively from the configured station
    connector's `OtherConnector`.
-   Only the arriving connector's Custom Data is inspected.
-   The connected grid is never searched.
-   The station never modifies another grid's Custom Data.
-   Automation is opt-in. Absence of a declaration is treated as
    non-participation.

Configuration contract:

``` ini
[StationCargoController]
Managed=true
```

### State Machine Extension

Version 1.1 introduces the `ReportAndWait` state. Non-participating
connectors remain locked, no cargo automation occurs, the current state
is reported, and the station waits for connector removal before
returning to `WaitingForContainer`.

### Architectural Boundary

Participation is determined before cargo processing begins. Once
participation has been established, the existing cargo-processing
pipeline remains unchanged.

### Implementation Strategy

A single helper method:

``` csharp
bool IsParticipatingConnector()
```

will encapsulate participation determination.

### Engineering Process

The project workflow is now:

1.  Analysis
2.  Tests
3.  Design
4.  Implementation

The objective is to move architectural creativity into the analysis and
design phases so implementation primarily becomes the faithful
translation of agreed design decisions into code.
