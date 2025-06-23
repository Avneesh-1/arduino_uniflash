#include "headers.h"

// Simple test sketch for ATmega4809
// This sketch will work with both Arduino Uno and ATmega4809

void setup() {
  // Initialize main serial port
  Serial.begin(SERIAL_BAUD);
  
  // Initialize pins
  pinMode(LED_BUILTIN, OUTPUT);
  
  Serial.println("ATmega4809 Test Sketch");
  Serial.println("=====================");
  Serial.println("Built-in LED will blink every second");
  Serial.println("Serial communication is working!");
}

void loop() {
  // Blink the built-in LED
  digitalWrite(LED_BUILTIN, HIGH);
  delay(1000);
  digitalWrite(LED_BUILTIN, LOW);
  delay(1000);
  
  // Send a test message
  Serial.println("Hello from ATmega4809!");
  
  // Simulate sensor data (for testing the main application)
  Serial.print("$Voltage$ :");
  Serial.println("4.5");
  
  Serial.print("$TDS$ :");
  Serial.println("150");
  
  Serial.print("$Temp$ :");
  Serial.println("25.5");
} 