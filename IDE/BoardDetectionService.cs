using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniFlash.IDE
{
    public class ConnectedBoard
    {
        public string PortName { get; set; }
        public string BoardType { get; set; }
        public string BoardName { get; set; }
        public string Vendor { get; set; }
        public string Product { get; set; }
        public bool IsCompatible { get; set; }
        public string CompatibilityMessage { get; set; }
    }

    public class BoardDetectionService
    {
        private string arduinoCliPath;

        public BoardDetectionService()
        {
            FindArduinoCliPath();
        }

        private void FindArduinoCliPath()
        {
            // First try to find arduino-cli in PATH
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "arduino-cli",
                        Arguments = "version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    arduinoCliPath = "arduino-cli";
                    return;
                }
            }
            catch { }

            // If not found in PATH, try common installation locations
            string[] possiblePaths = new string[]
            {
                @"C:\Program Files (x86)\Arduino CLI\arduino-cli.exe",
                @"C:\Program Files\Arduino CLI\arduino-cli.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Arduino CLI\arduino-cli.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Arduino CLI\arduino-cli.exe"
            };

            arduinoCliPath = possiblePaths.FirstOrDefault(path => File.Exists(path));
        }

        public async Task<List<ConnectedBoard>> DetectConnectedBoardsAsync()
        {
            var connectedBoards = new List<ConnectedBoard>();
            
            try
            {
                // Get list of available COM ports
                string[] availablePorts = SerialPort.GetPortNames();
                
                foreach (string port in availablePorts)
                {
                    var board = await DetectBoardOnPortAsync(port);
                    if (board != null)
                    {
                        connectedBoards.Add(board);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail completely
                Console.WriteLine($"Error detecting boards: {ex.Message}");
            }

            return connectedBoards;
        }

        private async Task<ConnectedBoard> DetectBoardOnPortAsync(string portName)
        {
            try
            {
                // Use arduino-cli to detect board on this port
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = arduinoCliPath,
                        Arguments = $"board list --format json",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    return ParseBoardInfo(output, portName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting board on {portName}: {ex.Message}");
            }

            // Fallback: try to detect by port name patterns and USB device info
            return DetectBoardByPortName(portName);
        }

        private ConnectedBoard ParseBoardInfo(string jsonOutput, string portName)
        {
            try
            {
                // Look for the specific port in the JSON output
                var board = new ConnectedBoard { PortName = portName };

                // Split JSON into lines to find the entry for our port
                var lines = jsonOutput.Split('\n');
                bool foundPort = false;
                
                foreach (var line in lines)
                {
                    if (line.Contains($"\"port\": \"{portName}\""))
                    {
                        foundPort = true;
                        continue;
                    }
                    
                    if (foundPort)
                    {
                        // Extract board type
                        if (line.Contains("\"board\":"))
                        {
                            var boardMatch = Regex.Match(line, @"""board"":\s*""([^""]+)""");
                            if (boardMatch.Success)
                            {
                                board.BoardType = boardMatch.Groups[1].Value;
                                board.BoardName = ExtractBoardName(board.BoardType);
                            }
                        }
                        
                        // Extract vendor
                        if (line.Contains("\"vendor\":"))
                        {
                            var vendorMatch = Regex.Match(line, @"""vendor"":\s*""([^""]+)""");
                            if (vendorMatch.Success)
                            {
                                board.Vendor = vendorMatch.Groups[1].Value;
                            }
                        }
                        
                        // Extract product
                        if (line.Contains("\"product\":"))
                        {
                            var productMatch = Regex.Match(line, @"""product"":\s*""([^""]+)""");
                            if (productMatch.Success)
                            {
                                board.Product = productMatch.Groups[1].Value;
                            }
                        }
                        
                        // If we've found all the info we need, break
                        if (!string.IsNullOrEmpty(board.BoardType) && 
                            !string.IsNullOrEmpty(board.Vendor) && 
                            !string.IsNullOrEmpty(board.Product))
                        {
                            break;
                        }
                    }
                }

                // If we didn't find the board type, try alternative parsing
                if (string.IsNullOrEmpty(board.BoardType))
                {
                    board = DetectBoardByPortName(portName);
                }
                else
                {
                    // Ensure all properties are at least initialized to something
                    board.BoardName ??= "Unknown Board";
                    board.Vendor ??= "Unknown Vendor";
                    board.Product ??= "Unknown Product";
                }

                return board;
            }
            catch
            {
                return DetectBoardByPortName(portName);
            }
        }

        private ConnectedBoard DetectBoardByPortName(string portName)
        {
            var board = new ConnectedBoard 
            { 
                PortName = portName,
                BoardName = "Unknown Board",
                BoardType = "unknown",
                Vendor = "Unknown Vendor",
                Product = "Unknown Product"
            };

            try
            {
                var deviceInfo = GetUSBDeviceInfo(portName);
                
                if (string.IsNullOrEmpty(deviceInfo)) return board;

                Console.WriteLine($"Device info for {portName}: {deviceInfo}");
                    
                if (deviceInfo.Contains("CP210x")) // Silicon Labs USB to UART bridge
                {
                    board.BoardName = "ESP32 (CP210x)";
                    board.BoardType = "esp32:esp32:esp32";
                    board.Vendor = "Silicon Labs";
                    board.Product = "CP210x";
                }
                else if (deviceInfo.Contains("CH340")) // Common on many Arduino clones and ESP boards
                {
                    board.BoardName = "Generic Board (CH340)";
                    board.BoardType = "arduino:avr:uno"; // A safe default
                    board.Vendor = "WCH";
                    board.Product = "CH340";
                }
                else if (deviceInfo.Contains("Arduino"))
                {
                    board.BoardName = "Arduino";
                    board.BoardType = "arduino:avr:uno";
                    board.Vendor = "Arduino";
                    if (deviceInfo.Contains("Uno"))
                    {
                        board.BoardName = "Arduino Uno";
                        board.Product = "Uno";
                    }
                    else if (deviceInfo.Contains("Mega"))
                    {
                        board.BoardName = "Arduino Mega";
                        board.BoardType = "arduino:avr:mega";
                        board.Product = "Mega";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectBoardByPortName: {ex.Message}");
            }

            return board;
        }

        private string GetUSBDeviceInfo(string portName)
        {
            try
            {
                // Try to get device information from Windows registry
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c wmic path Win32_SerialPort where \"DeviceID='{portName}'\" get Caption,Description,Manufacturer /format:csv",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"WMI output for {portName}: {output}");

                if (!string.IsNullOrEmpty(output))
                {
                    // Parse the CSV output to get device info
                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains(portName) && !line.Contains("Caption"))
                        {
                            return line;
                        }
                    }
                }

                // Try alternative method using PowerShell
                var psProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"Get-WmiObject -Class Win32_SerialPort | Where-Object {{$_.DeviceID -eq '{portName}'}} | Select-Object Caption,Description,Manufacturer | ConvertTo-Csv\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                psProcess.Start();
                string psOutput = psProcess.StandardOutput.ReadToEnd();
                psProcess.WaitForExit();

                Console.WriteLine($"PowerShell output for {portName}: {psOutput}");

                if (!string.IsNullOrEmpty(psOutput))
                {
                    var psLines = psOutput.Split('\n');
                    foreach (var line in psLines)
                    {
                        if (line.Contains(portName) && !line.Contains("Caption"))
                        {
                            return line;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting USB device info: {ex.Message}");
            }

            return string.Empty;
        }

        private string ExtractBoardName(string boardType)
        {
            if (string.IsNullOrEmpty(boardType))
                return "Unknown Board";

            // Extract friendly name from board type
            var parts = boardType.Split(':');
            if (parts.Length >= 3)
            {
                switch (parts[1])
                {
                    case "avr":
                        return $"Arduino {parts[2].ToUpper()}";
                    case "megaavr":
                        return $"ATmega{parts[2]}";
                    case "esp32":
                        return $"ESP32 {parts[2]}";
                    case "esp8266":
                        return $"ESP8266 {parts[2]}";
                    case "sam":
                        return $"Arduino {parts[2].ToUpper()}";
                    case "samd":
                        return $"Arduino {parts[2].ToUpper()}";
                    default:
                        return $"{parts[1].ToUpper()} {parts[2].ToUpper()}";
                }
            }

            return boardType;
        }

        public bool ValidateBoardCompatibility(string selectedBoardType, string detectedBoardType)
        {
            if (string.IsNullOrEmpty(detectedBoardType) || detectedBoardType == "unknown")
                return true; // Allow unknown boards with warning

            // Define strict compatibility rules
            var compatibilityRules = new Dictionary<string, string[]>
            {
                // Arduino IDE compatible boards - only allow same family
                ["arduino:avr:uno"] = new[] { "arduino:avr:uno", "arduino:avr:nano", "arduino:avr:mega", "arduino:avr:leonardo" },
                ["arduino:avr:nano"] = new[] { "arduino:avr:nano", "arduino:avr:uno" },
                ["arduino:avr:mega"] = new[] { "arduino:avr:mega", "arduino:avr:uno" },
                ["arduino:avr:leonardo"] = new[] { "arduino:avr:leonardo", "arduino:avr:uno" },
                
                // MegaCoreX compatible boards - only allow same family
                ["MegaCoreX:megaavr:4809"] = new[] { "MegaCoreX:megaavr:4809", "MegaCoreX:megaavr:4808", "MegaCoreX:megaavr:3208" },
                ["MegaCoreX:megaavr:4808"] = new[] { "MegaCoreX:megaavr:4808", "MegaCoreX:megaavr:4809" },
                ["MegaCoreX:megaavr:3208"] = new[] { "MegaCoreX:megaavr:3208", "MegaCoreX:megaavr:4809" },
                
                // ESP32 boards - only allow same family
                ["esp32:esp32:esp32"] = new[] { "esp32:esp32:esp32", "esp32:esp32:esp32s3", "esp32:esp32:esp32c3" },
                ["esp32:esp32:esp32s3"] = new[] { "esp32:esp32:esp32s3", "esp32:esp32:esp32" },
                ["esp32:esp32:esp32c3"] = new[] { "esp32:esp32:esp32c3", "esp32:esp32:esp32" },
                
                // ESP8266 boards - only allow same family
                ["esp8266:esp8266:nodemcuv2"] = new[] { "esp8266:esp8266:nodemcuv2", "esp8266:esp8266:esp01" },
                ["esp8266:esp8266:esp01"] = new[] { "esp8266:esp8266:esp01", "esp8266:esp8266:nodemcuv2" }
            };

            // Check for exact compatibility first
            if (compatibilityRules.TryGetValue(selectedBoardType, out var compatibleBoards))
            {
                return compatibleBoards.Contains(detectedBoardType);
            }

            // If no specific rule, check if they're from the same family
            return AreBoardsFromSameFamily(selectedBoardType, detectedBoardType);
        }

        private bool AreBoardsFromSameFamily(string board1, string board2)
        {
            if (string.IsNullOrEmpty(board1) || string.IsNullOrEmpty(board2))
                return false;

            var parts1 = board1.Split(':');
            var parts2 = board2.Split(':');

            if (parts1.Length >= 2 && parts2.Length >= 2)
            {
                // Check if they're from the same manufacturer/family
                return parts1[0] == parts2[0] && parts1[1] == parts2[1];
            }

            return false;
        }

        public string GetCompatibilityMessage(string selectedBoardType, ConnectedBoard detectedBoard)
        {
            if (detectedBoard == null)
                return "No board detected on this port.";

            if (ValidateBoardCompatibility(selectedBoardType, detectedBoard.BoardType))
            {
                return $"✓ Compatible: {detectedBoard.BoardName} detected on {detectedBoard.PortName}";
            }
            else
            {
                // Provide specific guidance based on the mismatch
                var guidance = GetSpecificGuidance(selectedBoardType, detectedBoard.BoardType);
                return $"⚠ INCOMPATIBLE: {detectedBoard.BoardName} detected on {detectedBoard.PortName}. " +
                       $"Selected board type '{selectedBoardType}' is NOT suitable for this hardware. " +
                       guidance;
            }
        }

        private string GetSpecificGuidance(string selectedBoardType, string detectedBoardType)
        {
            // Provide specific guidance for common mismatches
            if (selectedBoardType.StartsWith("arduino:avr:") && detectedBoardType.StartsWith("esp32:"))
            {
                return "You cannot upload Arduino code to ESP32. ESP32 requires ESP32-specific code and libraries. " +
                       "Please select 'ESP32 Dev Module' as your board type.";
            }
            
            if (selectedBoardType.StartsWith("arduino:avr:") && detectedBoardType.StartsWith("esp8266:"))
            {
                return "You cannot upload Arduino code to ESP8266. ESP8266 requires ESP8266-specific code and libraries. " +
                       "Please select 'NodeMCU 1.0' or appropriate ESP8266 board type.";
            }
            
            if (selectedBoardType.StartsWith("esp32:") && detectedBoardType.StartsWith("arduino:avr:"))
            {
                return "You cannot upload ESP32 code to Arduino. Arduino requires Arduino-specific code and libraries. " +
                       "Please select 'Arduino Uno' or appropriate Arduino board type.";
            }
            
            if (selectedBoardType.StartsWith("esp8266:") && detectedBoardType.StartsWith("arduino:avr:"))
            {
                return "You cannot upload ESP8266 code to Arduino. Arduino requires Arduino-specific code and libraries. " +
                       "Please select 'Arduino Uno' or appropriate Arduino board type.";
            }
            
            if (selectedBoardType.StartsWith("MegaCoreX:") && detectedBoardType.StartsWith("arduino:avr:"))
            {
                return "You cannot upload MegaCoreX code to Arduino. ATmega4809 requires MegaCoreX-specific code and libraries. " +
                       "Please select 'ATmega4809' as your board type.";
            }
            
            if (selectedBoardType.StartsWith("arduino:avr:") && detectedBoardType.StartsWith("MegaCoreX:"))
            {
                return "You cannot upload Arduino code to ATmega4809. ATmega4809 requires MegaCoreX-specific code and libraries. " +
                       "Please select 'ATmega4809' as your board type.";
            }

            return "Please select the correct board type that matches your connected hardware.";
        }

        public async Task<ConnectedBoard> GetBestCompatibleBoardAsync(string selectedBoardType)
        {
            var connectedBoards = await DetectConnectedBoardsAsync();
            
            // Find the most compatible board
            var compatibleBoards = connectedBoards
                .Where(b => ValidateBoardCompatibility(selectedBoardType, b.BoardType))
                .ToList();

            if (compatibleBoards.Any())
            {
                // Prefer exact matches, then same family
                var exactMatch = compatibleBoards.FirstOrDefault(b => b.BoardType == selectedBoardType);
                if (exactMatch != null)
                    return exactMatch;

                return compatibleBoards.First();
            }

            return null;
        }
    }
} 