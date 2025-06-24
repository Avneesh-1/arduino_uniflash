/*
 * ESP32 Varying Data Sketch for UniFlash IDE
 * This sketch generates varying TDS and voltage values every time
 * Perfect for testing graphs with changing data
 * 
 * Data Format: $Voltage$ 3.3V $TDS$ 500 $Temp$ 25.5
 * 
 * Pin Connections:
 * - Built-in LED: GPIO 2 (for status indication)
 * - Optional: Connect real sensors to GPIO 36 (TDS) and GPIO 39 (Temperature)
 */

// Pin definitions
const int LED_PIN = 2;         // Built-in LED for status
const int TDS_PIN = 36;        // TDS sensor analog input (optional)
const int TEMP_PIN = 39;       // Temperature sensor analog input (optional)

// Simulation parameters
float baseVoltage = 3.3;
float baseTDS = 500.0;
float baseTemp = 25.5;

// Timing
const unsigned long SEND_INTERVAL = 1000; // Send data every 1 second
unsigned long lastSendTime = 0;
unsigned long startTime = 0;

// Random seed for ESP32
int randomSeedValue = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("ESP32 Varying Data Sketch Starting...");
  Serial.println("Data format: $Voltage$ X.XV $TDS$ XXX $Temp$ XX.X");
  Serial.println("Values will vary every reading for graph testing");
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(TDS_PIN, INPUT);
  pinMode(TEMP_PIN, INPUT);
  
  // Set ADC resolution for ESP32
  analogReadResolution(12); // 12-bit resolution (0-4095)
  
  // Initialize random seed using ESP32's unique chip ID
  randomSeedValue = (int)(ESP.getEfuseMac() >> 32) % 1000;
  randomSeed(randomSeedValue);
  
  // Initial LED state
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
  
  startTime = millis();
  Serial.println("Setup complete. Starting varying data transmission...");
  Serial.printf("Random seed: %d\n", randomSeedValue);
}

void loop() {
  unsigned long currentTime = millis();
  
  // Send data at regular intervals
  if (currentTime - lastSendTime >= SEND_INTERVAL) {
    // Generate varying values
    float voltage = generateVaryingVoltage(currentTime);
    float tdsValue = generateVaryingTDS(currentTime);
    float temperature = generateVaryingTemperature(currentTime);
    
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
      // Send immediate reading with current varying values
      float voltage = generateVaryingVoltage(millis());
      float tdsValue = generateVaryingTDS(millis());
      float temperature = generateVaryingTemperature(millis());
      
      Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                    voltage, tdsValue, temperature);
    }
    else if (command == "real") {
      // Read real sensor values if connected
      float realVoltage = readRealVoltage();
      float realTDS = readRealTDS();
      float realTemp = readRealTemperature();
      
      Serial.printf("$Voltage$ %.2fV $TDS$ %.1f $Temp$ %.1f\n", 
                    realVoltage, realTDS, realTemp);
    }
    else if (command == "status") {
      Serial.println("ESP32 Varying Data Status:");
      Serial.printf("Uptime: %lu seconds\n", (currentTime - startTime) / 1000);
      Serial.printf("Random seed: %d\n", randomSeedValue);
      Serial.printf("Base values: V=%.2fV, TDS=%.1f, Temp=%.1f°C\n", 
                    baseVoltage, baseTDS, baseTemp);
      
      float currentV = generateVaryingVoltage(currentTime);
      float currentTDS = generateVaryingTDS(currentTime);
      float currentTemp = generateVaryingTemperature(currentTime);
      
      Serial.printf("Current varying values: V=%.2fV, TDS=%.1f, Temp=%.1f°C\n", 
                    currentV, currentTDS, currentTemp);
    }
    else if (command == "help") {
      Serial.println("Available commands:");
      Serial.println("  'led on' - Turn LED on");
      Serial.println("  'led off' - Turn LED off");
      Serial.println("  'read' - Send immediate varying reading");
      Serial.println("  'real' - Send real sensor reading (if connected)");
      Serial.println("  'status' - Show current status and values");
      Serial.println("  'help' - Show this help");
    }
    else if (command.length() > 0) {
      Serial.printf("Unknown command: '%s'\n", command.c_str());
      Serial.println("Type 'help' for available commands");
    }
  }
  
  delay(10); // Small delay to prevent watchdog issues
}

float generateVaryingVoltage(unsigned long time) {
  // Generate voltage that varies between 3.0V and 3.6V
  float variation = sin(time / 2000.0) * 0.3; // Slow sine wave
  float noise = (random(-50, 50) / 1000.0);   // Small random noise
  return baseVoltage + variation + noise;
}

float generateVaryingTDS(unsigned long time) {
  // Generate TDS that varies between 300 and 700 ppm
  float variation = sin(time / 1500.0) * 200; // Medium speed sine wave
  float noise = random(-20, 20);              // Random noise
  return baseTDS + variation + noise;
}

float generateVaryingTemperature(unsigned long time) {
  // Generate temperature that varies between 20°C and 30°C
  float variation = sin(time / 3000.0) * 5;   // Slow sine wave
  float noise = (random(-10, 10) / 10.0);     // Small random noise
  return baseTemp + variation + noise;
}

float readRealVoltage() {
  // Read the ESP32 supply voltage (approximate)
  return 3.3; // Simplified - for real voltage measurement you'd need a voltage divider
}

float readRealTDS() {
  // Read real TDS sensor if connected
  int rawValue = analogRead(TDS_PIN);
  
  // Convert to voltage (0-3.3V range)
  float voltage = (rawValue / 4095.0) * 3.3;
  
  // Convert to TDS value (ppm) - simplified conversion
  float tdsValue = (voltage * 1000);
  
  // Ensure reasonable range
  if (tdsValue < 0) tdsValue = 0;
  if (tdsValue > 2000) tdsValue = 2000;
  
  return tdsValue;
}

float readRealTemperature() {
  // Read real temperature sensor if connected
  int rawValue = analogRead(TEMP_PIN);
  
  // Convert to voltage (0-3.3V range)
  float voltage = (rawValue / 4095.0) * 3.3;
  
  // Convert to temperature (Celsius) - assuming LM35 or similar (10mV/°C)
  float temperature = (voltage * 100);
  
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