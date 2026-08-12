/*
 * DockHand v1.0.9-O004
 *
 * Space Engineers Version 1
 * In-Game Programmable Block Edition
 *
 * Purpose:
 * Automate loading and unloading stations for detachable
 * gooseEgg cargo containers.
 *
 * O-004 investigation implementation:
 * - A Managed Grid may declare PowerThreshold in the
 *   participating connector's [StationCargoController] Custom Data.
 * - Declared power thresholds are limited to 1%-99%.
 * - If no power threshold is declared, the station default is 99%.
 * - Aggregate battery stored power / aggregate battery capacity
 *   determines power readiness.
 * - A grid with no applicable batteries satisfies the power condition.
 * - Power readiness is evaluated before release and again during
 *   the disconnect-delay period.
 * - Existing cargo-fill behavior remains unchanged for this
 *   investigation.
 *
 * D-shape #5 / Connect Wait:
 * - If a pending Connectable wait is abandoned, all accumulated wait
 *   time is discarded.
 * - A new full ConnectWaitSeconds interval begins when the connector
 *   becomes Connectable again.
 *
 * Testability:
 * - While a Managed Grid is connected and being evaluated, DockHand
 *   passively displays the effective power requirement, applicable
 *   current power state, and readiness result through Echo().
 * - The displayed power information is produced from the same
 *   PowerStatus used for the release decision.
 */

enum StationState
{
    WaitingForContainer,
    Processing,
    WaitingForPower,
    DisconnectPending,
    WaitingForContainerRemoval,
    ReportAndWait,
    Error
}

StationState _state = StationState.WaitingForContainer;

IMyShipConnector _stationConnector;

string _mode = "Load";
string _connectorName = "Cnx";

double _threshold = 95.0;
double _disconnectDelaySeconds = 10.0;
double _connectWaitSeconds = 1.0;

const double DefaultPowerThreshold = 99.0;
const double MinimumPowerThreshold = 1.0;
const double MaximumPowerThreshold = 99.0;

double _disconnectTimerSeconds = 0.0;
double _connectWaitTimerSeconds = 0.0;
bool _connectWaitPending = false;

double _finalFilledPercent = 0.0;

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

    Echo("=== DockHand ===");
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

        case StationState.WaitingForPower:
            ProcessWaitingForPower();
            break;

        case StationState.DisconnectPending:
            ProcessDisconnectPending();
            break;

        case StationState.WaitingForContainerRemoval:
            ProcessWaitingForContainerRemoval();
            break;

        case StationState.ReportAndWait:
            ProcessReportAndWait();
            break;
    }
}

void ProcessWaitingForContainer()
{
    if (_stationConnector.Status != MyShipConnectorStatus.Connectable)
    {
        AbandonConnectWait();
        return;
    }

    if (!_connectWaitPending)
    {
        _connectWaitPending = true;
        _connectWaitTimerSeconds = 0.0;

        Echo("Container detected.");
        Echo("Waiting before connection...");
        return;
    }

    _connectWaitTimerSeconds +=
        Runtime.TimeSinceLastRun.TotalSeconds;

    double remaining =
        _connectWaitSeconds -
        _connectWaitTimerSeconds;

    if (remaining < 0)
        remaining = 0;

    Echo("Connect in "
        + remaining.ToString("F1")
        + "s");

    if (_connectWaitTimerSeconds <
        _connectWaitSeconds)
        return;

    Echo("Attempting connection...");

    _stationConnector.Connect();

    if (_stationConnector.Status ==
        MyShipConnectorStatus.Connected)
    {
        Echo("Connected.");

        AbandonConnectWait();

        if (IsParticipatingConnector(_stationConnector.OtherConnector))
        {
            _state = StationState.Processing;
        }
        else
        {
            Echo("Connector not participating.");
            _state = StationState.ReportAndWait;
        }
    }
}

void AbandonConnectWait()
{
    /*
     * D-shape #5:
     *
     * Abandoning a pending wait discards all accumulated time.
     * A later transition back to Connectable therefore starts a
     * completely new ConnectWaitSeconds interval.
     */
    _connectWaitPending = false;
    _connectWaitTimerSeconds = 0.0;
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

    /*
     * PowerStatus is evaluated before the cargo condition is tested.
     * This makes the effective requirement and current power state
     * continuously observable while the grid is being serviced,
     * rather than only after cargo becomes ready.
     */
    PowerStatus power = GetConnectedGridPowerStatus();

    EchoPowerStatus(power);

    double fillPercent =
        GetConnectedGridFillPercentage();

    Echo("Fill: " + fillPercent.ToString("F2") + "%");

    bool cargoReady = false;

    if (_mode.Equals("Load"))
    {
        cargoReady = fillPercent >= _threshold;
    }
    else if (_mode.Equals("Unload"))
    {
        cargoReady = fillPercent <= _threshold;
    }

    if (!cargoReady)
        return;

    Echo("Cargo requirement satisfied.");

    /*
     * Re-evaluate immediately before the release decision.
     * This keeps the decision tied to the same current-state data
     * that is displayed to the investigator.
     */
    power = GetConnectedGridPowerStatus();

    EchoPowerStatus(power);

    if (!power.IsReady)
    {
        Echo("Power requirement not satisfied.");
        Echo("Waiting for power...");

        _finalFilledPercent = fillPercent;
        _state = StationState.WaitingForPower;
        return;
    }

    Echo("Power requirement satisfied.");

    BeginDisconnectPending(fillPercent);
}

void ProcessWaitingForPower()
{
    if (_stationConnector.Status !=
        MyShipConnectorStatus.Connected)
    {
        Echo("Connection lost.");

        _state = StationState.WaitingForContainer;
        return;
    }

    PowerStatus power = GetConnectedGridPowerStatus();

    EchoPowerStatus(power);

    if (!power.IsReady)
    {
        Echo("Waiting for power...");
        return;
    }

    Echo("Power requirement satisfied.");

    BeginDisconnectPending(_finalFilledPercent);
}

void BeginDisconnectPending(double fillPercent)
{
    _disconnectTimerSeconds = 0.0;
    _finalFilledPercent = fillPercent;
    _state = StationState.DisconnectPending;
}

void ProcessDisconnectPending()
{
    if (_stationConnector.Status !=
        MyShipConnectorStatus.Connected)
    {
        Echo("Connection lost.");

        _state = StationState.WaitingForContainer;
        return;
    }

    PowerStatus power = GetConnectedGridPowerStatus();

    EchoPowerStatus(power);

    if (!power.IsReady)
    {
        Echo("Power requirement no longer satisfied.");
        Echo("Returning to WaitingForPower.");

        _state = StationState.WaitingForPower;
        return;
    }

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

void ProcessReportAndWait()
{
    Echo("Connected connector is not managed.");
    Echo("Waiting for dock to clear.");

    if (_stationConnector.Status == MyShipConnectorStatus.Unconnected)
    {
        _state = StationState.WaitingForContainer;
    }
}

bool IsParticipatingConnector(IMyShipConnector connector)
{
    if (connector == null)
        return false;

    string data = connector.CustomData ?? "";

    bool inSection = false;

    foreach (string raw in data.Split('\n'))
    {
        string line = raw.Trim();

        if (line.StartsWith("["))
        {
            inSection = line.Equals("[StationCargoController]");
            continue;
        }

        if (!inSection)
            continue;

        if (line.StartsWith("Managed="))
        {
            bool managed;
            if (bool.TryParse(line.Substring(8).Trim(), out managed))
                return managed;

            return false;
        }
    }

    return false;
}

void ProcessWaitingForContainerRemoval()
{
    Echo("Final fill%: " +
        _finalFilledPercent.ToString("F1") + "%");
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
        var inventory = container.GetInventory();

        currentVolume +=
            (double)inventory.CurrentVolume;

        maxVolume +=
            (double)inventory.MaxVolume;
    }

    var connectorInventory =
        _stationConnector.OtherConnector.GetInventory();

    currentVolume +=
        (double)connectorInventory.CurrentVolume;

    maxVolume +=
        (double)connectorInventory.MaxVolume;

    if (maxVolume <= 0.0)
        return 0.0;

    return
        (currentVolume / maxVolume) * 100.0;
}

struct PowerStatus
{
    public bool HasApplicableStorage;
    public bool HasDeclaredThreshold;
    public double ThresholdPercent;
    public double StoredPower;
    public double MaximumPower;
    public double FillPercent;
    public bool IsReady;
}

PowerStatus GetConnectedGridPowerStatus()
{
    PowerStatus status = new PowerStatus();

    status.ThresholdPercent =
        GetConnectedGridPowerThreshold(
            out status.HasDeclaredThreshold);

    if (_stationConnector == null ||
        _stationConnector.OtherConnector == null)
    {
        status.IsReady = true;
        return status;
    }

    var connectedGrid =
        _stationConnector.OtherConnector.CubeGrid;

    var batteries =
        new List<IMyBatteryBlock>();

    GridTerminalSystem.GetBlocksOfType(
        batteries,
        block => block.CubeGrid == connectedGrid);

    double storedPower = 0.0;
    double maximumPower = 0.0;

    foreach (var battery in batteries)
    {
        if (battery.MaxStoredPower <= 0.0)
            continue;

        storedPower +=
            (double)battery.CurrentStoredPower;

        maximumPower +=
            (double)battery.MaxStoredPower;
    }

    status.StoredPower = storedPower;
    status.MaximumPower = maximumPower;

    if (maximumPower <= 0.0)
    {
        /*
         * No applicable battery storage means there is no
         * power-readiness condition to satisfy.
         */
        status.HasApplicableStorage = false;
        status.FillPercent = 100.0;
        status.IsReady = true;
        return status;
    }

    status.HasApplicableStorage = true;

    status.FillPercent =
        (storedPower / maximumPower) * 100.0;

    if (status.FillPercent > 100.0)
        status.FillPercent = 100.0;

    if (status.FillPercent < 0.0)
        status.FillPercent = 0.0;

    status.IsReady =
        status.FillPercent >=
        status.ThresholdPercent;

    return status;
}

double GetConnectedGridPowerThreshold(
    out bool declared)
{
    declared = false;

    if (_stationConnector == null ||
        _stationConnector.OtherConnector == null)
        return DefaultPowerThreshold;

    string data =
        _stationConnector.OtherConnector.CustomData ?? "";

    bool inSection = false;

    foreach (string raw in data.Split('\n'))
    {
        string line = raw.Trim();

        if (line.StartsWith("["))
        {
            inSection =
                line.Equals("[StationCargoController]");
            continue;
        }

        if (!inSection)
            continue;

        if (!line.StartsWith("PowerThreshold="))
            continue;

        double threshold;

        if (!double.TryParse(
            line.Substring(15).Trim(),
            out threshold))
        {
            return DefaultPowerThreshold;
        }

        if (threshold < MinimumPowerThreshold)
            threshold = MinimumPowerThreshold;

        if (threshold > MaximumPowerThreshold)
            threshold = MaximumPowerThreshold;

        declared = true;
        return threshold;
    }

    return DefaultPowerThreshold;
}

void EchoPowerStatus(PowerStatus status)
{
    Echo("=== Power Readiness ===");

    if (!status.HasApplicableStorage)
    {
        Echo("Power storage: none");
        Echo("Power state: N/A");
        Echo("Power requirement: " +
            status.ThresholdPercent.ToString("F1") +
            "% (" +
            (status.HasDeclaredThreshold ? "declared" : "default") +
            ")");
        Echo("Power readiness: SATISFIED");
        return;
    }

    string source =
        status.HasDeclaredThreshold
            ? "declared"
            : "default";

    Echo("Power state: " +
        status.FillPercent.ToString("F2") + "%");

    Echo("Power requirement: " +
        status.ThresholdPercent.ToString("F1") +
        "% (" + source + ")");

    Echo("Power readiness: " +
        (status.IsReady ? "SATISFIED" : "NOT SATISFIED"));
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
            "ConnectWaitSeconds="))
        {
            double.TryParse(
                line.Substring(19),
                out _connectWaitSeconds);
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

    if (_connectWaitSeconds < 0)
        _connectWaitSeconds = 0;
}
