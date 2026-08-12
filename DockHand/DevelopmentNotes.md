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


------------------------------------------------------------------------

## 2026-07 Introducing a Product name: DockHand.

An important milestone after version 1.0.7 was the introduction of the concept of a product. The script was given the product name DockHand. This change had a larger impact than it might seem at first glance. It led to a long discussion on naming conventions, branding, user-facing information vs. programatic contract control, etc. We replaced the historical product identifier "StationCargoController" everywhere except where it forms part of the connector compatibility contract. The remaining documentation harmonization work was intentionally deferred by recording it as an Opportunity for a future Work Item.

## 2026-08 Opportunity O-003 — Configurable Connect Wait

Opportunity O-003 addresses DockHand issuing `Connect()` as soon as the
managed station connector becomes `Connectable`. The selected design
permits Space Engineers physics time to settle the arriving grid before
DockHand locks the connector.

### Implementation

DockHand now supports a station-specific configuration value:

```ini
ConnectWaitSeconds=1.0
```

The implementation retains `WaitingForContainer` as the state responsible
for detecting an arriving connector and adds internal pending-wait state
rather than introducing another station state.

When the configured station connector first becomes `Connectable`,
DockHand starts a connect-wait interval and does not call `Connect()` on
that update. While the connector remains continuously `Connectable`,
elapsed time is accumulated using `Runtime.TimeSinceLastRun`.

Only after the full configured interval has elapsed may DockHand issue
`Connect()`.

If the connector ceases to be `Connectable` before the interval completes,
the pending interval and all accumulated time are discarded. If the
connector later becomes `Connectable` again, DockHand begins a new full
wait interval. Time from an abandoned interval is never credited to the
new interval.

The existing behavior after a successful connection is unchanged:
participating connectors enter `Processing`; non-participating connectors
enter `ReportAndWait`.

### Configuration and Defensive Handling

`ConnectWaitSeconds` is read from the Programmable Block's existing
station configuration. The implementation uses a default of 1.0 second
when the setting is absent and clamps negative configured values to zero.

The production wait duration remains an empirical station-tuning choice.
The 1.0-second value is an implementation default, not evidence that one
second is sufficient for every docking situation.

### Implementation Shape

No new `StationState` value was introduced. The wait is subordinate to
`WaitingForContainer`, represented by:

- whether a connect wait is pending; and
- elapsed time in the current uninterrupted `Connectable` interval.

A small `AbandonConnectWait()` helper centralizes resetting both values.
This keeps the existing cargo-processing, disconnect-delay,
container-removal latch, participation contract, connector discovery, and
startup-recovery paths intact.

### Verification Status

This implementation was generated from the O-003 design obligations.
Those obligations require delayed connection, station-specific
configuration, continuous `Connectable` status for the full interval,
abandonment when `Connectable` is lost, and a fresh full interval after
`Connectable` returns.

The code has not yet been verified in the Space Engineers Programmable
Block environment. Existing functionality and the O-003 obligations
remain subject to the project's verification process.

## 2026-08 Opportunity O-004 — Power Readiness

Opportunity O-004 began with a simple product concern: an automated
cargo grid should not be released from a station until it has sufficient
power to perform its next mission.

The original Opportunity also included a cargo Fill% requirement. During
analysis, however, that apparently simple requirement exposed a much
larger design problem.

Space Engineers does not provide a simple way for DockHand to tell the
game's automated cargo-transfer mechanisms to stop at an arbitrary fill
percentage. Precise cargo fulfillment would require DockHand to assume
responsibility for selecting cargo, determining how much to transfer,
dealing with indivisible item quantities, and communicating the result
when an exact fill percentage could not be achieved.

That was a much larger product responsibility than the original
Opportunity warranted.

The resulting design discussion led to an important product decision:
cargo fulfillment and power readiness are separate concerns. The cargo
fulfillment problem was removed from the scope of O-004 and subsequently
captured as separate Opportunities rather than allowing it to distort
the design of power readiness.

### Product Design Insight

The purpose of variable cargo fill was reconsidered.

The actual problem it was intended to solve was:

> An overloaded grid may not be able to fly.

The simpler product solution is for the grid designer to build the cargo
grid with sufficient thrust and power to carry its intended maximum
cargo load. DockHand does not need to become a cargo-allocation system
merely because a product designer would prefer a more efficient ship.

This reinforced an important responsibility boundary:

> DockHand is responsible for station service requirements. The grid
> designer is responsible for designing a grid capable of performing its
> mission.

Cargo Discrimination and Fill Threshold were therefore removed from the
scope of O-004 and recorded as separate Opportunities.

### Evidentiary Investigation

The implementation was deliberately investigated before being accepted
as a production change.

Investigation 010-005 established that the O-004 candidate could satisfy
the Canonical Proofs.

The investigation established the following:

1. DockHand can prevent release when the Managed Grid's power-readiness
   requirement is not satisfied.
2. DockHand can release the Managed Grid when the requirement is
   satisfied.
3. DockHand can treat a Managed Grid with no applicable power storage as
   satisfying the power requirement.
4. DockHand can expose the effective power requirement and current power
   state through `Echo()`, making the behavior observable without access
   to internal Programmable Block variables.

The investigation therefore established the feasibility of the selected
realization.

### Selected Design

The selected realization for O-004 is:

> The Managed Grid declares its fulfillment requirements; the station is
> responsible for fulfilling them.

The Managed Grid declares its power-readiness requirement through the
Custom Data of its participating connector.

Example:

```ini
[StationCargoController]
Managed=true
PowerThreshold=95
```

`PowerThreshold` is optional. When absent, DockHand uses a default
requirement of 99 percent.

The requirement is fulfilled when the observed value meets or exceeds
the declared threshold.

If the requirement is not fulfilled, DockHand does not release the
Managed Grid.

A Managed Grid with no applicable power storage is considered
power-ready.

### Production Implementation

The investigation candidate was not promoted directly to production.

The implementation was compared with DockHand v1.0.9 to identify the
actual implementation delta required to incorporate O-004.

The resulting production candidate is DockHand v1.1.0.

The substantive implementation changes include:

- addition of the `WaitingForPower` state;
- evaluation of power readiness before release;
- continued evaluation while waiting for power;
- continued evaluation during `DisconnectPending`;
- abandonment of a pending release if power readiness becomes
  unsatisfied;
- reading `PowerThreshold` from the Managed Grid's participating
  connector;
- use of the 99 percent default when no declaration is present;
- aggregate battery stored-power calculation;
- treatment of no applicable battery storage as power-ready; and
- passive reporting of the power requirement, current state, and
  readiness.

Existing DockHand behavior was retained where it was not affected by
the O-004 requirement.

### Verification

Investigation 010-006 was created specifically to verify the delta
between DockHand v1.0.9 and DockHand v1.1.0.

This was intentionally not a second complete investigation of O-004.
Investigation 010-005 had already established the required behavior.

Investigation 010-006 instead asked whether the production
implementation incorporated that behavior while preserving the relevant
existing DockHand behavior.

Passes A through E were completed.

The Evidence Accounting established both O-004 Canonical Proofs:

* Canonical Proof I — Established.
* Canonical Proof II — Established.

The investigation therefore established that the DockHand v1.1.0
implementation satisfies the O-004 proof obligations without an
unintended regression in the relevant existing DockHand behavior.

### Design Outcome

Opportunity O-004 is **Seized**.

The completed work establishes the following product boundary:

> The Managed Grid declares what fulfillment it requires; DockHand is
> responsible for fulfilling that declaration before releasing the grid.

DockHand does not assume responsibility for deciding what cargo should
be loaded, how much cargo should be transferred, or how the grid should
be designed to carry its cargo.

Those concerns remain outside O-004.

### Development Process Observation

O-004 also provided useful evidence about the developing Evidentiary
method.

The Opportunity was proposed before its implementation feasibility was
known.

That was not a defect in the creative process.

The subsequent Evidentiary work exposed the cost and feasibility of the
original cargo-fill idea before implementation resources were committed
to it. The idea could therefore be reconsidered as a product decision
rather than being prematurely converted into an engineering obligation.

The process allowed the product designer to propose an idea, the
software designer to expose its consequences, and evidence to determine
whether the resulting realization was practical.

The lesson is not that product ideas must be technically feasible before
they are proposed.

The lesson is that the creative process must provide a disciplined way
to discover feasibility and cost before an idea becomes an implementation
commitment.

O-004 is the first Opportunity in the project where that distinction has
been particularly clear.
