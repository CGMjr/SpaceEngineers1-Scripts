# Opportunities

This document captures potential future improvements to the DockHand project.
An Opportunity is intentionally lightweight. It records an idea worth considering,
not a commitment to perform the work.

## Opportunity Index

| ID | Title | Disposition
|---|---|---|
| O-001 | *Harmonize product documentation to reflect Version 1.1 architecture* | Seized |
| O-002 | *Provide in-game, visible DockHand status to player* |
| O-003 | *Permit Managed Connectors time to settle before locking* | Seized |
| O-004 | *Ensure that the grid's power is charged before disconnecting* | Seized|
| O-005 | *Clarify Ownership of Fulfillment Requirements* | Terminated |
| O-006 | *Permit Managed Grids to declare cargo requirements* | |
| O-007 | *Remove the Fill Threshold requirement* | |


---
## O-001 *Harmonize product documentation to reflect Version 1.1 architecture*

### Why?
To support product identity and avoid confusion for future revisions of the product.
 
### Context
The architecture, design, taxonomy, workflow, and code have evolved. One or more product documents still reflect the old architecture and terminology. For example, the Design Document still describes the original goose/gooseEgg scenario even though the implementation now supports any participating docked grid.

### Disposition
Seized.
---

## O-002 *Provide in-game, visible DockHand status to player*

### Why?

Passively providing the current status of a DockHand terminal will enhance the gaming experience.

### Context

The currently depolyed version of DockHand has information about its configuration and state displayed in the terminal window of the game. But the player must find it, and there are multiple interactions (mouseclicks) to do so. Making that, or similar, information available without those interactions would streamline gameplay. Since the purpose of DockHand is to automate mundane, repetative tasks, this seems like a worthy revision.


---

## O-003 *Permit Managed Connectors time to settle before locking*

### Why?

GooseEggs are hovering over the surface and carrying ships such as the Goose are getting misaligned because managed connectors are locking too quickly.

### Context

In game a pair of connectors become "connectable" as the get closer but before they actually touch. DockHand triggers the locking very quickly. So connectors are getting locked before the gooseEgg (reusable containers) get settled on the deck. This also causes misalignment with the refueling connector that extends to refuel the Goose while it is waiting for the gooseEgg to load or unload. Some sort of delay between when DockHand senses "connectable" and telling the Managed connector to connect would likely solve this problem.

### Disposition
Seized.

---

## O-004 Ensure that the grid's power is charged before disconnecting

### Why?

An automated grid that house a Managed Connector will eventually run out of power and crash if it is not charged sufficiently.

### Context

The DockHand is certainly useful for player-piloted grids (ships, containers, etc.) But the real benefit is using automated drones between stations. DockHand monitors the Fill% of the grid's storage. It should also monitor the grid's power storage and only release the grid when it is both filled in cargo and power.

### Current Status

**Suspended.**

While evaluating candidate designs for this Opportunity, a more fundamental architectural question emerged concerning ownership of fulfillment requirements.

Specifically, the investigation raised the possibility that fulfillment requirements such as Cargo Fill% and Power Fill% may belong to the Managed Grid rather than the station.

Because the leading design candidate depended upon that responsibility assignment, further work on this Opportunity has been intentionally suspended.

Opportunity O-005, *Clarify Ownership of Fulfillment Requirements*, has been created to investigate that architectural question.

When O-005 reaches a conclusion, resume O-004 using the resulting responsibility assignments rather than the assumptions that existed when this Opportunity began.

---

## O-005 *Clarify Ownership of Fulfillment Requirements*

### Why?

DockHand should assign responsibilities to the stakeholder best able to define and satisfy them.

If fulfillment requirements belong to the wrong participant, otherwise sound features may reinforce an incorrect architecture.

### Context

While pursuing Opportunity O-004, *Ensure that the grid's power is charged before disconnecting*, the leading design candidate proposed allowing the station to define the required power charge before releasing a Managed Grid.

During evaluation of that candidate, an architectural concern emerged.

The discussion suggested that DockHand may have silently assumed the station owns the authority to define fulfillment requirements such as Cargo Fill% and Power Fill%.

A competing hypothesis emerged:

> The station owns fulfillment; the Managed Grid owns its fulfillment requirements.

If correct, the issue extends beyond O-004. Existing cargo behavior may also reflect the same responsibility assignment.

This Opportunity proposes examining the ownership of fulfillment requirements throughout DockHand before introducing additional features that depend upon them.

Questions to investigate include:

- Which stakeholder owns the authority to define fulfillment requirements?
- Which fulfillment requirements belong to the Managed Grid?
- Which responsibilities belong to the station?
- Which responsibilities belong to DockHand itself?
- Does the current architecture assign any of those responsibilities to the wrong participant?
- If responsibility assignments should change, what product behaviors would be affected?

The purpose of this Opportunity is **not** to redesign DockHand immediately.

Instead, determine whether the current responsibility assignments are architecturally sound.

If they are, Opportunity O-004 can resume with increased confidence.

If they are not, any architectural refactoring should become the result of this investigation rather than its starting assumption.

### Disposition
Terminated.

The investigation revealed that the rigor of Evidentiary was disproportionate to the risk and uncertainty of this Opportunity. The responsibility-assignment question is a conventional, low-risk design decision for which sufficient expertise and established design principles already exist. Rather than continue an unnecessarily elaborate investigation, the Opportunity is terminated. The experience exposed a limitation in Evidentiary itself: the method currently lacks a mechanism to determine whether, and to what degree, its rigor is warranted for a given Opportunity.

---

## O-006 *Permit Managed Grids to declare cargo requirements*

### Why?

Automated Managed Grids may require specific cargo from a station without player intervention. Allowing a Managed Grid to declare which cargo it will accept would enable DockHand to provide cargo service appropriate to the grid.

### Context

Different Managed Grids may have different cargo needs. For example, an automated grid serving IceZilla may require only ice, while an automated grid serving Derelict Station may require specific components or ingots.

Player-piloted grids do not necessarily require automated cargo loading, while automated grids may depend upon it.

A potential design is:

> The Managed Grid declares which cargo it will accept; the station's cargo service fulfills that declaration.

Space Engineers provides Programmable Block access to Conveyor Sorter configuration, including item filters and the `DrainAll` behavior. This may permit DockHand to control existing station cargo-service mechanisms without taking responsibility for selecting and transferring individual inventory quantities.

The feasibility, usefulness, and appropriate responsibility boundaries of this approach remain to be determined.

---

## O-007 *Remove the Fill Threshold requirement*

### Why?

The current Fill Threshold appears to address a problem that DockHand may not own: preventing a Managed Grid from becoming too heavily loaded to operate safely. The feature should be reconsidered to determine whether it represents an appropriate DockHand responsibility.

### Context

DockHand currently uses a Fill Threshold as part of determining when a Managed Grid has received sufficient cargo service.

During evaluation of Opportunity O-004, *Ensure that the grid's power is charged before disconnecting*, investigation of fulfillment requirements exposed a deeper product-design question.

The original rationale for a Fill Threshold was to help prevent an overloaded grid from becoming unable to fly. However, cargo fill percentage is only an indirect proxy for that problem because different cargo types have different mass.

The investigation also revealed a potentially more appropriate cargo-service model in which a Managed Grid declares which cargo it will accept and the station's cargo service fulfills that declaration. This model is being considered separately in Opportunity O-006, *Permit Managed Grids to declare cargo requirements*.

This Opportunity exists independently of O-006. Its purpose is to determine whether the existing Fill Threshold should remain a DockHand responsibility at all.

---

# Opportunity Template

## O-nnn *Short, descriptive title*

### Why?

Why might this opportunity matter? Why should the reader keep reading? Describe the value, benefit, or problem being addressed. Keep it short, one or two sentences at most.

### Context

Record any background information, observations, references, assumptions,
or musings that may help decide whether to take this Opportunity.

---
