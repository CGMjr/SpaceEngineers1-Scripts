/*
 * StationCargoController v1.0.1
 *
 * Space Engineers Version 1
 * In-Game Programmable Block Edition
 *
 * Purpose:
 * Automate loading and unloading stations for detachable
 * gooseEgg cargo containers.
 *
 * Design Highlights:
 * - Single managed connector.
 * - Exact connector name matching.
 * - Same-construct connector discovery.
 * - Load and unload modes.
 * - Volume-based fill percentage.
 * - Configurable disconnect delay.
 * - Startup recovery.
 * - WaitingForContainerRemoval latch state.
 */

enum StationState
{
    WaitingForContainer,
    Processing,
    DisconnectPending,
    WaitingForContainerRemoval,
    Error
}

StationState _state = StationState.WaitingForContainer;

IMyShipConnector _stationConnector;

string _mode = "Load";
string _connectorName = "Cnx";

double _threshold = 95.0;
double _disconnectDelaySeconds = 10.0;

double _disconnectTimerSeconds = 0.0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    LoadConfiguration();

    if (!TryFindManagedConnector())
    {
        _state = StationState.Error;
        return;
    }

    RecoverState();
}

public void Main(string argument, UpdateType updateSource)
{
    if (_state == StationState.Error)
    {
        Echo("ERROR");
        Echo("Check connector configuration.");
        return;
    }

    if (!TryFindManagedConnector())
    {
        _state = StationState.Error;
        Echo("Managed connector not found.");
        return;
    }

    Echo("=== StationCargoController ===");
    Echo("State: " + _state);
    Echo("Mode: " + _mode);
    Echo("Threshold: " + _threshold.ToString("F1") + "%");

    switch (_state)
    {
        case StationState.WaitingForContainer:
            ProcessWaitingForContainer();
            break;

        case StationState.Processing:
            ProcessProcessing();
            break;

        case StationState.DisconnectPending:
            ProcessDisconnectPending();
            break;

        case StationState.WaitingForContainerRemoval:
            ProcessWaitingForContainerRemoval();
            break;
    }
}

void ProcessWaitingForContainer()
{
    if (_stationConnector.Status != MyShipConnectorStatus.Connectable)
        return;

    Echo("Container detected.");
    Echo("Attempting connection...");

    _stationConnector.Connect();

    if (_stationConnector.Status ==
        MyShipConnectorStatus.Connected)
    {
        Echo("Connected.");
        _state = StationState.Processing;
    }
}

void ProcessProcessing()
{
    if (_stationConnector.Status !=
        MyShipConnectorStatus.Connected)
    {
        Echo("Connection lost.");

        _state = StationState.WaitingForContainer;
        return;
    }

    double fillPercent =
        GetConnectedGridFillPercentage();

    Echo("Fill: " + fillPercent.ToString("F2") + "%");

    bool complete = false;

    if (_mode.Equals("Load"))
    {
        complete = fillPercent >= _threshold;
    }
    else if (_mode.Equals("Unload"))
    {
        complete = fillPercent <= _threshold;
    }

    if (!complete)
        return;

    Echo("Threshold reached.");

    _disconnectTimerSeconds = 0.0;

    _state = StationState.DisconnectPending;
}

void ProcessDisconnectPending()
{
    _disconnectTimerSeconds +=
        Runtime.TimeSinceLastRun.TotalSeconds;

    double remaining =
        _disconnectDelaySeconds -
        _disconnectTimerSeconds;

    if (remaining < 0)
        remaining = 0;

    Echo("Disconnect in "
        + remaining.ToString("F1")
        + "s");

    if (_disconnectTimerSeconds <
        _disconnectDelaySeconds)
        return;

    Echo("Disconnecting...");

    _stationConnector.Disconnect();

    _state =
        StationState.WaitingForContainerRemoval;
}

void ProcessWaitingForContainerRemoval()
{
    Echo("Waiting for dock to clear.");

    /*
     * Critical Design Requirement:
     *
     * After processing a container,
     * do not reconnect until the dock
     * becomes completely clear.
     *
     * This prevents endless:
     *
     * Connect
     * -> Process
     * -> Disconnect
     * -> Connect
     * -> Process
     *
     * loops.
     */

    if (_stationConnector.Status ==
        MyShipConnectorStatus.Unconnected)
    {
        Echo("Dock clear.");

        _state =
            StationState.WaitingForContainer;
    }
}

bool TryFindManagedConnector()
{
    var connectors =
        new List<IMyShipConnector>();

    GridTerminalSystem.GetBlocksOfType(
        connectors,
        block =>
            block.IsSameConstructAs(Me)
            &&
            block.CustomName == _connectorName);

    if (connectors.Count == 1)
    {
        _stationConnector = connectors[0];
        return true;
    }

    if (connectors.Count == 0)
    {
        Echo("ERROR");
        Echo("Connector not found:");
        Echo(_connectorName);
    }
    else
    {
        Echo("ERROR");
        Echo("Multiple connectors named:");
        Echo(_connectorName);
    }

    return false;
}

void RecoverState()
{
    if (_stationConnector == null)
        return;

    switch (_stationConnector.Status)
    {
        case MyShipConnectorStatus.Connected:
            _state = StationState.Processing;
            break;

        case MyShipConnectorStatus.Connectable:
            _state = StationState.WaitingForContainer;
            break;

        default:
            _state = StationState.WaitingForContainer;
            break;
    }
}

double GetConnectedGridFillPercentage()
{
    if (_stationConnector == null)
        return 0.0;

    if (_stationConnector.OtherConnector == null)
        return 0.0;

    var connectedGrid =
        _stationConnector.OtherConnector.CubeGrid;

    var cargoContainers =
        new List<IMyCargoContainer>();

    GridTerminalSystem.GetBlocksOfType(
        cargoContainers,
        block => block.CubeGrid == connectedGrid);

    if (cargoContainers.Count == 0)
    {
        Echo("No cargo containers found.");
        return 0.0;
    }

    double currentVolume = 0.0;
    double maxVolume = 0.0;

    foreach (var container in cargoContainers)
    {
        var inventory =
            container.GetInventory();

        currentVolume +=
            (double)inventory.CurrentVolume;

        maxVolume +=
            (double)inventory.MaxVolume;
    }

    if (maxVolume <= 0.0)
        return 0.0;

    return
        (currentVolume / maxVolume) * 100.0;
}

void LoadConfiguration()
{
    string customData =
        Me.CustomData ?? "";

    string[] lines =
        customData.Split('\n');

    foreach (string rawLine in lines)
    {
        string line =
            rawLine.Trim();

        if (line.Length == 0)
            continue;

        if (line.StartsWith("["))
            continue;

        if (line.StartsWith("Mode="))
        {
            _mode =
                line.Substring(5).Trim();
        }
        else if (line.StartsWith("Threshold="))
        {
            double.TryParse(
                line.Substring(10),
                out _threshold);
        }
        else if (line.StartsWith(
            "DisconnectDelaySeconds="))
        {
            double.TryParse(
                line.Substring(23),
                out _disconnectDelaySeconds);
        }
        else if (line.StartsWith(
            "ConnectorName="))
        {
            _connectorName =
                line.Substring(14).Trim();
        }
    }

    if (_threshold < 0)
        _threshold = 0;

    if (_threshold > 100)
        _threshold = 100;

    if (_disconnectDelaySeconds < 0)
        _disconnectDelaySeconds = 0;
}
