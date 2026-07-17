#Introduction
This is a record of in-game test results for StationCargoController code. 
#Test Results
## Session 001

Date: 7/16/2026
Game version: 1.209.024

### Compilation
- Script did not compile.
### Notes
Version 1 code was generated for: MDK-SE projects, Visual Studio projects, Standalone .cs

## Session 002

Date: 7/16/2026
Game version: 1.209.024

### Compilation
- StationCargoController v1.0.1
  - PASS
### Runtime Validation
- Custom Data Parsing
  - PASS
- Connector Discovery
  - PASS
- Auto Connect
  - PASS
- OtherConnector
  - PASS
- Connected Grid Discovery
  - PASS
- Cargo Container Discovery
  - PASS
- Volume Fill Calculation
  - PASS
### Notes
Station successfully connected an unconnected gooseEgg.
Reported fill percentage matched the actual container fill level.
Confirmed that the station can discover and inspect inventories on the connected gooseEgg grid.

## Session 003
Date: 7/16/2026
Game version: 1.209.024

### Fill Percentage Regression

A bug was introduced while adding support for the gooseEgg connector inventory.

The cargo-container volume calculations were accidentally removed and replaced with repeated connector inventory calculations.

Result:
- Fill percentage incorrectly reported 100%.

Resolution:
- Restore cargo-container calculations.
- Add connector inventory exactly once after aggregating all cargo-container inventories.

Status:
PASS after correction.

## Session 004
Date: 7/16/2026
Game version: 1.209.024

### Load Mode Validation

Tests Performed

- Fill gooseEgg above threshold.
- Observe threshold detection.
- Observe disconnect timer.
- Observe connector disconnect.
- Leave gooseEgg physically sitting on dock.

Results

- Threshold detection: PASS
- Disconnect timer: PASS
- Connector disconnect: PASS
- WaitingForContainerRemoval latch: PASS

## Session 005
Date: 7/17/2026
Game version: 1.209.024

### Unload Script Happy Path Tests

- The unload platform is complete. The latest version of the script, v1.0.6, was loaded into the PB. The INI entries were loaded and adjusted in the PB's Custom Data field. The INI entries were:
    [Station]
    Mode=Unload
    Threshold=5
    DisconnectDelaySeconds=10
    ConnectorName=goosePad-Cnx
- The unload platform's connector was renamed to goosePad-Cnx to match the INI entry.

Tests performed:

- Place a gooseEgg container on the unload platform. Confirm White-yellow-green ring state.
- Confirm Latch state of unload platform.
- Confirm correct gooosEgg fill percentage is calculated.
- Confirm inventories for gooseEgg and unload station are visible.
- Manually move inventory to attain <Threshold% in gooseEgg. Confirm Disconnect processing.
- Confirm Waiting for removal processing.
- Confirm Waiting for Container processing.

Result:
- Script behaved as expected, including locking the connectors and computing the correct fill percentage.
- After manually moving inventory, the script executed the disconnect and waiting for removal code.
- After gooseEgg was removed from the unload platform, the script set the correct latch state and executed the "WaitingForContainer" code.

Status:
All test PASSED.

