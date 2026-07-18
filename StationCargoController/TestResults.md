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


## Session 006

Date: 07/17/26
Game version: 1.209.024

### Test 006-01 – Empty Container Arrival

#### Objective

Verify correct behavior when a gooseEgg arrives already below the unload threshold.

#### Setup

- Configure station for Unload mode.
- Prepare a gooseEgg with fill percentage below the configured threshold (0–4%).
- Dock gooseEgg at the unload station.

#### Expected Results

1. Station connector automatically locks.
2. Script identifies connected inventories.
3. Fill percentage is calculated correctly.
4. Threshold is immediately recognized as satisfied.
5. Disconnect delay begins.
6. Connector disconnects after delay expires.
7. WaitingForContainerRemoval state activates.
8. Script does not reconnect while container remains present.

#### Actual Results

1. Pass
2. Pass
3. Pass
4. Pass
5. Pass
6. Pass
7. Pass
8. Pass

#### Pass / Fail

PASS

### Test 006-02 – Conveyor Path Failure

#### Objective

Verify behavior when cargo transfer cannot occur after docking.

#### Setup

- Configure station for Unload mode.
- Prepare a partially loaded gooseEgg.
- Intentionally break the conveyor path by disabling a sorter, disabling a conveyor block, or otherwise preventing inventory transfer.
- Dock gooseEgg at the unload station.

#### Expected Results

1. Station connector automatically locks.
2. Script identifies connected inventories.
3. Fill percentage is calculated correctly.
4. Cargo transfer does not occur.
5. Fill percentage remains above threshold.
6. Script remains in Processing state.
7. Disconnect delay does not begin.
8. Connector remains connected.
9. No error state occurs.

#### Actual Results

1. Pass
2. Pass
3. Pass
4. Pass
5. Pass
6. Pass
7. Pass
8. Pass
9. Pass

#### Pass / Fail

PASS

### Test 006-03 – Mid-Transfer Departure

#### Objective

Verify recovery when the container departs before unloading is complete.

#### Setup

- Configure station for Unload mode.
- Prepare a partially loaded gooseEgg.
- Dock gooseEgg and allow unloading to begin.
- Before threshold is reached, force connector separation by forcing a connection unlock (was: moving the transport ship away from the dock. This is not possible without unlock.)

#### Expected Results

1. Cargo transfer begins normally.
2. Connection is unexpectedly lost.
3. Script detects disconnect.
4. Script exits Processing state.
5. Script returns to WaitingForContainer state.
6. No error state occurs.
7. No reconnect attempts occur.
8. Script remains ready for the next docking operation.

#### Actual Results

DNF

#### Pass / Fail

FAIL

### Test 006-04 – Recompile While Connected

#### Objective

Verify startup recovery while unloading is actively in progress.

#### Setup

- Configure station for Unload mode.
- Dock a partially loaded gooseEgg.
- Allow unloading to begin.
- While cargo transfer is active, open the PB editor and recompile the script.

#### Expected Results

1. Script recompiles successfully.
2. Startup recovery logic executes.
3. Connected connector is rediscovered.
4. Connected inventories are rediscovered.
5. Fill percentage calculation resumes correctly.
6. Processing state is reconstructed.
7. Unloading continues normally.
8. Threshold detection still functions.
9. Disconnect occurs normally after threshold is reached.

#### Actual Results

1. Pass
2. Pass
3. Pass
4. Pass
5. Pass
6. Pass
7. DNC
8. DNC
9. DNC

#### Pass / Fail

FAIL: unloading occurs too fast to hit recompile. Must retest with an additional tester.

### Test 006-05 – WaitingForContainerRemoval Recovery

#### Objective

Verify latch-state persistence across restart scenarios.

#### Setup

- Complete a normal unload cycle.
- Allow script to enter WaitingForContainerRemoval state.
- Leave gooseEgg positioned at the dock.
- Perform one or more of the following:
  - Recompile PB
  - Save and reload world
  - Power-cycle Programmable Block

#### Expected Results

1. Startup recovery executes.
2. Script recognizes container is still present.
3. WaitingForContainerRemoval state is restored.
4. Connector remains disconnected.
5. No reconnect attempts occur.
6. Script refuses to process the same container again.
7. After container departure, script returns to WaitingForContainer state.
8. New containers can be processed normally.

#### Actual Results

1. PASS
2. PASS
3. PASS
4. PASS
5. PASS
6. PASS
7. PASS
8. PASS


#### Pass / Fail

PASS

### Session Summary

| Test ID | Description | Result |
|----------|-------------|----------|
| 006-01 | Empty Container Arrival | PASS |
| 006-02 | Conveyor Path Failure | PASS |
| 006-03 | Mid-Transfer Departure | FAIL |
| 006-04 | Recompile While Connected | FAIL |
| 006-05 | WaitingForContainerRemoval Recovery | PASS |

### Overall Session Result

ALL TESTS THAT COULD BE EXECUTED PASSED. 006-3 & 006-4 REQUIRE TWO OR MORE TESTERS TO EXECUTE.
