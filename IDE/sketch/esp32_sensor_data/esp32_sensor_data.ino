/*
 * ESP32 Sensor Data Sketch for UniFlash IDE
 * This sketch sends sensor data in the format expected by UniFlash graphs and Excel
 * 
 * Data Format: $Voltage$ 3.3V $TDS$ 500 $Temp$ 25.5
 * 
 * Pin Connections:
 * - TDS Sensor: GPIO 36 (ADC1_CH0)
 * - Temperature Sensor: GPIO 39 (ADC1_CH3) 
 * - Built-in LED: GPIO 2 (for status indication)
 */

// Pin definitions
const int TDS_PIN = 36;        // TDS sensor analog input
const int TEMP_PIN = 39;       // Temperature sensor analog input
const int LED_PIN = 2;         // Built-in LED for status

// Calibration values (adjust these based on your sensors)
const float VREF = 3.3;        // ESP32 reference voltage
const float TEMP_OFFSET = 0.0; // Temperature sensor offset
const float TDS_OFFSET = 0.0;  // TDS sensor offset

// Timing
const unsigned long SEND_INTERVAL = 1000; // Send data every 1 second
unsigned long lastSendTime = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("ESP32 Sensor Data Sketch Starting...");
  Serial.println("Data format: $Voltage$ X.XV $TDS$ XXX $Temp$ XX.X");
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(TDS_PIN, INPUT);
  pinMode(TEMP_PIN, INPUT);
  
  // Set ADC resolution for ESP32
  analogReadResolution(12); // 12-bit resolution (0-4095)
  
  // Initial LED state
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  
  Serial.println("Setup complete. Starting sensor readings...");
}

void loop() {
  unsigned long currentTime = millis();
  
  // Send data at regular intervals
  if (currentTime - lastSendTime >= SEND_INTERVAL) {
    // Read sensor values
    float voltage = readVoltage();
    float tdsValue = readTDS();
    float temperature = readTemperature();
    
    // Send data in UniFlash-compatible format
    Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                  voltage, tdsValue, temperature);
    
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
      float voltage = readVoltage();
      float tdsValue = readTDS();
      float temperature = readTemperature();
      
      Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                    voltage, tdsValue, temperature);
    }
    else if (command == "calibrate") {
      Serial.println("Calibration mode - sending raw values:");
      Serial.printf("Raw TDS: %d\n", analogRead(TDS_PIN));
      Serial.printf("Raw Temp: %d\n", analogRead(TEMP_PIN));
      Serial.printf("Voltage: %.2fV\n", readVoltage());
    }
    else if (command == "help") {
      Serial.println("Available commands:");
      Serial.println("  'led on' - Turn LED on");
      Serial.println("  'led off' - Turn LED off");
      Serial.println("  'read' - Send immediate sensor reading");
      Serial.println("  'calibrate' - Show raw sensor values");
      Serial.println("  'help' - Show this help");
    }
    else if (command.length() > 0) {
      Serial.printf("Unknown command: '%s'\n", command.c_str());
      Serial.println("Type 'help' for available commands");
    }
  }
  
  delay(10); // Small delay to prevent watchdog issues
}

float readVoltage() {
  // Read the ESP32 supply voltage (approximate)
  // This is a simplified reading - for more accurate voltage measurement
  // you would need a voltage divider circuit
  return VREF;
}

float readTDS() {
  // Read TDS sensor value
  int rawValue = analogRead(TDS_PIN);
  
  // Convert to voltage (0-3.3V range)
  float voltage = (rawValue / 4095.0) * VREF;
  
  // Convert to TDS value (ppm) - this is a simplified conversion
  // You may need to calibrate this based on your specific sensor
  float tdsValue = (voltage * 1000) + TDS_OFFSET;
  
  // Ensure reasonable range
  if (tdsValue < 0) tdsValue = 0;
  if (tdsValue > 2000) tdsValue = 2000;
  
  return tdsValue;
}

float readTemperature() {
  // Read temperature sensor value
  int rawValue = analogRead(TEMP_PIN);
  
  // Convert to voltage (0-3.3V range)
  float voltage = (rawValue / 4095.0) * VREF;
  
  // Convert to temperature (Celsius) - this is a simplified conversion
  // You may need to calibrate this based on your specific sensor
  // Assuming LM35 or similar sensor (10mV/°C)
  float temperature = (voltage * 100) + TEMP_OFFSET;
  
  // Ensure reasonable range
  if (temperature < -40) temperature = -40;
  if (temperature > 125) temperature = 125;
  
  return temperature;
}

/*
 * Alternative data format for CSV compatibility:
 * Uncomment the line below and comment out the Serial.printf line in loop()
 * to send data in CSV format instead
 */
// Serial.printf("%.2f,%.1f,%.1f\n", voltage, tdsValue, temperature); 