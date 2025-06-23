using System;
using System.Threading.Tasks;
using UniFlash.IDE;

namespace UniFlash
{
    public class DebugBoardDetection
    {
        public static async Task RunDebug()
        {
            Console.WriteLine("=== Debug Board Detection ===");
            
            var detectionService = new BoardDetectionService();
            
            // Test 1: Get all connected boards
            Console.WriteLine("\n1. Detecting all connected boards...");
            var connectedBoards = await detectionService.DetectConnectedBoardsAsync();
            
            if (connectedBoards.Count == 0)
            {
                Console.WriteLine("❌ No boards detected!");
                Console.WriteLine("Please check:");
                Console.WriteLine("- Is your ESP32 connected via USB?");
                Console.WriteLine("- Is the USB cable data-enabled (not just charging)?");
                Console.WriteLine("- Are the drivers installed?");
                return;
            }
            
            Console.WriteLine($"✅ Found {connectedBoards.Count} board(s):");
            foreach (var board in connectedBoards)
            {
                Console.WriteLine($"\n📋 Board Details:");
                Console.WriteLine($"   Port: {board.PortName}");
                Console.WriteLine($"   Name: '{board.BoardName}'");
                Console.WriteLine($"   Type: '{board.BoardType}'");
                Console.WriteLine($"   Vendor: '{board.Vendor}'");
                Console.WriteLine($"   Product: '{board.Product}'");
                
                // Test compatibility with Arduino Uno
                bool isCompatibleWithArduino = detectionService.ValidateBoardCompatibility("arduino:avr:uno", board.BoardType);
                Console.WriteLine($"   Compatible with Arduino Uno: {(isCompatibleWithArduino ? "✅ YES" : "❌ NO")}");
                
                if (!isCompatibleWithArduino)
                {
                    Console.WriteLine($"   🚫 This board should BLOCK Arduino uploads!");
                }
            }
            
            // Test 2: Manual WMI query
            Console.WriteLine("\n2. Testing manual WMI query...");
            foreach (var board in connectedBoards)
            {
                Console.WriteLine($"\n🔍 Manual WMI query for {board.PortName}:");
                
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c wmic path Win32_SerialPort where \"DeviceID='{board.PortName}'\" get Caption,Description,Manufacturer /format:csv",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    Console.WriteLine($"WMI Output:\n{output}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            
            // Test 3: Test arduino-cli board list
            Console.WriteLine("\n3. Testing arduino-cli board list...");
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "arduino-cli",
                        Arguments = "board list --format json",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"arduino-cli Output:\n{output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"arduino-cli Error:\n{error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running arduino-cli: {ex.Message}");
            }
            
            Console.WriteLine("\n=== Debug Complete ===");
        }
    }
} 