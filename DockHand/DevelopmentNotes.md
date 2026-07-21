# Development Notes

## Design Evolution

The project began as a connector-toggle script running on a transport
ship.

Investigation of Space Engineers PB API limitations revealed:

- Programmable Blocks cannot discover arbitrary disconnected grids.
- Landing-gear-attached grids are not exposed through
  GridTerminalSystem in a useful way.
- A PB on a gooseEgg cannot be triggered directly from the goose
  cockpit.

The design evolved toward a station-centric architecture.

------------------------------------------------------------------------

## Key Discovery

Testing confirmed:

1. A station PB can see its own connector before docking.
2. The station PB can lock the station connector.
3. Once locked, the station PB can access the connected gooseEgg grid.
4. Cargo inventories on the gooseEgg become visible to the station PB.

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



## 2026-07 Version 1.1 Design Evolution

Following completion of Session 007 test planning, the project
architecture was extended to support explicit connector participation.

### Connector Participation

Connector participation is determined exclusively from the configured
station connector's `OtherConnector`.

A participating connector advertises itself through the following
contract:

```ini
[StationCargoController]
Managed=true
```

Connectors that advertise participation enter the existing cargo
automation pipeline unchanged.

Connectors that do not advertise participation are intentionally
excluded from cargo automation. They remain connected in the
`ReportAndWait` state until they depart.

This design intentionally avoids:

- searching the connected grid for participating connectors
- modifying another grid's Custom Data

The station's responsibility is limited to evaluating the connector
directly attached to its configured dock.

### State Machine Extension

Version 1.1 introduces the `ReportAndWait` state to support
non-participating connectors.

The station reports that the connector is not participating, performs no
cargo automation, and waits for connector removal before returning to
`WaitingForContainer`.

### Architectural Boundary

Participation is determined before cargo processing begins. Once
participation has been established, the existing cargo-processing
pipeline remains unchanged.

### Engineering Process

The project workflow is now:

1. Analysis
2. Tests
3. Design
4. Implementation

The objective is to move architectural creativity into the analysis and
design phases so implementation primarily becomes the faithful
translation of agreed design decisions into code.

------------------------------------------------------------------------

## 2026-07 Evolution of Our Development Philosophy

Recent discussions prompted us to refine the language we use to describe how work moves from an idea to a completed feature.

Earlier entries in these notes describe the engineering process in terms of Analysis, Tests, Design, and Implementation. That description remains historically accurate and intentionally has not been revised. It records how we understood the project at that point in time.

During the Version 1.1 implementation we observed that implementation, testing, documentation, and design updates all traveled together on a single feature branch and were completed together under a single Definition of Done. This experience led us to recognize that the Work Item—not an individual artifact or GitHub Issue—is the fundamental unit of engineering work.

### Opportunities, Work Items, and Branches

We now distinguish three related, but separate, concepts.

An **Opportunity** is the recognition that something may be worth creating, improving, or investigating.

Analysis determines whether that Opportunity should become one or more **Work Items**.

A **Work Item** represents one coherent piece of work that produces a meaningful change to the project. A Work Item is independent of any particular tracking tool.

A **Git branch** is simply the implementation vehicle used to perform a Work Item. The branch exists to isolate changes while the Work Item is completed; it is not the Work Item itself.

The relationship can be summarized as follows:

```text
Opportunity
    ↓
Analysis
    ↓
0..N Work Items
    ↓
1 implementation branch
    ↓
1 coherent product change
    ↓
1..N affected artifacts
    ↓
Definition of Done
    ↓
Merge
```

The branch is therefore a temporary implementation mechanism, while the Work Item represents the enduring unit of engineering work.

### Evolving Taxonomy

An important lesson from Version 1.1 is that our terminology has become more precise.

Early discussions frequently used the term **GitHub Issue** when describing work to be performed. Experience led us to recognize that this unintentionally allowed the tool to define the process.

We now separate three independent concerns:

- **Creative Workflow** — how ideas become software.
- **Work Tracking** — how Work Items are organized, prioritized, and monitored.
- **GitHub Workflow** — how Git and GitHub are used to implement, review, and integrate completed Work Items.

GitHub Issues remain an excellent mechanism for tracking Work Items within this repository, but they are no longer considered the definition of the development process itself.

This refinement better reflects the philosophy that software development is fundamentally a creative activity supported by tools rather than a process defined by those tools.

It also reinforces one of the project's guiding principles: implementation should be the faithful execution of decisions made during analysis and design, while the project's terminology should describe the engineering concepts themselves rather than the particular tools used to manage them.