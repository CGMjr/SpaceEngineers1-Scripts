// ============================================================================
// DockHand Investigation 009-002
// Disposable Connector Telemetry Probe
//
// PURPOSE:
// Gather raw telemetry needed to determine whether world-position data can
// distinguish a Managed Connector that has settled from one that is merely
// approaching.
//
// INSTALLATION:
// Run this script in a Programmable Block on the MOVABLE TEST GRID.
//
// REQUIRED:
// Set CONNECTOR_NAME to the exact name of the Managed Connector on that grid.
//
// COMMANDS:
//
//   calibrate
//      Use while CONNECTED to the station.
//      Records OtherConnector.GetPosition() as the stationary target position.
//
//   start
//      Clears the current observation run and begins sampling.
//
//   stop
//      Stops sampling.
//
//   clear
//      Clears the current observation run.
//
//   status
//      Displays current telemetry without changing state.
//
// The calibrated target position remains in memory while this compiled
// instance of the script remains alive. Recompile => recalibrate.
//
// Output is written to Echo().
// ============================================================================

const string CONNECTOR_NAME = "Managed Connector";

// Number of Update10 samples between logged observations.
// Update10 is approximately 6 executions/second.
// 3 => approximately 2 logged observations/second.
const int LOG_EVERY_N_SAMPLES = 3;

IMyShipConnector _connector;

Vector3D _targetPosition;
bool _calibrated = false;
bool _logging = false;

Vector3D _previousPosition;
bool _havePreviousPosition = false;

int _sampleCounter = 0;
int _logCounter = 0;

double _elapsedSeconds = 0.0;

StringBuilder _log = new StringBuilder();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    _connector = GridTerminalSystem.GetBlockWithName(CONNECTOR_NAME)
        as IMyShipConnector;

    if (_connector == null)
    {
        Echo("009-002 TELEMETRY PROBE");
        Echo("");
        Echo("ERROR:");
        Echo("Connector not found:");
        Echo(CONNECTOR_NAME);
        return;
    }

    Echo("009-002 TELEMETRY PROBE");
    Echo("");
    Echo("Connector:");
    Echo(_connector.CustomName);
    Echo("");
    Echo("Dock once, then run:");
    Echo("calibrate");
}

public void Main(string argument, UpdateType updateSource)
{
    if (_connector == null)
    {
        Echo("ERROR: Managed Connector not found.");
        return;
    }

    string command = argument == null
        ? ""
        : argument.Trim().ToLowerInvariant();

    if (command == "calibrate")
    {
        Calibrate();
        DisplayStatus();
        return;
    }

    if (command == "start")
    {
        StartLogging();
        DisplayStatus();
        return;
    }

    if (command == "stop")
    {
        _logging = false;
        DisplayStatus();
        return;
    }

    if (command == "clear")
    {
        ClearRun();
        DisplayStatus();
        return;
    }

    if (command == "status")
    {
        DisplayStatus();
        return;
    }

    if (_logging)
    {
        SampleTelemetry();
    }

    DisplayStatus();
}

void Calibrate()
{
    if (_connector.Status != MyShipConnectorStatus.Connected)
    {
        Echo("CALIBRATION FAILED");
        Echo("Connector must be Connected.");
        return;
    }

    IMyShipConnector other = _connector.OtherConnector;

    if (other == null)
    {
        Echo("CALIBRATION FAILED");
        Echo("OtherConnector unavailable.");
        return;
    }

    _targetPosition = other.GetPosition();
    _calibrated = true;

    Echo("CALIBRATED");
    Echo(VectorText(_targetPosition));
}

void StartLogging()
{
    if (!_calibrated)
    {
        Echo("START FAILED");
        Echo("Calibrate first.");
        return;
    }

    ClearRun();

    _logging = true;

    _log.AppendLine(
        "N | Time | Status | Distance | DeltaPos | ApproxSpeed");

    _log.AppendLine(
        "-------------------------------------------------------");
}

void ClearRun()
{
    _logging = false;

    _sampleCounter = 0;
    _logCounter = 0;
    _elapsedSeconds = 0.0;

    _havePreviousPosition = false;

    _log.Clear();
}

void SampleTelemetry()
{
    // Update10 nominally occurs every 10 simulation ticks.
    // At 60 simulation ticks/sec:
    //
    // 10 / 60 = 1/6 second.
    //
    // Runtime variation is possible; this is deliberately an
    // observational probe, not production timing logic.

    const double SAMPLE_SECONDS = 1.0 / 6.0;

    _sampleCounter++;
    _elapsedSeconds += SAMPLE_SECONDS;

    Vector3D currentPosition = _connector.GetPosition();

    double distance =
        Vector3D.Distance(currentPosition, _targetPosition);

    double deltaPosition = 0.0;
    double approximateSpeed = 0.0;

    if (_havePreviousPosition)
    {
        deltaPosition =
            Vector3D.Distance(
                currentPosition,
                _previousPosition);

        approximateSpeed =
            deltaPosition / SAMPLE_SECONDS;
    }

    _previousPosition = currentPosition;
    _havePreviousPosition = true;

    if (_sampleCounter % LOG_EVERY_N_SAMPLES != 0)
        return;

    _logCounter++;

    _log.Append(_logCounter);
    _log.Append(" | ");

    _log.Append(_elapsedSeconds.ToString("0.00"));
    _log.Append(" | ");

    _log.Append(_connector.Status.ToString());
    _log.Append(" | ");

    _log.Append(distance.ToString("0.0000"));
    _log.Append(" | ");

    _log.Append(deltaPosition.ToString("0.000000"));
    _log.Append(" | ");

    _log.Append(approximateSpeed.ToString("0.0000"));

    _log.AppendLine();
}

void DisplayStatus()
{
    Echo("=== Investigation 009-002 ===");
    Echo("Connector Telemetry Probe");
    Echo("");

    Echo("Connector: " + _connector.CustomName);
    Echo("State: " + _connector.Status);

    Vector3D currentPosition = _connector.GetPosition();

    Echo("");
    Echo("Connector position:");
    Echo(VectorText(currentPosition));

    Echo("");

    if (_calibrated)
    {
        Echo("Target position:");
        Echo(VectorText(_targetPosition));

        double distance =
            Vector3D.Distance(
                currentPosition,
                _targetPosition);

        Echo("");
        Echo("Distance: " +
            distance.ToString("0.0000") + " m");
    }
    else
    {
        Echo("Target: NOT CALIBRATED");
    }

    Echo("");
    Echo("Logging: " +
        (_logging ? "ACTIVE" : "STOPPED"));

    Echo("Samples logged: " + _logCounter);

    if (_log.Length > 0)
    {
        Echo("");
        Echo("---- OBSERVATIONS ----");
        Echo(_log.ToString());
    }
}

string VectorText(Vector3D value)
{
    return
        "X=" + value.X.ToString("0.000") +
        " Y=" + value.Y.ToString("0.000") +
        " Z=" + value.Z.ToString("0.000");
}