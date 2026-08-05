// ============================================================================
// DockHand Investigation 009-002
// Disposable Connector Telemetry Probe - CSV Version
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
//      Stops sampling and writes the complete run as CSV into this
//      Programmable Block's Custom Data.
//
//   clear
//      Clears the current observation run and clears this PB's Custom Data.
//
//   status
//      Displays current telemetry without changing state.
//
// IMPORTANT:
// Custom Data on THIS PB is used solely as the CSV evidence store.
// Any existing Custom Data will be replaced when "stop" or "clear" is run.
//
// Calibration remains in memory while this compiled script remains alive.
// Recompile => recalibrate.
// ============================================================================

const string CONNECTOR_NAME = "Managed Connector";

IMyShipConnector _connector;

Vector3D _targetPosition;
bool _calibrated = false;
bool _logging = false;

Vector3D _previousPosition;
bool _havePreviousPosition = false;

int _sampleCounter = 0;

double _elapsedSeconds = 0.0;

StringBuilder _csv = new StringBuilder();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    _connector = GridTerminalSystem.GetBlockWithName(CONNECTOR_NAME)
        as IMyShipConnector;

    Echo("=== Investigation 009-002 ===");
    Echo("Connector Telemetry Probe - CSV");

    if (_connector == null)
    {
        Echo("");
        Echo("ERROR:");
        Echo("Connector not found:");
        Echo(CONNECTOR_NAME);
        return;
    }

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
        StopLogging();
        DisplayStatus();
        return;
    }

    if (command == "clear")
    {
        ClearRun();
        Me.CustomData = "";
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

    // Deliberately leave old Custom Data intact until this run is stopped.
    // This prevents an incomplete run from immediately destroying the
    // previously captured evidence.

    _csv.AppendLine(
        "Sample,TimeSeconds,Status,DistanceMeters," +
        "DeltaPositionMeters,ApproxSpeedMetersPerSecond," +
        "PositionX,PositionY,PositionZ");

    _logging = true;
}

void StopLogging()
{
    if (!_logging)
    {
        Echo("STOP:");
        Echo("Logging was not active.");
        return;
    }

    _logging = false;

    // Publish the completed evidence set.
    Me.CustomData = _csv.ToString();

    Echo("LOGGING STOPPED");
    Echo("");
    Echo("CSV written to this PB's");
    Echo("Custom Data.");
}

void ClearRun()
{
    _logging = false;

    _sampleCounter = 0;
    _elapsedSeconds = 0.0;

    _havePreviousPosition = false;

    _csv.Clear();
}

void SampleTelemetry()
{
    // Update10 nominally occurs every 10 simulation ticks.
    // At 60 simulation ticks/sec:
    //
    // 10 / 60 = 1/6 second.
    //
    // This is an observational probe, not production timing logic.

    const double SAMPLE_SECONDS = 1.0 / 6.0;

    _sampleCounter++;
    _elapsedSeconds += SAMPLE_SECONDS;

    Vector3D currentPosition = _connector.GetPosition();

    double distance =
        Vector3D.Distance(
            currentPosition,
            _targetPosition);

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

    AppendCsvRow(
        _sampleCounter,
        _elapsedSeconds,
        _connector.Status.ToString(),
        distance,
        deltaPosition,
        approximateSpeed,
        currentPosition);
}

void AppendCsvRow(
    int sample,
    double timeSeconds,
    string status,
    double distance,
    double deltaPosition,
    double approximateSpeed,
    Vector3D position)
{
    _csv.Append(sample);
    _csv.Append(",");

    _csv.Append(timeSeconds.ToString("0.000"));
    _csv.Append(",");

    _csv.Append(status);
    _csv.Append(",");

    _csv.Append(distance.ToString("0.000000"));
    _csv.Append(",");

    _csv.Append(deltaPosition.ToString("0.000000"));
    _csv.Append(",");

    _csv.Append(approximateSpeed.ToString("0.000000"));
    _csv.Append(",");

    _csv.Append(position.X.ToString("0.000000"));
    _csv.Append(",");

    _csv.Append(position.Y.ToString("0.000000"));
    _csv.Append(",");

    _csv.Append(position.Z.ToString("0.000000"));

    _csv.AppendLine();
}

void DisplayStatus()
{
    Echo("=== Investigation 009-002 ===");
    Echo("Connector Telemetry Probe - CSV");
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
        Echo(
            "Distance: " +
            distance.ToString("0.000000") +
            " m");
    }
    else
    {
        Echo("Target: NOT CALIBRATED");
    }

    Echo("");
    Echo(
        "Logging: " +
        (_logging ? "ACTIVE" : "STOPPED"));

    Echo("Samples: " + _sampleCounter);

    if (_logging)
    {
        Echo("");
        Echo("Run 'stop' to write CSV");
        Echo("to this PB's Custom Data.");
    }
    else if (_sampleCounter > 0)
    {
        Echo("");
        Echo("CSV rows in memory: " +
             _sampleCounter);

        Echo("Run 'stop' after an active");
        Echo("run to publish Custom Data.");
    }
}

string VectorText(Vector3D value)
{
    return
        "X=" + value.X.ToString("0.000") +
        " Y=" + value.Y.ToString("0.000") +
        " Z=" + value.Z.ToString("0.000");
}