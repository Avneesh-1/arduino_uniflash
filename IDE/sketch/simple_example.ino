/*
 * Simple Arduino Example for UniFlash IDE
 * This sketch demonstrates basic Arduino functionality
 */

// Pin definitions
const int LED_PIN = 13;  // Built-in LED on most Arduino boards
const int BUTTON_PIN = 2; // Digital input pin for button

// Variables
int ledState = LOW;
int buttonState = 0;
int lastButtonState = 0;
unsigned long lastDebounceTime = 0;
unsigned long debounceDelay = 50;
int counter = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(9600);
  Serial.println("Simple Arduino Example Starting...");
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(BUTTON_PIN, INPUT_PULLUP);
  
  // Turn on LED initially
  digitalWrite(LED_PIN, HIGH);
  delay(1000);
  digitalWrite(LED_PIN, LOW);
  
  Serial.println("Setup complete! LED will blink every second.");
  Serial.println("Press button to increment counter.");
}

void loop() {
  // Blink LED every second
  static unsigned long lastBlinkTime = 0;
  if (millis() - lastBlinkTime >= 1000) {
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
  
  // Button handling with debouncing
  int reading = digitalRead(BUTTON_PIN);
  
  if (reading != lastButtonState) {
    lastDebounceTime = millis();
  }
  
  if ((millis() - lastDebounceTime) > debounceDelay) {
    if (reading != buttonState) {
      buttonState = reading;
      
      if (buttonState == LOW) { // Button pressed (pulled up)
        counter++;
        Serial.print("Button pressed! Counter: ");
        Serial.println(counter);
        
        // Flash LED 3 times when button is pressed
        for (int i = 0; i < 3; i++) {
          digitalWrite(LED_PIN, HIGH);
          delay(100);
          digitalWrite(LED_PIN, LOW);
          delay(100);
        }
      }
    }
  }
  
  lastButtonState = reading;
  
  // Small delay to prevent overwhelming the serial port
  delay(10);
} 