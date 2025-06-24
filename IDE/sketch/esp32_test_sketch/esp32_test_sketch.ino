/*
 * ESP32 Test Sketch for UniFlash IDE
 * This sketch tests basic ESP32 functionality
 */

// Built-in LED pin for ESP32
const int LED_PIN = 2;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  
  // Initialize LED pin
  pinMode(LED_PIN, OUTPUT);
  
  // Wait for serial to be ready
  delay(1000);
  
  Serial.println("ESP32 Test Sketch Starting...");
  Serial.println("Built-in LED will blink every second");
}

void loop() {
  // Turn LED on
  digitalWrite(LED_PIN, HIGH);
  Serial.println("LED ON");
  delay(1000);
  
  // Turn LED off
  digitalWrite(LED_PIN, LOW);
  Serial.println("LED OFF");
  delay(1000);
} 