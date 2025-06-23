/*
 * Hello World Arduino Example
 * Simple example to test your Arduino setup
 */

void setup() {
  // Initialize serial communication at 9600 baud rate
  Serial.begin(9600);
  
  // Wait a moment for serial to initialize
  delay(1000);
  
  // Print welcome message
  Serial.println("Hello World from Arduino!");
  Serial.println("This is a simple test sketch.");
  Serial.println("If you can see this message, your Arduino is working!");
}

void loop() {
  // Print a message every 2 seconds
  Serial.println("Arduino is running...");
  
  // Wait 2 seconds before next message
  delay(2000);
} 