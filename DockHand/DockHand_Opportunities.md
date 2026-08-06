# Opportunities

This document captures potential future improvements to the DockHand project.
An Opportunity is intentionally lightweight. It records an idea worth considering,
not a commitment to perform the work.

## Opportunity Index

| ID | Title |
|---|---|
| O-001 | *Harmonize product documentation to reflect Version 1.1 architecture* |
| O-002 | *Provide in-game, visible DockHand status to player* |
| O-003 | *Permit Managed Connectors time to settle before locking* |
| O-004 | *Ensure that the grid's power is charged before disconnecting* |


---
## O-001 *Harmonize product documentation to reflect Version 1.1 architecture*

### Why?
To support product identity and avoid confusion for future revisions of the product.
 
### Context
The architecture, design, taxonomy, workflow, and code have evolved. One or more product documents still reflect the old architecture and terminology. For example, the Design Document still describes the original goose/gooseEgg scenario even though the implementation now supports any participating docked grid.

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

---

# Opportunity Template

## O-004 Ensure that the grid's power is charged before disconnecting

### Why?

An automated grid that house a Managed Connector will eventually run out of power and crash if it is not charged sufficiently.

### Context

The DockHand is certainly useful for player-piloted grids (ships, containers, etc.) But the real benefit is using automated drones between stations. DockHand monitors the Fill% of the grid's storage. It should also monitor the grid's power storage and only release the grid when it is both filled in cargo and power.

---

# Opportunity Template

## O-nnn *Short, descriptive title*

### Why?

Why might this opportunity matter? Why should the reader keep reading? Describe the value, benefit, or problem being addressed. Keep it short, one or two sentences at most.

### Context

Record any background information, observations, references, assumptions,
or musings that may help decide whether to take this Opportunity.

---
