# Development Notes

## Design Evolution

The project began as a connector-toggle script running on a transport ship.

Investigation of Space Engineers PB API limitations revealed:

* Programmable Blocks cannot discover arbitrary disconnected grids.

* Landing-gear-attached grids are not exposed through GridTerminalSystem in a useful way.

* A PB on a gooseEgg cannot be triggered directly from the goose cockpit.

The design evolved toward a station-centric architecture.

***

## Key Discovery

Testing confirmed:

1. A station PB can see its own connector before docking.

2. The station PB can lock the station connector.

3. Once locked, the station PB can access the connected gooseEgg grid.

4. Cargo inventories on the gooseEgg become visible to the station PB.

This discovery enabled automatic fill-percentage monitoring.

***

## Container Philosophy

A gooseEgg should remain:

* Lightweight

* Passive

* Reusable

The final design intentionally avoids:

* Antennas

* Batteries

* Programmable Blocks

* Event Controllers

* Timer Blocks

on the container.

***

## Reconnect Loop Prevention

A major design challenge was preventing:

Connect → Process → Disconnect → Connect → Process → Disconnect

loops.

The solution is the WaitingForContainerRemoval state.

The dock must become Unconnected before another container may be processed.

***

## Future Research

Potential future investigation areas:

* Multi-dock stations

* Automatic container identification

* Freight statistics

* LCD dashboards

* Fleet management

## 2025-07 Development Notes

### PB Compiler Discovery

The in-game PB compiler rejected Program.cs files that included:

- using statements
- explicit Program class declarations

A PB-native implementation format is required.

### Connector Discovery Change

Connector discovery was revised to:

- Search only the PB's construct.
- Require exactly one matching connector.
- Enter Error state if zero or multiple matches are found.

This prevents accidental operation on the wrong dock.
