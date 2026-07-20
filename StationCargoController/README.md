# StationCargoController

Automated loading and unloading station controller for **Space Engineers Version 1**.

StationCargoController runs entirely on a station Programmable Block and
automates cargo transfer for detachable cargo containers ("gooseEggs").
The transport ship and cargo container remain passive and require no
automation of their own.

---

## Features

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

This entry is placed in the **arriving connector's** Custom Data—not in
the station's Programmable Block.

Connectors that do not advertise participation remain connected but are
excluded from cargo automation.

---

## Operational Summary

For participating connectors the station performs the following sequence:

1. Detect a connectable connector.
2. Lock the connector.
3. Verify participation.
4. Monitor cargo fill percentage.
5. Transfer cargo until the configured threshold is reached.
6. Wait the configured disconnect delay.
7. Disconnect.
8. Wait for container removal before accepting another container.

---

## Repository

```text
StationCargoController/
├── PB-Script.cs
├── README.md
├── DevelopmentNotes.md
├── SE1-StationCargoController-DesignDoc.md
└── TestResults.md
```

---

## Documentation

| Document | Purpose |
|----------|---------|
| README.md | Project overview and configuration |
| DevelopmentNotes.md | Engineering decisions and project evolution |
| SE1-StationCargoController-DesignDoc.md | System architecture and design rationale |
| TestResults.md | In-game verification history |

---

## Project Status

Current implementation:

**Version 1.1**

Major capabilities:

* Load stations
* Unload stations
* Startup recovery
* WaitingForContainerRemoval latch
* Connector participation contract
* ReportAndWait handling for non-participating connectors