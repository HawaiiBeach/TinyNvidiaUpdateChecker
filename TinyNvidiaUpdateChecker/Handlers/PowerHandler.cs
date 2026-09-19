using System;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Handlers
{
    internal static class PowerHandler
    {
        public static bool IsRunningOnBattery(out int batteryPercent)
        {
            PowerStatus status = SystemInformation.PowerStatus;
            batteryPercent = -1;
            if (status.BatteryChargeStatus != BatteryChargeStatus.Unknown &&
                (status.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) != 0)
            {
                return false;
            }

            float remaining = status.BatteryLifePercent;
            if (remaining >= 0 && remaining <= 1)
                batteryPercent = (int)Math.Round(remaining * 100);
            return status.PowerLineStatus == PowerLineStatus.Offline;
        }

        public static bool ConfirmHeavyOperation(string operation)
        {
            if (!IsRunningOnBattery(out int batteryPercent))
            {
                return true;
            }

            string remaining = batteryPercent >= 0 ? $"{batteryPercent}% remaining" : "charge unknown";
            string message = $"This computer is running on battery ({remaining}). " +
                             $"For safety, connect AC power before {operation}. Continue anyway?";

            if (MainConsole.confirmDL)
            {
                MainConsole.WriteLine($"WARNING: {message}");
                return batteryPercent > 15;
            }

            if (!MainConsole.showUI || batteryPercent <= 15)
            {
                MainConsole.WriteLine($"WARNING: {message}");
                return false;
            }

            return MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
    }
}