/*
 * ESP32 Sensor Data Simulation for UniFlash Graph
 * Simulates varying Temperature, Current, and TDS values
 * Output format: $Temp$ <value> $Current$ <value> $TDS$ <value>
 */

float t = 0;

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("ESP32 Sensor Data Simulation for UniFlash Graph");
}

void loop() {
  t += 0.1; // time step

  // Simulate smoothly varying values using sine/cosine
  float temperature = 25.0 + 5.0 * sin(t);         // 20°C to 30°C
  float current = 0.5 + 0.1 * cos(t * 0.7);        // 0.4A to 0.6A
  float tds = 300 + 50 * sin(t * 0.5 + 1.0);       // 250ppm to 350ppm

  // Output in UniFlash-compatible format
  Serial.printf("$Temp$ %.2f $Current$ %.3f $TDS$ %.1f\n", temperature, current, tds);

  delay(1000); // Update every 1 second
}