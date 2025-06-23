using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace UniFlash.IDE
{
    public class ArduinoUploader
    {
        private string arduinoCliPath;
        private string sketchDirectory;
        public string CurrentBoardType { get; set; } = "arduino:avr:uno"; // Default to Arduino Uno
        private BoardDetectionService boardDetectionService;

        public ArduinoUploader()
        {
            FindArduinoCliPath();
            sketchDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IDE", "sketch");
            if (!Directory.Exists(sketchDirectory))
            {
                Directory.CreateDirectory(sketchDirectory);
            }
            boardDetectionService = new BoardDetectionService();
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
            if (arduinoCliPath == null)
            {
                throw new FileNotFoundException("Arduino CLI not found. Please install Arduino CLI and make sure it's in your PATH.");
            }
        }

        private bool IsPortAvailable(string portName)
        {
            try
            {
                using (var port = new SerialPort(portName))
                {
                    port.Open();
                    port.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task WaitForPortAvailability(string portName, IProgress<string> progress)
        {
            int attempts = 0;
            while (!IsPortAvailable(portName) && attempts < 5)
            {
                progress?.Report($"[Waiting for port {portName} to become available...]");
                await Task.Delay(1000);
                attempts++;
            }
        }

        private string FindHeadersPath()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            while (dir != null)
            {
                string candidate = Path.Combine(dir, "IDE", "headers.h");
                if (File.Exists(candidate))
                    return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return null;
        }

        public void SetBoardType(string deviceType)
        {
            if (deviceType.Contains("ATmega4809"))
            {
                CurrentBoardType = "MegaCoreX:megaavr:4809";
            }
            // --- ESP32 Support ---
            else if (deviceType.Contains("ESP32 WROOM") || deviceType.Contains("ESP32 Dev Module") || deviceType.Contains("ESP32"))
            {
                CurrentBoardType = "esp32:esp32:esp32";
            }
            // --- Add more ESP32 variants as needed ---
            else if (deviceType.Contains("Arduino Uno") || (deviceType.Contains("Arduino") && deviceType.Contains("Uno")))
            {
                CurrentBoardType = "arduino:avr:uno";
            }
            else if (deviceType.Contains("Arduino Nano") || (deviceType.Contains("Arduino") && deviceType.Contains("Nano")))
            {
                CurrentBoardType = "arduino:avr:nano";
            }
            else if (deviceType.Contains("Arduino Mega") || (deviceType.Contains("Arduino") && deviceType.Contains("Mega")))
            {
                CurrentBoardType = "arduino:avr:mega";
            }
            else if (deviceType.Contains("Arduino Leonardo") || (deviceType.Contains("Arduino") && deviceType.Contains("Leonardo")))
            {
                CurrentBoardType = "arduino:avr:leonardo";
            }
            else if (deviceType.Contains("Arduino Micro") || (deviceType.Contains("Arduino") && deviceType.Contains("Micro")))
            {
                CurrentBoardType = "arduino:avr:micro";
            }
            else if (deviceType.Contains("Arduino Pro") || (deviceType.Contains("Arduino") && deviceType.Contains("Pro")))
            {
                CurrentBoardType = "arduino:avr:pro";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("BT"))
            {
                CurrentBoardType = "arduino:avr:bt";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Esplora"))
            {
                CurrentBoardType = "arduino:avr:esplora";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Ethernet"))
            {
                CurrentBoardType = "arduino:avr:ethernet";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Fio"))
            {
                CurrentBoardType = "arduino:avr:fio";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Gemma"))
            {
                CurrentBoardType = "arduino:avr:gemma";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Industrial"))
            {
                CurrentBoardType = "arduino:avr:chiwawa";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Mega ADK"))
            {
                CurrentBoardType = "arduino:avr:megaADK";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Mini"))
            {
                CurrentBoardType = "arduino:avr:mini";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("NG"))
            {
                CurrentBoardType = "arduino:avr:atmegang";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Robot Control"))
            {
                CurrentBoardType = "arduino:avr:robotControl";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Robot Motor"))
            {
                CurrentBoardType = "arduino:avr:robotMotor";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Uno Mini"))
            {
                CurrentBoardType = "arduino:avr:unomini";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Uno WiFi"))
            {
                CurrentBoardType = "arduino:avr:unowifi";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Yún"))
            {
                CurrentBoardType = "arduino:avr:yun";
            }
            else if (deviceType.Contains("Arduino") && deviceType.Contains("Yún Mini"))
            {
                CurrentBoardType = "arduino:avr:yunmini";
            }
            else if (deviceType.Contains("LilyPad Arduino") && !deviceType.Contains("USB"))
            {
                CurrentBoardType = "arduino:avr:lilypad";
            }
            else if (deviceType.Contains("LilyPad Arduino USB"))
            {
                CurrentBoardType = "arduino:avr:LilyPadUSB";
            }
            else if (deviceType.Contains("Linino One"))
            {
                CurrentBoardType = "arduino:avr:one";
            }
            else if (deviceType.Contains("Adafruit Circuit Playground"))
            {
                CurrentBoardType = "arduino:avr:circuitplay32u4cat";
            }
            else if (deviceType.Contains("Arduino"))
            {
                // Default fallback for any Arduino board
                CurrentBoardType = "arduino:avr:uno";
            }
            else
            {
                // Default to Arduino Uno if no specific match
                CurrentBoardType = "arduino:avr:uno";
            }
        }

        public async Task<bool> UploadCode(string code, string portName, IProgress<string> progress = null, string programmer = null)
        {
            try
            {
                // First, detect and validate the connected board
                progress?.Report("[Detecting connected boards...]");
                var connectedBoards = await boardDetectionService.DetectConnectedBoardsAsync();
                
                if (connectedBoards.Any())
                {
                    progress?.Report($"[Found {connectedBoards.Count} connected board(s):]");
                    foreach (var board in connectedBoards)
                    {
                        progress?.Report($"  - {board.BoardName} on {board.PortName}");
                        progress?.Report($"    Type: {board.BoardType}");
                        progress?.Report($"    Vendor: {board.Vendor}");
                        progress?.Report($"    Product: {board.Product}");
                    }

                    // Check if the selected port has a compatible board
                    var targetBoard = connectedBoards.FirstOrDefault(b => b.PortName == portName);
                    if (targetBoard != null)
                    {
                        var compatibilityMessage = boardDetectionService.GetCompatibilityMessage(CurrentBoardType, targetBoard);
                        progress?.Report($"[{compatibilityMessage}]");

                        if (!boardDetectionService.ValidateBoardCompatibility(CurrentBoardType, targetBoard.BoardType))
                        {
                            progress?.Report("[🚫 BLOCKED: Incompatible board detected!]");
                            progress?.Report($"[Selected board type: {CurrentBoardType}]");
                            progress?.Report($"[Detected board: {targetBoard.BoardName} ({targetBoard.BoardType})]");
                            progress?.Report("[This upload has been blocked to prevent damage to your hardware.]");
                            progress?.Report("[Please select the correct board type for your connected hardware.]");
                            
                            // Provide specific recommendations
                            var recommendations = GetBoardRecommendations(targetBoard.BoardType);
                            if (!string.IsNullOrEmpty(recommendations))
                            {
                                progress?.Report($"[Recommended board type: {recommendations}]");
                            }
                            
                            return false; // Block the upload
                        }
                        else
                        {
                            progress?.Report("[✓ Board compatibility verified - proceeding with upload]");
                        }
                    }
                    else
                    {
                        progress?.Report($"[⚠️ No board detected on port {portName}]");
                        progress?.Report("[This may indicate a connection issue or unsupported hardware.]");
                        progress?.Report("[Upload will proceed but may fail.]");
                    }
                }
                else
                {
                    progress?.Report("[⚠️ No boards detected on any COM port]");
                    progress?.Report("[Please check your connections and try again.]");
                    progress?.Report("[Upload will proceed but may fail.]");
                }

                // Robustly find headers.h
                string headersSource = FindHeadersPath();
                string headersDest = Path.Combine(sketchDirectory, "headers.h");

                progress?.Report($"[headers.h source: {headersSource ?? "NOT FOUND"}]");
                progress?.Report($"[headers.h dest: {headersDest}]");

                if (headersSource != null)
                {
                    File.Copy(headersSource, headersDest, true);
                    progress?.Report("[Copied headers.h to sketch directory]");
                }
                else
                {
                    progress?.Report("[FATAL: headers.h not found in any parent directory]");
                    return false;
                }

                // Write the main sketch file
                string sketchFile = Path.Combine(sketchDirectory, "sketch.ino");
                File.WriteAllText(sketchFile, code);

                // List all files in the sketch directory
                var files = Directory.GetFiles(sketchDirectory);
                progress?.Report("[Files in sketch directory:]");
                foreach (var file in files)
                    progress?.Report(file);

                // Abort if headers.h is not present
                if (!File.Exists(headersDest))
                {
                    progress?.Report("[FATAL] Missing headers.h in sketch folder. Aborting build.");
                    return false;
                }

                // Compile the sketch
                progress?.Report("[Compiling sketch...]");
                var compileResult = await RunCliCommand($"compile --fqbn {CurrentBoardType} \"{sketchDirectory}\"");
                if (!compileResult.success)
                {
                    progress?.Report($"[Compilation failed: {compileResult.output}]");
                    return false;
                }

                // Wait for port to become available
                await WaitForPortAvailability(portName, progress);

                // Then upload
                progress?.Report("[Uploading... Please wait]");
                string uploadCmd = $"upload -p {portName} --fqbn {CurrentBoardType} \"{sketchDirectory}\"";
                if (CurrentBoardType.StartsWith("MegaCoreX:megaavr"))
                {
                    if (!string.IsNullOrEmpty(programmer))
                    {
                        string cliProgrammer = MapProgrammerToCli(programmer);
                        if (!string.IsNullOrEmpty(cliProgrammer))
                        {
                            uploadCmd += $" --programmer {cliProgrammer}";
                            progress?.Report($"[Using programmer: {cliProgrammer}]");
                        }
                        else
                        {
                            progress?.Report($"[Warning: Unknown programmer '{programmer}', trying without programmer]");
                        }
                    }
                    else
                    {
                        progress?.Report("[No programmer specified, trying default upload method]");
                    }
                }
                else
                {
                    // For standard Arduino boards, don't use programmer parameter
                    progress?.Report("[Using standard Arduino upload method]");
                }
                var uploadResult = await RunCliCommand(uploadCmd);
                if (!uploadResult.success)
                {
                    progress?.Report($"[Upload failed: {uploadResult.output}]");
                    
                    // Provide specific guidance for ATmega4809 upload issues
                    if (CurrentBoardType.StartsWith("MegaCoreX:megaavr"))
                    {
                        progress?.Report("[ATmega4809 Upload Guidance:]");
                        progress?.Report("- ATmega4809 requires UPDI programming");
                        progress?.Report("- Regular USB-to-serial converters (CH340, CP2102) cannot program ATmega4809");
                        progress?.Report("- You need one of these UPDI programmers:");
                        progress?.Report("  * SerialUPDI adapter (DIY or commercial)");
                        progress?.Report("  * Atmel-ICE with UPDI support");
                        progress?.Report("  * Curiosity Nano");
                        progress?.Report("  * JTAG2UPDI (Arduino Nano as programmer)");
                        progress?.Report("  * PICkit4 with UPDI support");
                        progress?.Report("- For DIY SerialUPDI: Use Arduino Nano with jtag2updi firmware");
                        progress?.Report("- Connect UPDI pin (usually pin 6) to the programmer");
                    }
                    
                    return false;
                }

                progress?.Report("[Upload finished]");
                return true;
            }
            catch (Exception ex)
            {
                progress?.Report($"[Error during upload: {ex.Message}]\n");
                return false;
            }
        }

        private string MapProgrammerToCli(string programmer)
        {
            // Map friendly names to arduino-cli programmer IDs based on actual ATmega4809 support
            switch (programmer)
            {
                // Correct avrdude programmer IDs for MegaCoreX ATmega4809
                case "Atmel-ICE UPDI": return "atmelice_updi";
                case "Curiosity Nano": return "cusiositynano";
                case "JTAG2UPDI": return "jtag2updi";
                case "JTAGICE3 UPDI": return "jtagice3_updi";
                case "microUPDI/Uno Wifi": return "xplainedmini";
                case "MPLAB SNAP UPDI": return "snap_updi";
                case "PICkit4 UPDI": return "pickit4_updi";
                case "PICkit5 UPDI": return "pickit5_updi";
                case "SerialUPDI (115200 baud)": return "serialupdi_115200";
                case "SerialUPDI (230400 baud)": return "serialupdi_230400";
                case "SerialUPDI (460800 baud)": return "serialupdi_460800";
                case "SerialUPDI (57600 baud)": return "serialupdi_57600";
                case "Xplained Pro": return "xplainedmini";
                
                // Alternative mappings that might work
                case "SerialUPDI": return "serialupdi_115200";
                case "UPDI": return "serialupdi_115200";
                case "Serial": return "serialupdi_115200";
                case "Atmel-ICE": return "atmelice_updi";
                case "Curiosity": return "cusiositynano";
                case "JTAG2": return "jtag2updi";
                case "JTAGICE3": return "jtagice3_updi";
                case "microUPDI": return "xplainedmini";
                case "MPLAB SNAP": return "snap_updi";
                case "PICkit4": return "pickit4_updi";
                case "PICkit5": return "pickit5_updi";
                case "Xplained": return "xplainedmini";
                
                // Legacy mappings (keeping for compatibility)
                case "atmel_ice": return "atmelice_updi";
                case "curiosity_nano": return "cusiositynano";
                case "jtag2updi": return "jtag2updi";
                case "jtagice3": return "jtagice3_updi";
                case "microupdi": return "xplainedmini";
                case "mplab_snap": return "snap_updi";
                case "pickit4": return "pickit4_updi";
                case "pickit5": return "pickit5_updi";
                case "serialupdi": return "serialupdi_115200";
                case "xplained_pro": return "xplainedmini";
                
                default: return null;
            }
        }

        private async Task<(bool success, string output)> RunCliCommand(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = arduinoCliPath,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    return (process.ExitCode == 0, output + error);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Add new method to get board detection info
        public async Task<List<ConnectedBoard>> GetConnectedBoardsAsync()
        {
            return await boardDetectionService.DetectConnectedBoardsAsync();
        }

        public async Task<ConnectedBoard> GetBestCompatibleBoardAsync()
        {
            return await boardDetectionService.GetBestCompatibleBoardAsync(CurrentBoardType);
        }

        public string GetCompatibilityMessage(ConnectedBoard board)
        {
            return boardDetectionService.GetCompatibilityMessage(CurrentBoardType, board);
        }

        public bool ValidateBoardCompatibility(string selectedBoardType, string detectedBoardType)
        {
            return boardDetectionService.ValidateBoardCompatibility(selectedBoardType, detectedBoardType);
        }

        private string GetBoardRecommendations(string detectedBoardType)
        {
            if (string.IsNullOrEmpty(detectedBoardType))
                return string.Empty;

            switch (detectedBoardType)
            {
                case "esp32:esp32:esp32":
                    return "ESP32 Dev Module";
                case "esp32:esp32:esp32s3":
                    return "ESP32-S3 Dev Module";
                case "esp32:esp32:esp32c3":
                    return "ESP32-C3 Dev Module";
                case "esp8266:esp8266:nodemcuv2":
                    return "NodeMCU 1.0 (ESP-12E Module)";
                case "esp8266:esp8266:esp01":
                    return "ESP8266 ESP-01";
                case "MegaCoreX:megaavr:4809":
                    return "ATmega4809";
                case "MegaCoreX:megaavr:4808":
                    return "ATmega4808";
                case "MegaCoreX:megaavr:3208":
                    return "ATmega3208";
                case "arduino:avr:uno":
                    return "Arduino Uno";
                case "arduino:avr:nano":
                    return "Arduino Nano";
                case "arduino:avr:mega":
                    return "Arduino Mega";
                default:
                    return string.Empty;
            }
        }
    }
} 