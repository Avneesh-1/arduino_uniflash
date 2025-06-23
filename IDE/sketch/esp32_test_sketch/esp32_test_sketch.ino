/*
 * ESP32 Test Sketch
 * Basic functionality test for ESP32 WROOM
 */

// Pin definitions
const int LED_PIN = 2;        // Built-in LED
const int BUTTON_PIN = 4;     // Test button pin
const int ANALOG_PIN = 34;    // Analog input pin

// Variables
int buttonState = 0;
int lastButtonState = 0;
unsigned long lastDebounceTime = 0;
unsigned long debounceDelay = 50;
int buttonPressCount = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  delay(1000);
  
  Serial.println("ESP32 Test Sketch Started");
  Serial.println("==========================");
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(BUTTON_PIN, INPUT_PULLUP);
  
  // Initial LED state
  digitalWrite(LED_PIN, LOW);
  
  Serial.println("Setup complete. System ready.");
  Serial.println("Commands:");
  Serial.println("  'led on' - Turn LED on");
  Serial.println("  'led off' - Turn LED off");
  Serial.println("  'blink' - Blink LED 5 times");
  Serial.println("  'analog' - Read analog value");
  Serial.println("  'info' - Show system info");
}

void loop() {
  // Handle button input with debouncing
  handleButton();
  
  // Handle serial commands
  handleSerialCommands();
  
  // Read and display analog value every 3 seconds
  static unsigned long lastAnalogRead = 0;
  if (millis() - lastAnalogRead > 3000) {
    int analogValue = analogRead(ANALOG_PIN);
    Serial.printf("Analog Pin %d: %d\n", ANALOG_PIN, analogValue);
    lastAnalogRead = millis();
  }
  
  delay(10); // Small delay to prevent watchdog issues
}

void handleButton() {
  // Read the button state
  int reading = digitalRead(BUTTON_PIN);
  
  // If the button state changed, reset the debouncing timer
  if (reading != lastButtonState) {
    lastDebounceTime = millis();
  }
  
  // If enough time has passed since the last change
  if ((millis() - lastDebounceTime) > debounceDelay) {
    // If the button state has changed
    if (reading != buttonState) {
      buttonState = reading;
      
      // If button is pressed (LOW due to INPUT_PULLUP)
      if (buttonState == LOW) {
        buttonPressCount++;
        Serial.printf("Button pressed! Count: %d\n", buttonPressCount);
        
        // Toggle LED on button press
        digitalWrite(LED_PIN, !digitalRead(LED_PIN));
      }
    }
  }
  
  lastButtonState = reading;
}

void handleSerialCommands() {
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
    else if (command == "blink") {
      Serial.println("Blinking LED 5 times...");
      for (int i = 0; i < 5; i++) {
        digitalWrite(LED_PIN, HIGH);
        delay(200);
        digitalWrite(LED_PIN, LOW);
        delay(200);
      }
      Serial.println("Blink complete");
    }
    else if (command == "analog") {
      int value = analogRead(ANALOG_PIN);
      Serial.printf("Analog Pin %d: %d\n", ANALOG_PIN, value);
    }
    else if (command == "info") {
      showSystemInfo();
    }
    else if (command == "help") {
      Serial.println("Available commands:");
      Serial.println("  'led on' - Turn LED on");
      Serial.println("  'led off' - Turn LED off");
      Serial.println("  'blink' - Blink LED 5 times");
      Serial.println("  'analog' - Read analog value");
      Serial.println("  'info' - Show system info");
      Serial.println("  'help' - Show this help");
    }
    else if (command.length() > 0) {
      Serial.printf("Unknown command: '%s'\n", command.c_str());
      Serial.println("Type 'help' for available commands");
    }
  }
}

void showSystemInfo() {
  Serial.println("\n=== System Information ===");
  Serial.printf("Free Heap: %d bytes\n", ESP.getFreeHeap());
  Serial.printf("Chip ID: %06X\n", ESP.getChipId());
  Serial.printf("Flash Chip ID: %06X\n", ESP.getFlashChipId());
  Serial.printf("Flash Chip Size: %d bytes\n", ESP.getFlashChipSize());
  Serial.printf("CPU Frequency: %d MHz\n", ESP.getCpuFreqMHz());
  Serial.printf("Button Press Count: %d\n", buttonPressCount);
  Serial.printf("Uptime: %lu seconds\n", millis() / 1000);
  Serial.println("==========================\n");
} 