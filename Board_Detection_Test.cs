using System;
using System.Threading.Tasks;
using UniFlash.IDE;

namespace UniFlash
{
    public class BoardDetectionTest
    {
        public static async Task RunTest()
        {
            Console.WriteLine("=== Enhanced Board Detection Test ===");
            
            var uploader = new ArduinoUploader();
            
            // Test 1: Detect all connected boards
            Console.WriteLine("\n1. Detecting all connected boards...");
            var connectedBoards = await uploader.GetConnectedBoardsAsync();
            
            if (connectedBoards.Count == 0)
            {
                Console.WriteLine("No boards detected. Please connect a board and try again.");
                return;
            }
            
            Console.WriteLine($"Found {connectedBoards.Count} board(s):");
            foreach (var board in connectedBoards)
            {
                Console.WriteLine($"  - {board.BoardName} on {board.PortName}");
                Console.WriteLine($"    Type: {board.BoardType}");
                Console.WriteLine($"    Vendor: {board.Vendor}");
                Console.WriteLine($"    Product: {board.Product}");
            }
            
            // Test 2: Test compatibility with different board types
            Console.WriteLine("\n2. Testing compatibility with different board types...");
            
            string[] testBoardTypes = {
                "arduino:avr:uno",
                "MegaCoreX:megaavr:4809",
                "esp32:esp32:esp32",
                "esp8266:esp8266:nodemcuv2"
            };
            
            foreach (var boardType in testBoardTypes)
            {
                uploader.SetBoardType(boardType);
                Console.WriteLine($"\nTesting compatibility with {boardType}:");
                
                foreach (var board in connectedBoards)
                {
                    var message = uploader.GetCompatibilityMessage(board);
                    Console.WriteLine($"  {message}");
                    
                    // Test if upload would be blocked
                    bool isCompatible = uploader.ValidateBoardCompatibility(boardType, board.BoardType);
                    if (!isCompatible)
                    {
                        Console.WriteLine($"  🚫 Upload would be BLOCKED for this combination");
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ Upload would be ALLOWED for this combination");
                    }
                }
            }
            
            // Test 3: Simulate upload attempts
            Console.WriteLine("\n3. Simulating upload attempts...");
            
            foreach (var board in connectedBoards)
            {
                Console.WriteLine($"\nSimulating upload to {board.BoardName} on {board.PortName}:");
                
                // Test with Arduino Uno selection
                uploader.SetBoardType("arduino:avr:uno");
                bool wouldBlock = !uploader.ValidateBoardCompatibility("arduino:avr:uno", board.BoardType);
                
                if (wouldBlock)
                {
                    Console.WriteLine($"  🚫 BLOCKED: Cannot upload Arduino code to {board.BoardName}");
                    Console.WriteLine($"  Reason: {uploader.GetCompatibilityMessage(board)}");
                }
                else
                {
                    Console.WriteLine($"  ✓ ALLOWED: Arduino code can be uploaded to {board.BoardName}");
                }
                
                // Test with ESP32 selection
                uploader.SetBoardType("esp32:esp32:esp32");
                wouldBlock = !uploader.ValidateBoardCompatibility("esp32:esp32:esp32", board.BoardType);
                
                if (wouldBlock)
                {
                    Console.WriteLine($"  🚫 BLOCKED: Cannot upload ESP32 code to {board.BoardName}");
                    Console.WriteLine($"  Reason: {uploader.GetCompatibilityMessage(board)}");
                }
                else
                {
                    Console.WriteLine($"  ✓ ALLOWED: ESP32 code can be uploaded to {board.BoardName}");
                }
            }
            
            // Test 4: Find best compatible board
            Console.WriteLine("\n4. Finding best compatible board for each type...");
            
            foreach (var boardType in testBoardTypes)
            {
                uploader.SetBoardType(boardType);
                var bestBoard = await uploader.GetBestCompatibleBoardAsync();
                
                if (bestBoard != null)
                {
                    Console.WriteLine($"Best board for {boardType}: {bestBoard.BoardName} on {bestBoard.PortName}");
                }
                else
                {
                    Console.WriteLine($"No compatible board found for {boardType}");
                }
            }
            
            Console.WriteLine("\n=== Test Complete ===");
            Console.WriteLine("\nKey Features Demonstrated:");
            Console.WriteLine("✓ Automatic board detection via USB device info");
            Console.WriteLine("✓ Strict compatibility validation");
            Console.WriteLine("✓ Blocking of incompatible uploads");
            Console.WriteLine("✓ Specific error messages and recommendations");
            Console.WriteLine("✓ Prevention of hardware damage");
        }
    }
} 