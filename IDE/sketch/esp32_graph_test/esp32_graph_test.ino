/*
 * ESP32 Graph Test Sketch for UniFlash IDE
 * This sketch sends test data in the exact format expected by UniFlash graphs
 * 
 * Data Format: $Voltage$ 3.3V $TDS$ 500 $Temp$ 25.5
 * 
 * This is a simple test to verify that ESP32 data appears in graphs and Excel
 */

// Pin definitions
const int LED_PIN = 2;         // Built-in LED for status

// Test data simulation
float testVoltage = 3.3;
float testTDS = 500.0;
float testTemp = 25.5;

// Timing
const unsigned long SEND_INTERVAL = 1000; // Send data every 1 second
unsigned long lastSendTime = 0;
unsigned long startTime = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("ESP32 Graph Test Sketch Starting...");
  Serial.println("Data format: $Voltage$ X.XV $TDS$ XXX $Temp$ XX.X");
  Serial.println("This will send test data every second for graph testing");
  
  // Initialize LED pin
  pinMode(LED_PIN, OUTPUT);
  
  // Initial LED state
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  
  startTime = millis();
  Serial.println("Setup complete. Starting test data transmission...");
}

void loop() {
  unsigned long currentTime = millis();
  
  // Send data at regular intervals
  if (currentTime - lastSendTime >= SEND_INTERVAL) {
    // Simulate changing values
    testVoltage = 3.3 + (sin(currentTime / 1000.0) * 0.1); // Vary between 3.2-3.4V
    testTDS = 500.0 + (sin(currentTime / 2000.0) * 100);   // Vary between 400-600 ppm
    testTemp = 25.5 + (sin(currentTime / 3000.0) * 5);     // Vary between 20.5-30.5°C
    
    // Send data in UniFlash-compatible format
    Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                  testVoltage, testTDS, testTemp);
    
    // Blink LED to indicate data transmission
    digitalWrite(LED_PIN, HIGH);
    delay(50);
    digitalWrite(LED_PIN, LOW);
    
    lastSendTime = currentTime;
  }
  
  // Handle serial commands
  if (Serial.available()) {
    String command = Serial.readStringUntil('\n');
    command.trim();
    command.toLowerCase();
    
    if (command == "led on") {
      digitalWrite(LED_PIN, HIGH);
      Serial.println("LED turned ON");
    }
    else if (command == "led off") {
      digitalWrite(LED_PIN, LOW);
      Serial.println("LED turned OFF");
    }
    else if (command == "read") {
      // Send immediate reading
      Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                    testVoltage, testTDS, testTemp);
    }
    else if (command == "csv") {
      // Send data in CSV format
      Serial.printf("%.2f,%.1f,%.1f\n", testVoltage, testTDS, testTemp);
    }
    else if (command == "status") {
      Serial.println("ESP32 Graph Test Status:");
      Serial.printf("Uptime: %lu seconds\n", (currentTime - startTime) / 1000);
      Serial.printf("Current values: V=%.2fV, TDS=%.1f, Temp=%.1f°C\n", 
                    testVoltage, testTDS, testTemp);
      Serial.printf("Data format: $Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                    testVoltage, testTDS, testTemp);
    }
    else if (command == "help") {
      Serial.println("Available commands:");
      Serial.println("  'led on' - Turn LED on");
      Serial.println("  'led off' - Turn LED off");
      Serial.println("  'read' - Send immediate sensor reading");
      Serial.println("  'csv' - Send data in CSV format");
      Serial.println("  'status' - Show current status");
      Serial.println("  'help' - Show this help");
    }
    else if (command.length() > 0) {
      Serial.printf("Unknown command: '%s'\n", command.c_str());
      Serial.println("Type 'help' for available commands");
    }
  }
  
  delay(10); // Small delay to prevent watchdog issues
} 