/*
 * DockHand Investigation Probe — Investigation 010-002
 * Temporary investigative code only. Not DockHand product code.
 *
 * Interrogates the connected Managed Grid using:
 *   1. IMyBatteryBlock
 *   2. IMyPowerProducer
 *   3. BlockDefinition
 *
 * Put this script in the station PB, connect the investigated grid,
 * and run once. Change CONNECTOR_NAME if necessary.
 */

const string CONNECTOR_NAME = "Cnx";

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.None;
}

public void Main(string argument, UpdateType updateSource)
{
    Echo("=== Investigation 010-002 ===");
    Echo("Power-Storage Classification Probe");
    Echo("");

    IMyShipConnector stationConnector = FindStationConnector();
    if (stationConnector == null)
        return;

    if (stationConnector.Status != MyShipConnectorStatus.Connected ||
        stationConnector.OtherConnector == null)
    {
        Echo("RESULT: No connected Managed Grid.");
        return;
    }

    var managedGrid = stationConnector.OtherConnector.CubeGrid;
    var blocks = new List<IMyTerminalBlock>();

    GridTerminalSystem.GetBlocksOfType(
        blocks,
        block => block.CubeGrid == managedGrid);

    int batteryCount = 0;
    int producerCount = 0;
    int producerNotBatteryCount = 0;

    Echo("Station connector: " + stationConnector.CustomName);
    Echo("Managed grid EntityId: " + managedGrid.EntityId);
    Echo("Terminal blocks: " + blocks.Count);
    Echo("");

    foreach (var block in blocks)
    {
        bool isBattery = block is IMyBatteryBlock;
        bool isProducer = block is IMyPowerProducer;

        if (isBattery) batteryCount++;
        if (isProducer) producerCount++;
        if (isProducer && !isBattery) producerNotBatteryCount++;

        Echo("BLOCK");
        Echo("Name: " + block.CustomName);
        Echo("Runtime type: " + block.GetType().Name);
        Echo("BlockDefinition: " + block.BlockDefinition.ToString());
        Echo("Definition name: " + block.DefinitionDisplayNameText);
        Echo("IMyBatteryBlock: " + YesNo(isBattery));
        Echo("IMyPowerProducer: " + YesNo(isProducer));

        if (isBattery)
        {
            var battery = block as IMyBatteryBlock;
            Echo("CurrentStoredPower: "
                + battery.CurrentStoredPower.ToString("F6") + " MWh");
            Echo("MaxStoredPower: "
                + battery.MaxStoredPower.ToString("F6") + " MWh");

            double pct = 0.0;
            if (battery.MaxStoredPower > 0.0)
                pct = ((double)battery.CurrentStoredPower /
                       (double)battery.MaxStoredPower) * 100.0;

            Echo("StoredPowerPercent: " + pct.ToString("F2") + "%");
            Echo("ChargeMode: " + battery.ChargeMode.ToString());
        }

        if (isProducer)
        {
            var producer = block as IMyPowerProducer;
            Echo("CurrentOutput: "
                + producer.CurrentOutput.ToString("F6") + " MW");
            Echo("MaxOutput: "
                + producer.MaxOutput.ToString("F6") + " MW");
        }

        Echo("--------------------------------");
    }

    Echo("");
    Echo("SUMMARY");
    Echo("Terminal blocks: " + blocks.Count);
    Echo("IMyBatteryBlock: " + batteryCount);
    Echo("IMyPowerProducer: " + producerCount);
    Echo("Producer but NOT battery: " + producerNotBatteryCount);

    if (batteryCount == 0)
        Echo("Battery classification: ZERO");
    else
        Echo("Battery classification: " + batteryCount + " found");

    if (producerNotBatteryCount > 0)
        Echo("IMyPowerProducer includes non-battery producers.");
}

IMyShipConnector FindStationConnector()
{
    var connectors = new List<IMyShipConnector>();

    GridTerminalSystem.GetBlocksOfType(
        connectors,
        connector =>
            connector.IsSameConstructAs(Me)
            && connector.CustomName == CONNECTOR_NAME);

    if (connectors.Count == 1)
        return connectors[0];

    if (connectors.Count == 0)
    {
        Echo("ERROR: Station connector not found.");
        Echo("Expected exact name: " + CONNECTOR_NAME);
    }
    else
    {
        Echo("ERROR: Multiple matching station connectors.");
    }

    return null;
}

string YesNo(bool value)
{
    return value ? "YES" : "NO";
}
