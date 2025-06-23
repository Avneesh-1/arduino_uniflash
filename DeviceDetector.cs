using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace UniFlash
{
    public class DetectedDevice
    {
        public string PortName { get; set; }
        public string DeviceType { get; set; }
        public string SerialMessage { get; set; }
        public override string ToString() => string.IsNullOrWhiteSpace(SerialMessage)
            ? $"{DeviceType} ({PortName})"
            : $"{DeviceType} ({PortName}): {SerialMessage.Trim()}";
    }

    public static class DeviceDetector
    {
        private static readonly Dictionary<string, string> KnownChips = new()
        {
            { "CH340", "CH340-based Arduino (Uno/Nano/Clone)" },
            { "CP2102", "CP2102 USB to UART (common on dev boards)" },
            // Add more as needed
        };

        public static List<DetectedDevice> GetConnectedDevices()
        {
            var devices = new List<DetectedDevice>();
            foreach (var port in SerialPort.GetPortNames())
            {
                string deviceType = "Unknown device";
                string serialMsg = null;
                // Try to open the port and read a message
                try
                {
                    using (var sp = new SerialPort(port, 9600))
                    {
                        sp.ReadTimeout = 500;
                        sp.Open();
                        Thread.Sleep(1000); // Wait for device to send boot message
                        if (sp.BytesToRead > 0)
                        {
                            serialMsg = sp.ReadExisting();
                        }
                        sp.Close();
                    }
                }
                catch { }
                // Try to identify by port name (CH340, CP2102, etc.)
                if (port.ToUpper().Contains("CH340"))
                    deviceType = KnownChips["CH340"];
                else if (port.ToUpper().Contains("CP2102"))
                    deviceType = KnownChips["CP2102"];
                devices.Add(new DetectedDevice { PortName = port, DeviceType = deviceType, SerialMessage = serialMsg });
            }
            return devices;
        }
    }
} 