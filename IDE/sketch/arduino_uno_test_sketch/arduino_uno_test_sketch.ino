/*
 * Arduino Uno Test Sketch
 * This sketch works with Arduino Uno, Nano, Mega, and other standard boards
 * It does NOT require special programming hardware
 */

// Pin definitions for standard Arduino boards
const int LED_PIN = 13;  // Built-in LED on Arduino Uno/Nano
const int BUTTON_PIN = 2; // Digital input pin

// Variables
int ledState = LOW;
int counter = 0;
unsigned long lastBlinkTime = 0;
const unsigned long BLINK_INTERVAL = 1000; // 1 second

void setup() {
  // Initialize serial communication
  Serial.begin(9600);
  
  // Wait for serial to initialize
  delay(1000);
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(BUTTON_PIN, INPUT_PULLUP);
  
  // Welcome message
  Serial.println("=== Arduino Uno Test Sketch ===");
  Serial.println("This sketch works with standard Arduino boards");
  Serial.println("LED will blink every second");
  Serial.println("Press button on pin 2 to increment counter");
  Serial.println("================================");
  
  // Initial LED flash
  digitalWrite(LED_PIN, HIGH);
  delay(500);
  digitalWrite(LED_PIN, LOW);
}

void loop() {
  // Blink LED every second
  if (millis() - lastBlinkTime >= BLINK_INTERVAL) {
    ledState = !ledState;
    digitalWrite(LED_PIN, ledState);
    lastBlinkTime = millis();
    
    // Print status every 5 seconds
    static int blinkCount = 0;
    blinkCount++;
    if (blinkCount % 5 == 0) {
      Serial.print("System running... Counter: ");
      Serial.println(counter);
    }
  }
  
  // Check button press
  if (digitalRead(BUTTON_PIN) == LOW) {
    // Button pressed (pulled up, so LOW means pressed)
    counter++;
    Serial.print("Button pressed! Counter: ");
    Serial.println(counter);
    
    // Flash LED 3 times
    for (int i = 0; i < 3; i++) {
      digitalWrite(LED_PIN, HIGH);
      delay(100);
      digitalWrite(LED_PIN, LOW);
      delay(100);
    }
    
    // Wait for button release
    while (digitalRead(BUTTON_PIN) == LOW) {
      delay(10);
    }
  }
  
  // Small delay to prevent overwhelming the serial port
  delay(10);
} 