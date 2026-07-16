/*
 * StationCargoController
 *
 * Version: 1.0
 *
 * Purpose:
 * Automatically manage loading and unloading stations for gooseEgg cargo
 * containers.
 *
 * Features:
 * - Auto-connect when connector becomes Connectable.
 * - Monitor fill percentage of connected gooseEgg.
 * - Load mode (disconnect when fill >= threshold).
 * - Unload mode (disconnect when fill <= threshold).
 * - Configurable disconnect delay.
 * - Prevent reconnect loops via state machine latch.
 * - Automatic recovery after world reload.
 */

using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

public sealed class Program : MyGridProgram
{
    private enum StationState
    {
        WaitingForContainer,
        Processing,
        DisconnectPending,
        WaitingForContainerRemoval,
        Error
    }

    private const string SectionName = "[Station]";

    private StationState _state = StationState.WaitingForContainer;

    private IMyShipConnector _stationConnector;

    private string _mode = "Load";
    private string _connectorName = "Cnx";
    private double _threshold = 95.0;
    private double _disconnectDelaySeconds = 10.0;

    private double _disconnectTimerSeconds;

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update100;

        LoadConfiguration();

        if (!TryFindConnector())
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
            return;
        }

        if (!TryFindConnector())
        {
            Echo("Managed connector not found.");
            _state = StationState.Error;
            return;
        }

        Echo($"State: {_state}");
        Echo($"Mode : {_mode}");
        Echo($"Threshold : {_threshold:F1}%");

        switch (_state)
        {
            case StationState.WaitingForContainer:
                ProcessWaitingForContainer();
                break;

            case StationState.Processing:
                ProcessConnectedContainer();
                break;

            case StationState.DisconnectPending:
                ProcessDisconnectPending();
                break;

            case StationState.WaitingForContainerRemoval:
                ProcessWaitingForRemoval();
                break;
        }
    }

    private void ProcessWaitingForContainer()
    {
        if (_stationConnector.Status == MyShipConnectorStatus.Connectable)
        {
            Echo("Connectable container detected.");

            _stationConnector.Connect();

            if (_stationConnector.Status == MyShipConnectorStatus.Connected)
            {
                Echo("Connected.");
                _state = StationState.Processing;
            }
        }
    }

    private void ProcessConnectedContainer()
    {
        if (_stationConnector.Status != MyShipConnectorStatus.Connected)
        {
            Echo("Connection lost.");
            _state = StationState.WaitingForContainer;
            return;
        }

        double fillPercent = GetConnectedGridFillPercentage();

        Echo($"Fill: {fillPercent:F2}%");

        bool complete =
            (_mode.Equals("Load", StringComparison.OrdinalIgnoreCase)
                && fillPercent >= _threshold)
            ||
            (_mode.Equals("Unload", StringComparison.OrdinalIgnoreCase)
                && fillPercent <= _threshold);

        if (!complete)
            return;

        Echo("Threshold reached.");

        _disconnectTimerSeconds = 0.0;
        _state = StationState.DisconnectPending;
    }

    private void ProcessDisconnectPending()
    {
        _disconnectTimerSeconds += Runtime.TimeSinceLastRun.TotalSeconds;

        Echo(
            $"Disconnect in {Math.Max(0.0, _disconnectDelaySeconds - _disconnectTimerSeconds):F1}s");

        if (_disconnectTimerSeconds < _disconnectDelaySeconds)
            return;

        _stationConnector.Disconnect();

        Echo("Disconnected.");

        _state = StationState.WaitingForContainerRemoval;
    }

    private void ProcessWaitingForRemoval()
    {
        Echo("Waiting for dock to clear.");

        /*
         * Design Decision:
         *
         * The connector must transition to Unconnected before
         * the station is allowed to process another container.
         *
         * This prevents reconnect loops when a completed
         * gooseEgg remains physically sitting on the dock.
         */
        if (_stationConnector.Status == MyShipConnectorStatus.Unconnected)
        {
            Echo("Dock clear.");

            _state = StationState.WaitingForContainer;
        }
    }

    private bool TryFindConnector()
    {
        _stationConnector =
            GridTerminalSystem.GetBlockWithName(_connectorName)
            as IMyShipConnector;

        return _stationConnector != null;
    }

    private void RecoverState()
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

    private double GetConnectedGridFillPercentage()
    {
        if (_stationConnector.OtherConnector == null)
            return 0.0;

        var connectedGrid = _stationConnector.OtherConnector.CubeGrid;

        var cargoContainers = new List<IMyCargoContainer>();

        GridTerminalSystem.GetBlocksOfType(
            cargoContainers,
            block => block.CubeGrid == connectedGrid);

        if (cargoContainers.Count == 0)
            return 0.0;

        double currentVolume = 0.0;
        double maxVolume = 0.0;

        foreach (var container in cargoContainers)
        {
            var inventory = container.GetInventory();

            currentVolume += (double)inventory.CurrentVolume;
            maxVolume += (double)inventory.MaxVolume;
        }

        if (maxVolume <= 0.0)
            return 0.0;

        return (currentVolume / maxVolume) * 100.0;
    }

    private void LoadConfiguration()
    {
        string customData = Me.CustomData ?? string.Empty;

        var lines = customData.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("Mode="))
            {
                _mode = line.Substring("Mode=".Length).Trim();
            }
            else if (line.StartsWith("Threshold="))
            {
                double.TryParse(
                    line.Substring("Threshold=".Length),
                    out _threshold);
            }
            else if (line.StartsWith("DisconnectDelaySeconds="))
            {
                double.TryParse(
                    line.Substring("DisconnectDelaySeconds=".Length),
                    out _disconnectDelaySeconds);
            }
            else if (line.StartsWith("ConnectorName="))
            {
                _connectorName =
                    line.Substring("ConnectorName=".Length).Trim();
            }
        }

        if (_threshold < 0)
            _threshold = 0;

        if (_threshold > 100)
            _threshold = 100;

        if (_disconnectDelaySeconds < 0)
            _disconnectDelaySeconds = 0;
    }
}
