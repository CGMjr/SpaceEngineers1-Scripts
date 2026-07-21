# DockHand

DockHand is a Space Engineers Programmable Block script that automates routine cargo transfer operations at docking stations, allowing players to focus on the fun stuff rather than repetitive logistics.

# Why "DockHand"?

A dock hand assists the operator by performing the repetitive tasks required to keep cargo moving efficiently.

DockHand follows the same philosophy. Rather than replacing the player, DockHand handles routine dock operations so the player can focus on designing, flying, and building.

DockHand is intended to feel like a dependable crew member rather than another machine that requires constant attention.

---

## Features

DockHand operates from a station's Programmable Block. It manages a designated station connector and cooperates with participating cargo containers through a simple connector contract. This station-centric design keeps arriving containers simple while allowing docking operations to be automated.

* Automatic connector locking.
* Automatic disconnect using configurable fill thresholds.
* Load and Unload operating modes.
* Startup recovery after recompilation or world reload.
* WaitingForContainerRemoval latch to prevent reconnect loops.
* Explicit connector participation contract.
* Single managed station connector.
* Configuration through Custom Data.

---

## Station Configuration

Configure the station Programmable Block using Custom Data.

Example:

```ini
[Station]
Mode=Load
Threshold=95
DisconnectDelaySeconds=10
ConnectorName=Cnx
```

| Setting | Description |
|----------|-------------|
| Mode | Load or Unload |
| Threshold | Completion percentage |
| DisconnectDelaySeconds | Delay before disconnecting |
| ConnectorName | Exact name of the managed station connector |

---

## Connector Participation

Version 1.1 introduces an explicit participation contract.

Only arriving connectors that advertise participation are processed.

Example:

```ini
[StationCargoController]
Managed=true
```

This entry is placed in the **arriving connector's** Custom Data—not in the station's Programmable Block.

Connectors that do not advertise participation remain connected but are excluded from cargo automation.

---

## Operational Summary

For participating connectors the station performs the following sequence:

1. Detect a connectable connector.
2. Lock the connector.
3. Verify participation.
4. Monitor cargo fill percentage.
5. Wait the configured disconnect delay.
6. Disconnect.
7. Wait for container removal before accepting another container.

---

## Repository

```text
DockHand/
├── PB-Script.cs
├── README.md
├── DevelopmentNotes.md
├── SE1-DockHand-DesignDoc.md
└── TestResults.md
```

---

## Documentation

| Document | Purpose |
|----------|---------|
| README.md | Project overview and configuration |
| DevelopmentNotes.md | Engineering decisions and project evolution |
| SE1-DockHand-DesignDoc.md | System architecture and design rationale |
| TestResults.md | In-game verification history |

---

## Current Capabilities

Current Release:

**Version 1.1**

Major capabilities:

* Load stations
* Unload stations
* Startup recovery
* WaitingForContainerRemoval latch
* Connector participation contract
* ReportAndWait handling for non-participating connectors